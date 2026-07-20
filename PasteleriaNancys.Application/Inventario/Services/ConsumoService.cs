using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Services
{
    public class ConsumoService : IConsumoService
    {
        // Ubicación de producción — mismo valor que DespachoService.UbicacionOrigen. Las batidas,
        // el descuento automático por receta y el consumo manual se descuentan del stock de
        // San Mateo, no de la vitrina de venta (Mercado Campesino).
        private const string UbicacionProduccion = "San Mateo";

        // La fórmula de la batida ya NO vive hardcodeada aquí: es la Receta_Item de los ítems
        // Intermedios Bizcocho de Vainilla / Bizcocho de Chocolate, expresada POR PORCIÓN
        // (batida real ÷ 200 porciones — sembrada por la migración con los datos del usuario,
        // incluidos los 25 huevos fijos por batida = 0.125 huevo/porción y la caramelina del
        // chocolate = 0.0005 kg/porción, que reproduce exactamente los montos anteriores:
        // 0.050 kg por horneada estándar y 0.100 kg por batida 100% chocolate extra).

        private readonly IHorneadaRepository _horneadaRepository;
        private readonly IConsumoInsumoRepository _consumoRepository;
        private readonly IItemCatalogoRepository _itemCatalogoRepository;
        private readonly ILoteRepository _loteRepository;
        private readonly IRecetaRepository _recetaRepository;

        public ConsumoService(
            IHorneadaRepository horneadaRepository,
            IConsumoInsumoRepository consumoRepository,
            IItemCatalogoRepository itemCatalogoRepository,
            ILoteRepository loteRepository,
            IRecetaRepository recetaRepository)
        {
            _horneadaRepository = horneadaRepository;
            _consumoRepository = consumoRepository;
            _itemCatalogoRepository = itemCatalogoRepository;
            _loteRepository = loteRepository;
            _recetaRepository = recetaRepository;
        }

        public async Task<HorneadaDto> RegistrarHorneadaAsync(Guid idUsuarioRegistro, RegistrarHorneadaRequest request)
        {
            if (request.CantidadBatidas <= 0)
            {
                throw new ReglaNegocioException("La cantidad de batidas debe ser mayor a 0.");
            }

            // La mixta estándar SIEMPRE existe (1 de las batidas del día) — el resto puede
            // repartirse entre extra-chocolate y vainilla, pero nunca puede haber más batidas
            // extra de chocolate que "batidas totales - 1".
            if (request.CantidadBatidasChocolateExtra < 0 ||
                request.CantidadBatidasChocolateExtra > request.CantidadBatidas - 1)
            {
                throw new ReglaNegocioException(
                    $"Las batidas 100% chocolate no pueden superar {request.CantidadBatidas - 1} " +
                    "(la mixta estándar ya cuenta como una de las batidas del día).");
            }

            var porcionesChocolate =
                (ProduccionConstantes.BizcochosChocolatePorHorneadaEstandar
                    + request.CantidadBatidasChocolateExtra * ProduccionConstantes.BizcochosPorBatida)
                * ProduccionConstantes.PorcionesPorBizcocho;
            var porcionesVainilla =
                request.CantidadBatidas * ProduccionConstantes.PorcionesPorBatida - porcionesChocolate;

            var horneada = new Horneada
            {
                Id = Guid.NewGuid(),
                Fecha = DateTime.UtcNow.Date,
                CantidadBatidas = request.CantidadBatidas,
                CantidadBatidasChocolateExtra = request.CantidadBatidasChocolateExtra,
                IdUsuarioRegistro = idUsuarioRegistro,
                FechaRegistro = DateTime.UtcNow
            };

            await _horneadaRepository.AgregarAsync(horneada);

            var bizcochoVainilla = await ObtenerBizcochoAsync(ProduccionConstantes.CodigoBizcochoVainilla);
            var bizcochoChocolate = await ObtenerBizcochoAsync(ProduccionConstantes.CodigoBizcochoChocolate);

            // Suma la receta de cada sabor multiplicada por sus porciones (los insumos base
            // compartidos —harina, azúcar, huevo— se agregan en un solo descuento PEPS por insumo).
            var necesidadPorInsumo = new Dictionary<Guid, decimal>();
            await AcumularRecetaAsync(necesidadPorInsumo, bizcochoVainilla, porcionesVainilla);
            await AcumularRecetaAsync(necesidadPorInsumo, bizcochoChocolate, porcionesChocolate);

            var consumos = new List<ConsumoInsumo>();
            foreach (var (idInsumo, cantidad) in necesidadPorInsumo)
            {
                var insumo = await _itemCatalogoRepository.ObtenerPorIdAsync(idInsumo)
                    ?? throw new ReglaNegocioException(
                        "La receta del bizcocho referencia un insumo que ya no existe en el catálogo.");
                consumos.AddRange(await DescontarStockAsync(insumo, cantidad, horneada.Id, idUsuarioRegistro, null, null));
            }

            foreach (var consumo in consumos)
            {
                await _consumoRepository.AgregarAsync(consumo);
            }

            // La horneada PRODUCE stock de bizcocho: lotes PEPS de porciones en San Mateo, que
            // luego el armado de tortas (Viaje/Despacho) descuenta automáticamente por receta.
            await CrearLoteBizcochoAsync(bizcochoVainilla, porcionesVainilla);
            await CrearLoteBizcochoAsync(bizcochoChocolate, porcionesChocolate);

            await _horneadaRepository.GuardarCambiosAsync();

            horneada.Consumos = consumos;
            return MapearHorneadaDto(horneada);
        }

        public async Task<List<HorneadaDto>> ObtenerHorneadasAsync()
        {
            var horneadas = await _horneadaRepository.ObtenerTodosAsync();
            return horneadas.OrderByDescending(h => h.FechaRegistro).Select(MapearHorneadaDto).ToList();
        }

        public async Task<ConsumoInsumoDto> RegistrarConsumoManualAsync(Guid idUsuarioRegistro, RegistrarConsumoRequest request)
        {
            if (request.Cantidad <= 0)
            {
                throw new ReglaNegocioException("La cantidad consumida debe ser mayor a 0.");
            }

            var item = await _itemCatalogoRepository.ObtenerPorIdAsync(request.IdItem)
                ?? throw new NoEncontradoException($"No se encontró el ítem con id {request.IdItem}.");

            if (item.Tipo == "Terminado")
            {
                throw new ReglaNegocioException("Solo se puede registrar consumo de insumos o intermedios (no productos terminados).");
            }

            var consumos = await DescontarStockAsync(item, request.Cantidad, null, idUsuarioRegistro, request.Motivo?.Trim(), null);

            foreach (var consumo in consumos)
            {
                await _consumoRepository.AgregarAsync(consumo);
            }

            await _consumoRepository.GuardarCambiosAsync();

            var primero = consumos[0];
            return new ConsumoInsumoDto
            {
                Id = primero.Id,
                IdHorneada = null,
                IdItem = item.Id,
                NombreItem = item.Nombre,
                UnidadMedida = item.UnidadMedida,
                CantidadDescontada = consumos.Sum(c => c.CantidadDescontada),
                Motivo = primero.Motivo,
                Fecha = primero.Fecha
            };
        }

        public async Task<List<ConsumoInsumoDto>> ObtenerConsumosManualesAsync()
        {
            var consumos = await _consumoRepository.ObtenerTodosAsync();
            return consumos
                .Where(c => c.IdHorneada is null)
                .OrderByDescending(c => c.Fecha)
                .Select(c => new ConsumoInsumoDto
                {
                    Id = c.Id,
                    IdHorneada = c.IdHorneada,
                    IdItem = c.IdItem,
                    NombreItem = c.Item.Nombre,
                    UnidadMedida = c.Item.UnidadMedida,
                    CantidadDescontada = c.CantidadDescontada,
                    Motivo = c.Motivo,
                    Fecha = c.Fecha
                })
                .ToList();
        }

        /// <summary>
        /// Descuento automático por receta (requisito del tutor, 2026-07-17): al producir
        /// <paramref name="cantidad"/> unidades del producto, descuenta cada componente de su
        /// Receta_Item del stock PEPS de San Mateo (bizcochos en porciones, cremas/jaleas en kg,
        /// etc.), dejando la trazabilidad en Consumo_Insumo.IdLoteProducido. NO guarda cambios —
        /// el llamador persiste todo junto en su propia transacción. Si un componente no alcanza,
        /// lanza ReglaNegocioException y nada queda a medias.
        /// </summary>
        public async Task DescontarPorRecetaAsync(Guid idItemProducido, decimal cantidad, Guid idLoteProducido, Guid idUsuarioRegistro)
        {
            var receta = await _recetaRepository.ObtenerPorProductoTerminadoAsync(idItemProducido);
            if (receta.Count == 0)
            {
                // Producto sin receta cargada (ej. personalizables): no hay nada que descontar
                // automáticamente — el consumo se registra manual, como hasta ahora.
                return;
            }

            var producto = await _itemCatalogoRepository.ObtenerPorIdAsync(idItemProducido);

            foreach (var linea in receta)
            {
                var componente = await _itemCatalogoRepository.ObtenerPorIdAsync(linea.IdItemInsumo)
                    ?? throw new ReglaNegocioException(
                        $"La receta de '{producto?.Nombre}' referencia un componente que ya no existe en el catálogo.");

                var consumos = await DescontarStockAsync(
                    componente,
                    linea.CantidadRequerida * cantidad,
                    null,
                    idUsuarioRegistro,
                    $"Producción: {producto?.Nombre} ×{cantidad:0.##}",
                    idLoteProducido);

                foreach (var consumo in consumos)
                {
                    await _consumoRepository.AgregarAsync(consumo);
                }
            }
        }

        /// <summary>
        /// Descuento automático al producir una torta PERSONALIZABLE (pedido del usuario,
        /// 2026-07-18): estas tortas no tienen Receta_Item fija porque la combinación la elige
        /// el cliente, así que los componentes salen de la configuración del pedido:
        ///  - Bizcocho: NumeroPorciones según la masa elegida (Mixto = mitad vainilla, mitad
        ///    chocolate), del stock PEPS de San Mateo producido por la Horneada.
        ///  - Crema/relleno/colorante elegidos: kg por porción derivados de las recetas fijas
        ///    de la casa (ProduccionConstantes, asunción marcada).
        /// NO guarda cambios — el llamador (PedidoService) persiste todo junto: si algo no
        /// alcanza, la excepción corta antes de guardar y el pedido no cambia de estado.
        /// </summary>
        public async Task DescontarPorPedidoPersonalizableAsync(
            Guid idPedido,
            string descripcionPedido,
            int numeroPorciones,
            string tipoMasa,
            Guid idInsumoCrema,
            Guid? idInsumoRelleno,
            Guid? idInsumoColorante,
            Guid idUsuarioRegistro)
        {
            if (numeroPorciones <= 0)
            {
                throw new ReglaNegocioException("El pedido no tiene un número de porciones válido.");
            }

            var motivo = $"Producción de pedido: {descripcionPedido}";

            // Bizcocho según la masa elegida — mitad y mitad para Mixto (15 porciones → 7.5/7.5,
            // los lotes de bizcocho aceptan decimales porque se corta capa por capa).
            var porcionesVainilla = tipoMasa switch
            {
                "Vainilla" => (decimal)numeroPorciones,
                "Chocolate" => 0m,
                "Mixto" => numeroPorciones / 2m,
                _ => throw new ReglaNegocioException($"Tipo de masa desconocido: '{tipoMasa}'.")
            };
            var porcionesChocolate = numeroPorciones - porcionesVainilla;

            var consumos = new List<ConsumoInsumo>();

            if (porcionesVainilla > 0)
            {
                var bizcocho = await ObtenerBizcochoAsync(ProduccionConstantes.CodigoBizcochoVainilla);
                consumos.AddRange(await DescontarStockAsync(bizcocho, porcionesVainilla, null, idUsuarioRegistro, motivo, null, idPedido));
            }

            if (porcionesChocolate > 0)
            {
                var bizcocho = await ObtenerBizcochoAsync(ProduccionConstantes.CodigoBizcochoChocolate);
                consumos.AddRange(await DescontarStockAsync(bizcocho, porcionesChocolate, null, idUsuarioRegistro, motivo, null, idPedido));
            }

            var componentes = new List<(Guid IdInsumo, decimal KgPorPorcion)> { (idInsumoCrema, ProduccionConstantes.KgCremaPorPorcion) };
            if (idInsumoRelleno is not null)
            {
                componentes.Add((idInsumoRelleno.Value, ProduccionConstantes.KgRellenoPorPorcion));
            }
            if (idInsumoColorante is not null)
            {
                componentes.Add((idInsumoColorante.Value, ProduccionConstantes.KgColorantePorPorcion));
            }

            foreach (var (idInsumo, kgPorPorcion) in componentes)
            {
                var insumo = await _itemCatalogoRepository.ObtenerPorIdAsync(idInsumo)
                    ?? throw new ReglaNegocioException("El pedido referencia un insumo que ya no existe en el catálogo.");
                consumos.AddRange(await DescontarStockAsync(insumo, kgPorPorcion * numeroPorciones, null, idUsuarioRegistro, motivo, null, idPedido));
            }

            foreach (var consumo in consumos)
            {
                await _consumoRepository.AgregarAsync(consumo);
            }
        }

        private async Task<ItemCatalogo> ObtenerBizcochoAsync(string codigoReferencia) =>
            await _itemCatalogoRepository.ObtenerPorCodigoAsync(codigoReferencia)
                ?? throw new ReglaNegocioException(
                    $"No se encontró el ítem intermedio '{codigoReferencia}' — verifique que la migración de bizcochos esté aplicada.");

        private async Task AcumularRecetaAsync(Dictionary<Guid, decimal> necesidad, ItemCatalogo bizcocho, decimal porciones)
        {
            if (porciones <= 0)
            {
                return;
            }

            var receta = await _recetaRepository.ObtenerPorProductoTerminadoAsync(bizcocho.Id);
            if (receta.Count == 0)
            {
                throw new ReglaNegocioException(
                    $"'{bizcocho.Nombre}' no tiene receta cargada — sin ella no se puede calcular el consumo de la batida.");
            }

            foreach (var linea in receta)
            {
                necesidad[linea.IdItemInsumo] =
                    necesidad.GetValueOrDefault(linea.IdItemInsumo) + linea.CantidadRequerida * porciones;
            }
        }

        private async Task CrearLoteBizcochoAsync(ItemCatalogo bizcocho, decimal porciones)
        {
            if (porciones <= 0)
            {
                return;
            }

            var ahora = DateTime.UtcNow;
            await _loteRepository.AgregarAsync(new LotePeps
            {
                Id = Guid.NewGuid(),
                IdItem = bizcocho.Id,
                IdProveedor = null,
                Ubicacion = UbicacionProduccion,
                CantidadInicial = porciones,
                CantidadDisponible = porciones,
                FechaElaboracion = ahora,
                FechaCaducidad = ahora.AddDays(ProduccionConstantes.DiasVidaUtilBizcocho),
                Estado = "Óptimo",
                FechaRegistro = ahora
            });
        }

        private async Task<List<ConsumoInsumo>> DescontarStockAsync(
            ItemCatalogo item, decimal cantidadRequerida, Guid? idHorneada, Guid idUsuarioRegistro, string? motivo, Guid? idLoteProducido, Guid? idPedido = null)
        {
            var lotes = await _loteRepository.ObtenerDisponiblesParaVentaAsync(item.Id, UbicacionProduccion);

            var consumos = new List<ConsumoInsumo>();
            var restante = cantidadRequerida;
            var ahora = DateTime.UtcNow;

            foreach (var lote in lotes)
            {
                if (restante <= 0) break;

                var descuento = Math.Min(lote.CantidadDisponible, restante);
                lote.CantidadDisponible -= descuento;
                restante -= descuento;

                consumos.Add(new ConsumoInsumo
                {
                    Id = Guid.NewGuid(),
                    IdHorneada = idHorneada,
                    IdItem = item.Id,
                    Item = item,
                    IdLote = lote.Id,
                    IdLoteProducido = idLoteProducido,
                    IdPedido = idPedido,
                    CantidadDescontada = descuento,
                    Motivo = motivo,
                    Fecha = ahora,
                    IdUsuarioRegistro = idUsuarioRegistro
                });
            }

            if (restante > 0)
            {
                var sugerencia = item.Tipo == "Intermedio"
                    ? " Registre primero la Horneada (o la preparación) que produce este intermedio."
                    : string.Empty;
                throw new ReglaNegocioException(
                    $"Stock insuficiente de '{item.Nombre}' en {UbicacionProduccion}: faltan {restante:0.###} {item.UnidadMedida}.{sugerencia}");
            }

            return consumos;
        }

        private static HorneadaDto MapearHorneadaDto(Horneada horneada) => new()
        {
            Id = horneada.Id,
            Fecha = horneada.Fecha,
            CantidadBatidas = horneada.CantidadBatidas,
            CantidadBatidasChocolateExtra = horneada.CantidadBatidasChocolateExtra,
            TotalBizcochos = horneada.CantidadBatidas * ProduccionConstantes.BizcochosPorBatida,
            BizcochosChocolate = ProduccionConstantes.BizcochosChocolatePorHorneadaEstandar
                + horneada.CantidadBatidasChocolateExtra * ProduccionConstantes.BizcochosPorBatida,
            BizcochosVainilla = horneada.CantidadBatidas * ProduccionConstantes.BizcochosPorBatida
                - ProduccionConstantes.BizcochosChocolatePorHorneadaEstandar
                - horneada.CantidadBatidasChocolateExtra * ProduccionConstantes.BizcochosPorBatida,
            FechaRegistro = horneada.FechaRegistro,
            Consumos = horneada.Consumos.Select(c => new ConsumoInsumoDto
            {
                Id = c.Id,
                IdHorneada = c.IdHorneada,
                IdItem = c.IdItem,
                NombreItem = c.Item?.Nombre ?? string.Empty,
                UnidadMedida = c.Item?.UnidadMedida ?? string.Empty,
                CantidadDescontada = c.CantidadDescontada,
                Motivo = c.Motivo,
                Fecha = c.Fecha
            }).ToList()
        };
    }
}
