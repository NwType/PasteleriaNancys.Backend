using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Services
{
    public class LoteService : ILoteService
    {
        private static readonly string[] EstadosValidos = { "Óptimo", "Crítico", "Baja" };

        private readonly ILoteRepository _loteRepository;
        private readonly IItemCatalogoRepository _itemCatalogoRepository;
        private readonly IProveedorRepository _proveedorRepository;

        public LoteService(
            ILoteRepository loteRepository,
            IItemCatalogoRepository itemCatalogoRepository,
            IProveedorRepository proveedorRepository)
        {
            _loteRepository = loteRepository;
            _itemCatalogoRepository = itemCatalogoRepository;
            _proveedorRepository = proveedorRepository;
        }

        public async Task<LoteDto> RegistrarAsync(RegistrarLoteRequest request)
        {
            _ = await _itemCatalogoRepository.ObtenerPorIdAsync(request.IdItem)
                ?? throw new NoEncontradoException($"No se encontró el ítem con id {request.IdItem}.");

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
                Ubicacion = request.Ubicacion.Trim(),
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
            return lotes.Select(MapearDto).ToList();
        }

        public async Task<LoteDto> ObtenerPorIdAsync(Guid id)
        {
            var lote = await _loteRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró el lote con id {id}.");

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

            // Nota: el flujo alterno de CU-18 ("advertir si tiene pedidos en producción que lo
            // requieren") depende del módulo de Pedidos/Web, que aún no está reconciliado con la
            // base de datos real — queda pendiente como integración futura.
            lote.Estado = request.Estado;
            await _loteRepository.GuardarCambiosAsync();

            return MapearDto(lote);
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
                NivelAlerta = (l.DiasParaCaducar ?? int.MaxValue) < 3 ? "CRITICO" : "PROXIMO"
            }).ToList();
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
