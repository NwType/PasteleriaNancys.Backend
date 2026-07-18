using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Services
{
    public class ConsumoService : IConsumoService
    {
        // Ubicación de producción — mismo valor que DespachoService.UbicacionOrigen. Las batidas y
        // el consumo manual de insumos de receta se descuentan del stock de San Mateo, no de la
        // vitrina de venta (Mercado Campesino).
        private const string UbicacionProduccion = "San Mateo";

        // Fórmula real por batida (dato dado por el usuario, 2026-07-13) — cantidades convertidas
        // de gramos/mililitros a la unidad base (kg/litro) en la que se registran los insumos.
        // Asunción marcada: se asume que la harina de la receta es "Harina de Trigo Especial 000"
        // (MP-HARI-001), la misma que ya usa Receta_Item para Torta de Chocolate — el usuario no
        // distinguió entre la 000 y la 00 (MP-HARI-002) al dar la fórmula de la batida.
        private const string CodigoHarina = "MP-HARI-001";
        private const string CodigoAzucar = "MP-HARI-004";
        private const string CodigoMaicena = "MP-HARI-003";
        private const string CodigoPolvoHornear = "MP-HARI-005";
        private const string CodigoCaramelina = "MP-CARA-001";
        private const string CodigoHuevo = "MP-HUEV-001";

        private const decimal HarinaKgPorBatida = 1.764m;
        private const decimal AzucarKgPorBatida = 1.372m;
        private const decimal MaicenaKgPorBatida = 0.775m;
        private const decimal PolvoHornearKgPorBatida = 0.075m;

        // De todas las batidas del día, exactamente UNA — siempre la última — sale mitad y
        // mitad (10 vainilla + 10 chocolate); el resto de las batidas del día son 100% vainilla.
        // Por eso la caramelina NO escala con la cantidad de batidas: es un monto fijo por
        // horneada (un solo "envase" se usa sin importar si hubo 1, 2 o 24 batidas ese día) —
        // aclarado por el usuario 2026-07-13 después de dos correcciones previas. El envase real
        // se pesa en kg (no en ml) — la receta dio "50ml" pero el insumo se registra y se
        // descuenta en kg, igual que el resto de los insumos secos de esta fórmula.
        private const decimal CaramelinaKgPorHorneada = 0.050m;

        // A diferencia de los insumos de arriba, los huevos NO tienen una fórmula fija —
        // el usuario confirmó que varía (20-23 por batida, un poco más en la última mitad-
        // y-mitad) y que "así lo hacen, lo vi en vivo". Por eso CantidadHuevos se ingresa a
        // mano en cada horneada en vez de multiplicarse por CantidadBatidas.

        private readonly IHorneadaRepository _horneadaRepository;
        private readonly IConsumoInsumoRepository _consumoRepository;
        private readonly IItemCatalogoRepository _itemCatalogoRepository;
        private readonly ILoteRepository _loteRepository;

        public ConsumoService(
            IHorneadaRepository horneadaRepository,
            IConsumoInsumoRepository consumoRepository,
            IItemCatalogoRepository itemCatalogoRepository,
            ILoteRepository loteRepository)
        {
            _horneadaRepository = horneadaRepository;
            _consumoRepository = consumoRepository;
            _itemCatalogoRepository = itemCatalogoRepository;
            _loteRepository = loteRepository;
        }

        public async Task<HorneadaDto> RegistrarHorneadaAsync(Guid idUsuarioRegistro, RegistrarHorneadaRequest request)
        {
            if (request.CantidadBatidas <= 0)
            {
                throw new ReglaNegocioException("La cantidad de batidas debe ser mayor a 0.");
            }

            if (request.CantidadHuevos <= 0)
            {
                throw new ReglaNegocioException("La cantidad de huevos usados debe ser mayor a 0.");
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

            var consumos = new List<ConsumoInsumo>();
            consumos.AddRange(await DescontarInsumoAsync(
                CodigoHarina, HarinaKgPorBatida * request.CantidadBatidas, horneada.Id, idUsuarioRegistro));
            consumos.AddRange(await DescontarInsumoAsync(
                CodigoAzucar, AzucarKgPorBatida * request.CantidadBatidas, horneada.Id, idUsuarioRegistro));
            consumos.AddRange(await DescontarInsumoAsync(
                CodigoMaicena, MaicenaKgPorBatida * request.CantidadBatidas, horneada.Id, idUsuarioRegistro));
            consumos.AddRange(await DescontarInsumoAsync(
                CodigoPolvoHornear, PolvoHornearKgPorBatida * request.CantidadBatidas, horneada.Id, idUsuarioRegistro));
            var caramelinaNecesaria = CaramelinaKgPorHorneada
                + ProduccionConstantes.CaramelinaKgPorBatidaChocolateExtra * request.CantidadBatidasChocolateExtra;
            consumos.AddRange(await DescontarInsumoAsync(
                CodigoCaramelina, caramelinaNecesaria, horneada.Id, idUsuarioRegistro));
            consumos.AddRange(await DescontarInsumoAsync(
                CodigoHuevo, request.CantidadHuevos, horneada.Id, idUsuarioRegistro));

            foreach (var consumo in consumos)
            {
                await _consumoRepository.AgregarAsync(consumo);
            }

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

            if (item.Tipo != "MateriaPrima")
            {
                throw new ReglaNegocioException("Solo se puede registrar consumo de insumos (materia prima).");
            }

            var consumos = await DescontarStockAsync(item, request.Cantidad, null, idUsuarioRegistro, request.Motivo?.Trim());

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

        private async Task<List<ConsumoInsumo>> DescontarInsumoAsync(
            string codigoReferencia, decimal cantidadRequerida, Guid idHorneada, Guid idUsuarioRegistro)
        {
            var item = await _itemCatalogoRepository.ObtenerPorCodigoAsync(codigoReferencia)
                ?? throw new ReglaNegocioException(
                    $"No se encontró el insumo '{codigoReferencia}' — verifique que esté cargado en el catálogo.");

            return await DescontarStockAsync(item, cantidadRequerida, idHorneada, idUsuarioRegistro, null);
        }

        private async Task<List<ConsumoInsumo>> DescontarStockAsync(
            ItemCatalogo item, decimal cantidadRequerida, Guid? idHorneada, Guid idUsuarioRegistro, string? motivo)
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
                    CantidadDescontada = descuento,
                    Motivo = motivo,
                    Fecha = ahora,
                    IdUsuarioRegistro = idUsuarioRegistro
                });
            }

            if (restante > 0)
            {
                throw new ReglaNegocioException(
                    $"Stock insuficiente de '{item.Nombre}' en {UbicacionProduccion}: faltan {restante:0.###} {item.UnidadMedida}.");
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
