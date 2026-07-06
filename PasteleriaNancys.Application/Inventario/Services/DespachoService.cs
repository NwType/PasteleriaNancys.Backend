using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Services
{
    public class DespachoService : IDespachoService
    {
        private const string UbicacionOrigen = "San Mateo";
        private const string UbicacionDestino = "Mercado Campesino";

        private readonly IViajeRepository _viajeRepository;
        private readonly ILoteRepository _loteRepository;

        public DespachoService(IViajeRepository viajeRepository, ILoteRepository loteRepository)
        {
            _viajeRepository = viajeRepository;
            _loteRepository = loteRepository;
        }

        public async Task<ViajeDto> CrearViajeAsync(CrearViajeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Conductor))
            {
                throw new ReglaNegocioException("El conductor es obligatorio.");
            }

            var viaje = new ViajeDespacho
            {
                Id = Guid.NewGuid(),
                IdUsuarioConductor = request.IdUsuarioConductor,
                Conductor = request.Conductor.Trim(),
                FechaDespacho = DateTime.UtcNow,
                Estado = "Programado",
                Observaciones = request.Observaciones?.Trim()
            };

            await _viajeRepository.AgregarAsync(viaje);
            await _viajeRepository.GuardarCambiosAsync();

            return MapearDto(viaje);
        }

        public async Task<List<ViajeDto>> ObtenerTodosAsync()
        {
            var viajes = await _viajeRepository.ObtenerTodosAsync();
            return viajes.Select(MapearDto).ToList();
        }

        public async Task<ViajeDto> ObtenerPorIdAsync(Guid id)
        {
            var viaje = await _viajeRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró el viaje con id {id}.");

            return MapearDto(viaje);
        }

        public async Task<ViajeDto> AgregarLoteAsync(Guid idViaje, AgregarLoteAlViajeRequest request)
        {
            var viaje = await _viajeRepository.ObtenerPorIdAsync(idViaje)
                ?? throw new NoEncontradoException($"No se encontró el viaje con id {idViaje}.");

            var lote = await _loteRepository.ObtenerPorIdAsync(request.IdLote)
                ?? throw new NoEncontradoException($"No se encontró el lote con id {request.IdLote}.");

            if (lote.Ubicacion != UbicacionOrigen)
            {
                throw new ReglaNegocioException($"El lote no se encuentra disponible en '{UbicacionOrigen}'.");
            }

            if (request.CantidadEnviada > lote.CantidadDisponible)
            {
                throw new ReglaNegocioException("Cantidad insuficiente en el lote seleccionado.");
            }

            var detalle = new ViajeDetalle
            {
                Id = Guid.NewGuid(),
                IdViaje = idViaje,
                IdLote = request.IdLote,
                CantidadEnviada = request.CantidadEnviada
            };

            await _viajeRepository.AgregarDetalleAsync(detalle);
            await _viajeRepository.GuardarCambiosAsync();

            var viajeActualizado = await _viajeRepository.ObtenerPorIdAsync(idViaje)
                ?? throw new NoEncontradoException($"No se encontró el viaje con id {idViaje}.");

            return MapearDto(viajeActualizado);
        }

        public async Task<ViajeDto> ConfirmarEntregaAsync(Guid idViaje)
        {
            var viaje = await _viajeRepository.ObtenerPorIdAsync(idViaje)
                ?? throw new NoEncontradoException($"No se encontró el viaje con id {idViaje}.");

            if (viaje.Estado == "Entregado")
            {
                throw new ConflictoException("Este despacho ya fue confirmado.");
            }

            foreach (var detalle in viaje.Detalles)
            {
                var lote = await _loteRepository.ObtenerPorIdAsync(detalle.IdLote)
                    ?? throw new NoEncontradoException($"No se encontró el lote con id {detalle.IdLote}.");

                lote.Ubicacion = UbicacionDestino;
            }

            viaje.Estado = "Entregado";
            await _viajeRepository.GuardarCambiosAsync();

            return MapearDto(viaje);
        }

        private static ViajeDto MapearDto(ViajeDespacho viaje) => new()
        {
            Id = viaje.Id,
            IdUsuarioConductor = viaje.IdUsuarioConductor,
            Conductor = viaje.Conductor,
            FechaDespacho = viaje.FechaDespacho,
            Estado = viaje.Estado,
            Observaciones = viaje.Observaciones,
            Detalles = viaje.Detalles.Select(d => new ViajeDetalleDto
            {
                Id = d.Id,
                IdLote = d.IdLote,
                CantidadEnviada = d.CantidadEnviada
            }).ToList()
        };
    }
}
