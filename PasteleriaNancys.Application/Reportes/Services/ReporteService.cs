using PasteleriaNancys.Application.Caja.Interfaces;
using PasteleriaNancys.Application.Inventario;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Application.Pedidos.Interfaces;
using PasteleriaNancys.Application.Reportes.Dtos;
using PasteleriaNancys.Application.Reportes.Interfaces;
using PasteleriaNancys.Domain.Inventario;
using PasteleriaNancys.Domain.Pedidos;

namespace PasteleriaNancys.Application.Reportes.Services
{
    public class ReporteService : IReporteService
    {
        private const string UbicacionVitrina = "Mercado Campesino";

        private readonly IItemCatalogoRepository _itemCatalogoRepository;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly ILoteRepository _loteRepository;
        private readonly IStockMinimoRepository _stockMinimoRepository;
        private readonly IVentaRepository _ventaRepository;
        private readonly IEventoFestivoRepository _eventoFestivoRepository;
        private readonly IRecetaRepository _recetaRepository;
        private readonly IHorneadaRepository _horneadaRepository;

        public ReporteService(
            IItemCatalogoRepository itemCatalogoRepository,
            IPedidoRepository pedidoRepository,
            ILoteRepository loteRepository,
            IStockMinimoRepository stockMinimoRepository,
            IVentaRepository ventaRepository,
            IEventoFestivoRepository eventoFestivoRepository,
            IRecetaRepository recetaRepository,
            IHorneadaRepository horneadaRepository)
        {
            _itemCatalogoRepository = itemCatalogoRepository;
            _pedidoRepository = pedidoRepository;
            _loteRepository = loteRepository;
            _stockMinimoRepository = stockMinimoRepository;
            _ventaRepository = ventaRepository;
            _eventoFestivoRepository = eventoFestivoRepository;
            _recetaRepository = recetaRepository;
            _horneadaRepository = horneadaRepository;
        }

