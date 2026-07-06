using PasteleriaNancys.Application.Caja.Dtos;
using PasteleriaNancys.Application.Caja.Interfaces;
using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Caja;

namespace PasteleriaNancys.Application.Caja.Services
{
    public class VentaService : IVentaService
    {
        private const string UbicacionVenta = "Mercado Campesino";
        private static readonly string[] MetodosPagoValidos = { "Efectivo", "QR" };

        private readonly IVentaRepository _ventaRepository;
        private readonly ITurnoRepository _turnoRepository;
        private readonly IItemCatalogoRepository _itemCatalogoRepository;
        private readonly ILoteRepository _loteRepository;

        public VentaService(
            IVentaRepository ventaRepository,
            ITurnoRepository turnoRepository,
            IItemCatalogoRepository itemCatalogoRepository,
            ILoteRepository loteRepository)
        {
            _ventaRepository = ventaRepository;
            _turnoRepository = turnoRepository;
            _itemCatalogoRepository = itemCatalogoRepository;
            _loteRepository = loteRepository;
        }

        public async Task<VentaDto> ObtenerPorIdAsync(Guid id)
        {
            var venta = await _ventaRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró la venta con id {id}.");

            return MapearDto(venta);
        }

        public async Task<VentaDto> RegistrarAsync(RegistrarVentaRequest request)
        {
            var turno = await _turnoRepository.ObtenerPorIdAsync(request.IdTurno)
                ?? throw new NoEncontradoException($"No se encontró el turno con id {request.IdTurno}.");

            if (turno.Estado != "Abierto")
            {
                throw new ReglaNegocioException("Debe aperturar un turno antes de registrar ventas.");
            }

            if (request.Productos.Count == 0)
            {
                throw new ReglaNegocioException("La venta debe incluir al menos un producto.");
            }

            if (!MetodosPagoValidos.Contains(request.MetodoPago))
            {
                throw new ReglaNegocioException("El método de pago debe ser 'Efectivo' o 'QR'.");
            }

            var idVenta = Guid.NewGuid();
            var detalles = new List<VentaDetalle>();
            decimal total = 0;

            foreach (var producto in request.Productos)
            {
                _ = await _itemCatalogoRepository.ObtenerPorIdAsync(producto.IdItem)
                    ?? throw new NoEncontradoException($"No se encontró el ítem con id {producto.IdItem}.");

                if (producto.Cantidad <= 0)
                {
                    throw new ReglaNegocioException("La cantidad debe ser mayor a cero.");
                }

                if (producto.PrecioUnitario < 0)
                {
                    throw new ReglaNegocioException("El precio unitario no puede ser negativo.");
                }

                total += producto.Cantidad * producto.PrecioUnitario;
            }

            if (total <= 0)
            {
                throw new ReglaNegocioException("El total de la venta debe ser mayor a cero.");
            }

            foreach (var producto in request.Productos)
            {
                await DescontarStockPepsAsync(producto.IdItem, producto.Cantidad);

                detalles.Add(new VentaDetalle
                {
                    Id = Guid.NewGuid(),
                    IdVenta = idVenta,
                    IdItem = producto.IdItem,
                    Cantidad = producto.Cantidad,
                    PrecioUnitario = producto.PrecioUnitario,
                    Subtotal = producto.Cantidad * producto.PrecioUnitario
                });
            }

            var venta = new VentaPos
            {
                Id = idVenta,
                IdTurno = request.IdTurno,
                FechaHora = DateTime.UtcNow,
                TotalPagado = total,
                MetodoPago = request.MetodoPago,
                Anulada = false,
                Detalles = detalles
            };

            turno.TotalIngresosSistema += total;

            await _ventaRepository.AgregarAsync(venta);
            await _ventaRepository.GuardarCambiosAsync();

            return MapearDto(venta);
        }

        public async Task<VentaDto> AnularAsync(Guid idVenta, AnularVentaRequest request)
        {
            var venta = await _ventaRepository.ObtenerPorIdAsync(idVenta)
                ?? throw new NoEncontradoException($"No se encontró la venta con id {idVenta}.");

            if (venta.Anulada)
            {
                throw new ConflictoException("Esta venta ya fue anulada.");
            }

            var turno = await _turnoRepository.ObtenerPorIdAsync(venta.IdTurno)
                ?? throw new NoEncontradoException($"No se encontró el turno con id {venta.IdTurno}.");

            if (turno.Estado != "Abierto")
            {
                throw new ReglaNegocioException("Solo se pueden anular ventas de un turno abierto.");
            }

            if (string.IsNullOrWhiteSpace(request.Motivo))
            {
                throw new ReglaNegocioException("Debe indicar el motivo de la anulación.");
            }

            foreach (var detalle in venta.Detalles)
            {
                await ReponerStockPepsAsync(detalle.IdItem, detalle.Cantidad);
            }

            venta.Anulada = true;
            venta.MotivoAnulacion = request.Motivo.Trim();
            turno.TotalIngresosSistema -= venta.TotalPagado;

            await _ventaRepository.GuardarCambiosAsync();

            return MapearDto(venta);
        }

        private async Task DescontarStockPepsAsync(Guid idItem, decimal cantidadRequerida)
        {
            var lotes = await _loteRepository.ObtenerDisponiblesParaVentaAsync(idItem, UbicacionVenta);

            var restante = cantidadRequerida;
            foreach (var lote in lotes)
            {
                if (restante <= 0) break;

                var descuento = Math.Min(lote.CantidadDisponible, restante);
                lote.CantidadDisponible -= descuento;
                restante -= descuento;
            }

            if (restante > 0)
            {
                throw new ReglaNegocioException("Stock insuficiente para este producto.");
            }
        }

        private async Task ReponerStockPepsAsync(Guid idItem, decimal cantidad)
        {
            // No existe una tabla que vincule qué lote exacto se descontó por línea de
            // venta, así que la reposición se aproxima devolviendo la cantidad al lote
            // con la fecha de elaboración más reciente que tenga espacio disponible.
            var lotes = await _loteRepository.ObtenerParaReponerAsync(idItem);

            var restante = cantidad;
            foreach (var lote in lotes)
            {
                if (restante <= 0) break;

                var espacio = lote.CantidadInicial - lote.CantidadDisponible;
                var reposicion = Math.Min(espacio, restante);
                lote.CantidadDisponible += reposicion;
                restante -= reposicion;
            }
        }

        private static VentaDto MapearDto(VentaPos venta) => new()
        {
            Id = venta.Id,
            IdTurno = venta.IdTurno,
            FechaHora = venta.FechaHora,
            TotalPagado = venta.TotalPagado,
            MetodoPago = venta.MetodoPago,
            Anulada = venta.Anulada,
            MotivoAnulacion = venta.MotivoAnulacion,
            Detalles = venta.Detalles.Select(d => new VentaDetalleDto
            {
                Id = d.Id,
                IdItem = d.IdItem,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal
            }).ToList()
        };
    }
}
