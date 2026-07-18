using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Services
{
    public class LoteService : ILoteService
    {
        private static readonly string[] EstadosValidos = { "Óptimo", "Crítico", "Baja" };

        // La materia prima solo se compra y se guarda en el centro de producción — nunca viaja al
        // punto de venta (eso es exclusivo de los productos terminados, vía Viaje/DespachoService).
        // Se fuerza el valor en vez de solo validarlo para que ni un bug del frontend ni una
        // llamada directa a la API puedan dejar un insumo mal ubicado.
        private const string UbicacionMateriaPrima = "San Mateo";

        private readonly ILoteRepository _loteRepository;
        private readonly IItemCatalogoRepository _itemCatalogoRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IStockMinimoRepository _stockMinimoRepository;
        private readonly IAlertaService _alertaService;

        public LoteService(
            ILoteRepository loteRepository,
            IItemCatalogoRepository itemCatalogoRepository,
            IProveedorRepository proveedorRepository,
            IStockMinimoRepository stockMinimoRepository,
            IAlertaService alertaService)
        {
            _loteRepository = loteRepository;
            _itemCatalogoRepository = itemCatalogoRepository;
            _proveedorRepository = proveedorRepository;
            _stockMinimoRepository = stockMinimoRepository;
            _alertaService = alertaService;
        }

        public async Task<LoteDto> RegistrarAsync(RegistrarLoteRequest request)
        {
            var item = await _itemCatalogoRepository.ObtenerPorIdAsync(request.IdItem)
                ?? throw new NoEncontradoException($"No se encontró el ítem con id {request.IdItem}.");

            if (item.Tipo != "MateriaPrima")
            {
                throw new ReglaNegocioException(
                    "Solo se pueden registrar lotes de materia prima. El stock de tortas se registra mediante un Viaje.");
            }

            if (request.IdProveedor is not null &&
                await _proveedorRepository.ObtenerPorIdAsync(request.IdProveedor.Value) is null)
            {
                throw new NoEncontradoException($"No se encontró el proveedor con id {request.IdProveedor}.");
            }

            if (request.FechaCaducidad <= request.FechaElaboracion)
            {
                throw new ReglaNegocioException("La fecha de caducidad debe ser posterior a la de elaboración.");
            }

            var lote = new LotePeps
            {
                Id = Guid.NewGuid(),
                IdItem = request.IdItem,
                IdProveedor = request.IdProveedor,
                Ubicacion = UbicacionMateriaPrima,
                CantidadInicial = request.CantidadInicial,
                CantidadDisponible = request.CantidadInicial,
                FechaElaboracion = request.FechaElaboracion,
                FechaCaducidad = request.FechaCaducidad,
                Estado = "Óptimo",
                FechaRegistro = DateTime.UtcNow
            };

            await _loteRepository.AgregarAsync(lote);
            await _loteRepository.GuardarCambiosAsync();

            // El orden PEPS (vw_Lotes_PEPS_Ordenados) se recalcula automáticamente
            // en SQL Server cada vez que se consulta; no requiere acción adicional aquí.
            var dias = (lote.FechaCaducidad.Date - DateTime.UtcNow.Date).Days;
            var alerta = dias < 3 ? "CRITICO" : dias <= 7 ? "PROXIMO" : null;

            var dto = MapearDto(lote);
            dto.Alerta = alerta;
            return dto;
        }

        public async Task<List<LoteDto>> ObtenerTodosAsync()
        {
            var lotes = await _loteRepository.ObtenerTodosAsync();
            await SincronizarEstadosVencidosAsync(lotes);
            return lotes.Select(MapearDto).ToList();
        }

        public async Task<LoteDto> ObtenerPorIdAsync(Guid id)
        {
            var lote = await _loteRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró el lote con id {id}.");

            await SincronizarEstadosVencidosAsync(new List<LotePeps> { lote });
            return MapearDto(lote);
        }

        public async Task<LoteDto> ActualizarEstadoAsync(Guid id, ActualizarEstadoLoteRequest request)
        {
            var lote = await _loteRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró el lote con id {id}.");

            if (!EstadosValidos.Contains(request.Estado))
            {
                throw new ReglaNegocioException("El estado debe ser 'Óptimo', 'Crítico' o 'Baja'.");
            }

            var pedidosEnRiesgo = new List<PedidoAfectadoDto>();

            // CU-18, flujo alterno: si el lote pasa a "Baja" y, sin su cantidad, el ítem cae en
            // stock crítico, se advierte de los pedidos pendientes que podrían verse afectados
            // (no se bloquea la baja, es solo una advertencia informativa).
            if (request.Estado == "Baja" && lote.Estado != "Baja")
            {
                var stockMinimo = await _stockMinimoRepository.ObtenerPorItemAsync(lote.IdItem);
                if (stockMinimo is not null && stockMinimo.Activo)
                {
                    var stockTotalActual = await _loteRepository.ObtenerStockDisponibleTotalAsync(lote.IdItem);
                    var stockSinEsteLote = stockTotalActual - lote.CantidadDisponible;

                    if (stockSinEsteLote <= stockMinimo.CantidadMinima)
                    {
                        pedidosEnRiesgo = await _alertaService.ConsultarPedidosAfectadosAsync(lote.IdItem);
                    }
                }
            }

            lote.Estado = request.Estado;
            await _loteRepository.GuardarCambiosAsync();

            var dto = MapearDto(lote);
            dto.PedidosEnRiesgo = pedidosEnRiesgo;
            return dto;
        }

        public async Task<List<LoteProximoACaducarDto>> ConsultarProximosACaducarAsync()
        {
            var lotes = await _loteRepository.ObtenerProximosACaducarAsync(7);

            return lotes.Select(l => new LoteProximoACaducarDto
            {
                IdLote = l.IdLote,
                IdItem = l.IdItem,
                CodigoReferencia = l.CodigoReferencia,
                NombreItem = l.NombreItem,
                Ubicacion = l.Ubicacion,
                CantidadDisponible = l.CantidadDisponible,
                FechaCaducidad = l.FechaCaducidad,
                Estado = l.Estado,
                DiasParaCaducar = l.DiasParaCaducar ?? 0,
                // "VENCIDO" = ya se dio de baja automáticamente por caducidad (SincronizarEstadosVencidosAsync) —
                // distinto de "CRITICO" (todavía vendible, pero se vence en menos de 3 días).
                NivelAlerta = l.Estado == "Baja" ? "VENCIDO" : (l.DiasParaCaducar ?? int.MaxValue) < 3 ? "CRITICO" : "PROXIMO"
            }).ToList();
        }

        /// <summary>
        /// El estado "Baja" manual (decisión humana, ej. por calidad) se respeta tal cual y nunca
        /// se revierte automáticamente. "Óptimo"/"Crítico"/"Baja por vencimiento" en cambio
        /// dependen de cuántos días faltan para caducar, algo que cambia con el simple paso del
        /// tiempo. Un lote YA VENCIDO (días negativos) pasa a "Baja" automáticamente — PEPS nunca
        /// debe poder vender ni consumir algo ya vencido, sin depender de que alguien lo note y lo
        /// dé de baja a mano (regla de negocio pedida explícitamente por el usuario, 2026-07-14,
        /// tras encontrar un lote real vencido hace más de dos semanas que seguía "Óptimo").
        /// </summary>
        private static string CalcularEstadoVigente(LotePeps lote)
        {
            if (lote.Estado == "Baja")
            {
                return "Baja";
            }

            var diasParaCaducar = (lote.FechaCaducidad.Date - DateTime.UtcNow.Date).Days;

            if (diasParaCaducar < 0)
            {
                return "Baja";
            }

            var venceProntoPorCaducidad = diasParaCaducar < 3;

            // Nunca "mejora" a Óptimo un estado que ya estaba en Crítico (podría haberse marcado
            // manualmente por otra razón, ej. calidad) — solo lo sube automáticamente cuando la
            // caducidad próxima lo amerita, algo que antes quedaba congelado desde el registro.
            return venceProntoPorCaducidad || lote.Estado == "Crítico" ? "Crítico" : "Óptimo";
        }

        // Persiste la transición Óptimo→Crítico→Baja-por-vencimiento la próxima vez que se leen
        // los lotes (Inventario PEPS, historial de entradas, etc.) — así el estado guardado en la
        // BD no queda "congelado" desde el registro y todo lo que ya filtra por Estado != 'Baja'
        // (ventas, consumo, stock disponible, alertas) queda automáticamente al día sin tocar esas
        // consultas una por una.
        private async Task SincronizarEstadosVencidosAsync(List<LotePeps> lotes)
        {
            var huboCambios = false;
            foreach (var lote in lotes)
            {
                var estadoVigente = CalcularEstadoVigente(lote);
                if (lote.Estado != estadoVigente)
                {
                    lote.Estado = estadoVigente;
                    huboCambios = true;
                }
            }

            if (huboCambios)
            {
                await _loteRepository.GuardarCambiosAsync();
            }
        }

        private static LoteDto MapearDto(LotePeps lote) => new()
        {
            Id = lote.Id,
            IdItem = lote.IdItem,
            IdProveedor = lote.IdProveedor,
            Ubicacion = lote.Ubicacion,
            CantidadInicial = lote.CantidadInicial,
            CantidadDisponible = lote.CantidadDisponible,
            FechaElaboracion = lote.FechaElaboracion,
            FechaCaducidad = lote.FechaCaducidad,
            Estado = lote.Estado,
            FechaRegistro = lote.FechaRegistro
        };
    }
}