        public async Task<ProyeccionHorneadoDto> ObtenerProyeccionHorneadoAsync()
        {
            var ahora = DateTime.UtcNow;

            var todosLosItems = await _itemCatalogoRepository.ObtenerTodosAsync();
            var productos = todosLosItems
                .Where(i => i.Tipo == "Terminado" && i.Activo)
                .ToList();

            var pedidosPendientes = await _pedidoRepository.ObtenerPendientesAsync();

            var pedidosPorProducto = pedidosPendientes
                .Where(p => p.Configuracion?.IdItemProductoBase is not null)
                .GroupBy(p => p.Configuracion!.IdItemProductoBase!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            var eventoProximo = await _eventoFestivoRepository.ObtenerProximoAsync(ahora, ahora.AddDays(7));
            var multiplicador = eventoProximo?.MultiplicadorDemanda ?? 1m;

            var resultado = new List<ProyeccionProductoDto>();

            foreach (var producto in productos)
            {
                var unidadesPedidos = pedidosPorProducto.TryGetValue(producto.Id, out var conteo) ? conteo : 0;

                var stockActualVitrina = await _loteRepository.ObtenerStockDisponiblePorUbicacionAsync(producto.Id, UbicacionVitrina);
                var stockMinimo = await _stockMinimoRepository.ObtenerPorItemAsync(producto.Id);
                var meta = stockMinimo is not null && stockMinimo.Activo ? stockMinimo.CantidadMinima : 0m;
                var stockFaltante = Math.Max(0m, meta - stockActualVitrina);

                var cantidadVendida30Dias = await _ventaRepository.ObtenerCantidadVendidaAsync(producto.Id, ahora.AddDays(-30), ahora);
                var promedioVentasDiarias = cantidadVendida30Dias / 30m;

                var componenteDemanda = promedioVentasDiarias * multiplicador;
                var totalHornear = (int)Math.Ceiling(unidadesPedidos + stockFaltante + componenteDemanda);

                resultado.Add(new ProyeccionProductoDto
                {
                    IdItem = producto.Id,
                    Nombre = producto.Nombre,
                    UnidadesPedidosConfirmados = unidadesPedidos,
                    StockFaltanteVitrina = stockFaltante,
                    PromedioVentasDiarias = promedioVentasDiarias,
                    MultiplicadorAplicado = multiplicador,
                    NombreEventoFestivo = eventoProximo?.NombreEvento,
                    TotalHornear = totalHornear
                });
            }

            var insumosNecesarios = await ExplotarInsumosNecesariosAsync(resultado);
            var recomendacionSabores = await CalcularRecomendacionSaboresAsync(resultado, todosLosItems, pedidosPendientes, ahora);
            var masVendidosHoy = await ObtenerMasVendidosHoyAsync(ahora, todosLosItems);

            return new ProyeccionHorneadoDto
            {
                Productos = resultado,
                InsumosNecesarios = insumosNecesarios,
                RecomendacionSabores = recomendacionSabores,
                MasVendidosHoy = masVendidosHoy
            };
        }

        // RF-12 ampliado (2026-07-13): el usuario pidió que la proyección detecte cuándo la
        // producción "estándar" de un día (solo la última batida sale mitad-y-mitad, el resto
        // 100% vainilla) no alcanza para cubrir la demanda real de chocolate. Cada torta se arma
        // con 2 bizcochos (capas): una pura gasta 2 del mismo sabor, una mixta gasta 1 de cada uno
        // (regla de negocio confirmada por el usuario, no inventada).
        //
        // La demanda de sabor viene de DOS fuentes que no se pisan entre sí:
        //  - Vitrina (recetas fijas, ItemCatalogo.TipoMasa): se usa el TotalHornear ya calculado
        //    por producto (pedidos + faltante de vitrina + tendencia), multiplicado por sabor.
        //  - Personalizables (Redonda/Cuadrada/Corazón a tu gusto): su producto base SIEMPRE tiene
        //    TipoMasa=null (es EsPersonalizable), así que el sabor elegido solo vive en
        //    Pedido_Configuracion.TipoMasa de cada pedido pendiente — se cuenta aparte.
        //
        // "Cubiertos" es dinámico, no un número fijo (pedido explícito del usuario, 2026-07-13:
        // que la recomendación "se limpie" cuando ya confirmaron/registraron la horneada del día):
        //  - Si TODAVÍA no se registró ninguna Horneada hoy, se asume el plan estándar (10, de la
        //    mixta que siempre se hace) — vista de planificación, antes de hornear.
        //  - Si YA se registró una o más Horneadas hoy, se usa lo que EFECTIVAMENTE se horneó
        //    (incluyendo cualquier batida extra de chocolate ya registrada) en vez de la asunción
        //    — así, si el encargado siguió la recomendación, el faltante baja a 0 y el banner
        //    pasa a verde solo.
        private async Task<RecomendacionSaboresDto> CalcularRecomendacionSaboresAsync(
            List<ProyeccionProductoDto> productosProyectados,
            List<ItemCatalogo> todosLosItems,
            List<PedidoCliente> pedidosPendientes,
            DateTime ahora)
        {
            var itemsPorId = todosLosItems.ToDictionary(i => i.Id);

            var tortasChocolate = 0;
            var tortasVainilla = 0;
            var tortasMixtas = 0;

            foreach (var p in productosProyectados)
            {
                if (p.TotalHornear <= 0 || !itemsPorId.TryGetValue(p.IdItem, out var item))
                {
                    continue;
                }

                switch (item.TipoMasa)
                {
                    case "Chocolate": tortasChocolate += p.TotalHornear; break;
                    case "Vainilla": tortasVainilla += p.TotalHornear; break;
                    case "Mixto": tortasMixtas += p.TotalHornear; break;
                }
            }

            foreach (var pedido in pedidosPendientes)
            {
                switch (pedido.Configuracion?.TipoMasa)
                {
                    case "Chocolate": tortasChocolate++; break;
                    case "Vainilla": tortasVainilla++; break;
                    case "Mixto": tortasMixtas++; break;
                }
            }

            var bizcochosChocolate = tortasChocolate * ProduccionConstantes.BizcochosPorTorta + tortasMixtas;
            var bizcochosVainilla = tortasVainilla * ProduccionConstantes.BizcochosPorTorta + tortasMixtas;

            var horneadasHoy = (await _horneadaRepository.ObtenerTodosAsync())
                .Where(h => h.Fecha.Date == ahora.Date)
                .ToList();
            var huboHorneadaHoy = horneadasHoy.Count > 0;
            var bizcochosChocolateProducidosHoy = horneadasHoy.Sum(h =>
                ProduccionConstantes.BizcochosChocolatePorHorneadaEstandar
                + h.CantidadBatidasChocolateExtra * ProduccionConstantes.BizcochosPorBatida);

            var cubiertos = huboHorneadaHoy
                ? bizcochosChocolateProducidosHoy
                : ProduccionConstantes.BizcochosChocolatePorHorneadaEstandar;
            var faltante = Math.Max(0, bizcochosChocolate - cubiertos);
            var batidasRecomendadas = (int)Math.Ceiling(faltante / (decimal)ProduccionConstantes.BizcochosPorBatida);

            string mensaje;
            if (faltante > 0 && huboHorneadaHoy)
            {
                mensaje = $"Ya hornearon {bizcochosChocolateProducidosHoy} bizcocho(s) de chocolate hoy, pero la demanda es de {bizcochosChocolate} ({tortasChocolate} torta(s) pura(s) + {tortasMixtas} mixta(s)). Todavía faltan {faltante} — recomendado: {batidasRecomendadas} batida(s) completa(s) de chocolate más.";
            }
            else if (faltante > 0)
            {
                mensaje = $"Se necesitan {bizcochosChocolate} bizcochos de chocolate ({tortasChocolate} torta(s) pura(s) + {tortasMixtas} mixta(s)), pero la batida mixta estándar del día solo cubre {cubiertos}. Recomendado: {batidasRecomendadas} batida(s) completa(s) de chocolate adicionales, en vez de solo la mitad-y-mitad de siempre.";
            }
            else if (huboHorneadaHoy)
            {
                mensaje = $"La demanda de chocolate ({bizcochosChocolate} bizcocho(s)) ya está cubierta con lo que hornearon hoy ({bizcochosChocolateProducidosHoy}).";
            }
            else
            {
                mensaje = $"La demanda de chocolate ({bizcochosChocolate} bizcocho(s)) está cubierta por la batida mixta estándar del día ({cubiertos}).";
            }

            return new RecomendacionSaboresDto
            {
                TortasChocolatePuroPendientes = tortasChocolate,
                TortasVainillaPuraPendientes = tortasVainilla,
                TortasMixtasPendientes = tortasMixtas,
                BizcochosChocolateNecesarios = bizcochosChocolate,
                BizcochosVainillaNecesarios = bizcochosVainilla,
                BizcochosChocolateCubiertosPorBatidaEstandar = cubiertos,
                HuboHorneadaRegistradaHoy = huboHorneadaHoy,
                BizcochosChocolateProducidosHoy = bizcochosChocolateProducidosHoy,
                FaltanteBizcochosChocolate = faltante,
                BatidasCompletasChocolateRecomendadas = batidasRecomendadas,
                RequiereBatidaCompletaChocolate = faltante > 0,
                Mensaje = mensaje
            };
        }

        // Reposición de vitrina basada en lo que de verdad se vendió HOY (velocidad real), a
        // diferencia del promedio de 30 días que ya usa la proyección por producto — pedido
        // explícito del usuario para "reponer y hacer bizcochos de las tortas que más se
        // vendieron en el día", medido por unidades (no por ingresos, confirmado por el usuario).
        private async Task<List<ProductoMasVendidoHoyDto>> ObtenerMasVendidosHoyAsync(
            DateTime ahora, List<ItemCatalogo> todosLosItems)
        {
            var ventasHoy = await _ventaRepository.ObtenerPorRangoAsync(ahora.Date, ahora);

            var cantidadPorItem = new Dictionary<Guid, decimal>();
            foreach (var detalle in ventasHoy.SelectMany(v => v.Detalles))
            {
                cantidadPorItem[detalle.IdItem] = cantidadPorItem.GetValueOrDefault(detalle.IdItem) + detalle.Cantidad;
            }

            var itemsPorId = todosLosItems.ToDictionary(i => i.Id);
            var resultado = new List<ProductoMasVendidoHoyDto>();

            foreach (var (idItem, cantidad) in cantidadPorItem.OrderByDescending(kv => kv.Value).Take(5))
            {
                if (!itemsPorId.TryGetValue(idItem, out var item))
                {
                    continue;
                }

                var stockActual = await _loteRepository.ObtenerStockDisponiblePorUbicacionAsync(idItem, UbicacionVitrina);
                var stockMinimo = await _stockMinimoRepository.ObtenerPorItemAsync(idItem);
                var meta = stockMinimo is not null && stockMinimo.Activo ? stockMinimo.CantidadMinima : 0m;

                resultado.Add(new ProductoMasVendidoHoyDto
                {
                    IdItem = idItem,
                    Nombre = item.Nombre,
                    CantidadVendidaHoy = cantidad,
                    StockActualVitrina = stockActual,
                    RecomiendaReponer = stockActual <= meta
                });
            }

            return resultado;
        }

        // Convierte "cuántas tortas hornear" en "cuánto insumo comprar", usando la Receta_Item
        // de cada torta (cantidad de insumo por unidad) y sumando entre tortas que comparten
        // el mismo insumo (ej. harina en varias recetas).
        private async Task<List<InsumoRequeridoDto>> ExplotarInsumosNecesariosAsync(List<ProyeccionProductoDto> productos)
        {
            var necesidadPorInsumo = new Dictionary<Guid, decimal>();

            foreach (var producto in productos.Where(p => p.TotalHornear > 0))
            {
                var receta = await _recetaRepository.ObtenerPorProductoTerminadoAsync(producto.IdItem);
                foreach (var recetaItem in receta)
                {
                    var cantidad = recetaItem.CantidadRequerida * producto.TotalHornear;
                    necesidadPorInsumo[recetaItem.IdItemInsumo] =
                        necesidadPorInsumo.GetValueOrDefault(recetaItem.IdItemInsumo) + cantidad;
                }
            }

            var resultado = new List<InsumoRequeridoDto>();
            foreach (var (idInsumo, cantidadNecesaria) in necesidadPorInsumo)
            {
                var insumo = await _itemCatalogoRepository.ObtenerPorIdAsync(idInsumo);
                if (insumo is null)
                {
                    continue;
                }

                var stockDisponible = await _loteRepository.ObtenerStockDisponibleTotalAsync(idInsumo);
                resultado.Add(new InsumoRequeridoDto
                {
                    IdItem = insumo.Id,
                    CodigoReferencia = insumo.CodigoReferencia,
                    Nombre = insumo.Nombre,
                    UnidadMedida = insumo.UnidadMedida,
                    CantidadNecesaria = cantidadNecesaria,
                    StockDisponible = stockDisponible,
                    CantidadFaltante = Math.Max(0m, cantidadNecesaria - stockDisponible)
                });
            }

            return resultado.OrderByDescending(i => i.CantidadFaltante).ToList();
        }
    }
}
