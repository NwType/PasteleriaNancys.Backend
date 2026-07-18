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
        private readonly IItemCatalogoRepository _itemCatalogoRepository;

        public DespachoService(IViajeRepository viajeRepository, ILoteRepository loteRepository, IItemCatalogoRepository itemCatalogoRepository)
        {
            _viajeRepository = viajeRepository;
            _loteRepository = loteRepository;
            _itemCatalogoRepository = itemCatalogoRepository;
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

        public async Task<ViajeDto> AgregarProductoAsync(Guid idViaje, AgregarProductoAlViajeRequest request)
        {
            var viaje = await _viajeRepository.ObtenerPorIdAsync(idViaje)
                ?? throw new NoEncontradoException($"No se encontró el viaje con id {idViaje}.");

            if (viaje.Estado == "Entregado")
            {
                throw new ReglaNegocioException("No se pueden agregar productos a un viaje que ya fue entregado.");
            }

            var item = await _itemCatalogoRepository.ObtenerPorIdAsync(request.IdItem)
                ?? throw new NoEncontradoException($"No se encontró el producto con id {request.IdItem}.");

            if (item.Tipo != "Terminado" || !item.Activo)
            {
                throw new ReglaNegocioException("Solo se pueden agregar tortas (productos terminados activos) al viaje.");
            }

            if (request.Cantidad <= 0)
            {
                throw new ReglaNegocioException("La cantidad debe ser mayor a 0.");
            }

            if (request.FechaCaducidad.Date <= DateTime.UtcNow.Date)
            {
                throw new ReglaNegocioException("La fecha de caducidad debe ser posterior a hoy.");
            }

            // El viaje ES el registro de la horneada: se crea un lote nuevo en San Mateo ya
            // comprometido al 100% a este viaje (CantidadDisponible = 0) — no queda stock suelto
            // en origen porque no hay excedente, todo lo horneado se envía.
            var loteOrigen = new LotePeps
            {
                Id = Guid.NewGuid(),
                IdItem = item.Id,
                IdProveedor = null,
                Ubicacion = UbicacionOrigen,
                CantidadInicial = request.Cantidad,
                CantidadDisponible = 0,
                FechaElaboracion = DateTime.UtcNow,
                FechaCaducidad = request.FechaCaducidad,
                Estado = "Óptimo",
                FechaRegistro = DateTime.UtcNow
            };
            await _loteRepository.AgregarAsync(loteOrigen);

            var detalle = new ViajeDetalle
            {
                Id = Guid.NewGuid(),
                IdViaje = idViaje,
                IdLote = loteOrigen.Id,
                CantidadEnviada = request.Cantidad
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
                var loteOrigen = await _loteRepository.ObtenerPorIdAsync(detalle.IdLote)
                    ?? throw new NoEncontradoException($"No se encontró el lote con id {detalle.IdLote}.");

                // La cantidad enviada llega como un lote propio en el destino (no se mueve el lote
                // completo) — lo que no se envió queda correctamente en el origen con su cantidad
                // real restante.
                var loteDestino = new LotePeps
                {
                    Id = Guid.NewGuid(),
                    IdItem = loteOrigen.IdItem,
                    IdProveedor = loteOrigen.IdProveedor,
                    Ubicacion = UbicacionDestino,
                    Estado = loteOrigen.Estado,
                    CantidadInicial = detalle.CantidadEnviada,
                    CantidadDisponible = detalle.CantidadEnviada,
                    FechaElaboracion = loteOrigen.FechaElaboracion,
                    FechaCaducidad = loteOrigen.FechaCaducidad,
                    FechaRegistro = DateTime.UtcNow
                };

                await _loteRepository.AgregarAsync(loteDestino);
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
                CantidadEnviada = d.CantidadEnviada,
                IdItem = d.Lote.IdItem,
                NombreItem = d.Lote.Item.Nombre,
                ImagenUrl = d.Lote.Item.ImagenUrl,
                PrecioUnitario = d.Lote.Item.PrecioUnitario,
                ColorDecoracion = d.Lote.Item.ColorDecoracion
            }).ToList()
        };
    }
}
