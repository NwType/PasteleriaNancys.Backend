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
        private readonly IConsumoService _consumoService;

        public DespachoService(
            IViajeRepository viajeRepository,
            ILoteRepository loteRepository,
            IItemCatalogoRepository itemCatalogoRepository,
            IConsumoService consumoService)
        {
            _viajeRepository = viajeRepository;
            _loteRepository = loteRepository;
            _itemCatalogoRepository = itemCatalogoRepository;
            _consumoService = consumoService;
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

        public async Task<ViajeDto> AgregarProductoAsync(Guid idViaje, Guid idUsuarioRegistro, AgregarProductoAlViajeRequest request)
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

            // Las personalizables no viajan a vitrina: se hacen POR PEDIDO y su inventario se
            // descuenta al pasar el pedido a "En Producción" (PedidoService). Antes esto pasaba
            // en silencio sin descontar nada — creaba stock invisible (la vitrina/POS filtran
            // personalizables) y sin consumo, pura inconsistencia (hallazgo 2026-07-18).
            if (item.EsPersonalizable)
            {
                throw new ReglaNegocioException(
                    $"'{item.Nombre}' es una torta personalizable: se produce por pedido (pasa a 'En Producción' en Pedidos), no se despacha a vitrina por viaje.");
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

            // Inventario automático (requisito del tutor, 2026-07-17): armar estas tortas
            // consume sus componentes según Receta_Item — porciones de bizcocho (horneadas antes
            // vía Horneada), cremas, jaleas, rellenos — del stock PEPS de San Mateo. Si algo no
            // alcanza, la excepción corta ANTES de guardar y no queda nada a medias.
            await _consumoService.DescontarPorRecetaAsync(item.Id, request.Cantidad, loteOrigen.Id, idUsuarioRegistro);

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
