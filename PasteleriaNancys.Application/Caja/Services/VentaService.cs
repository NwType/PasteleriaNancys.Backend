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
            var preciosPorItem = new Dictionary<Guid, decimal>();
            decimal total = 0;

            foreach (var producto in request.Productos)
            {
                var item = await _itemCatalogoRepository.ObtenerPorIdAsync(producto.IdItem)
                    ?? throw new NoEncontradoException($"No se encontró el ítem con id {producto.IdItem}.");

                if (producto.Cantidad <= 0)
                {
                    throw new ReglaNegocioException("La cantidad debe ser mayor a cero.");
                }

                // El precio siempre se toma del catálogo, nunca del request — el cliente/cajero no
                // puede fijar el precio de venta, solo qué producto y cuánta cantidad.
                preciosPorItem[producto.IdItem] = item.PrecioUnitario;
                total += producto.Cantidad * item.PrecioUnitario;
            }

            if (total <= 0)
            {
                throw new ReglaNegocioException("El total de la venta debe ser mayor a cero.");
            }

            var consumos = new List<VentaDetalleLote>();
            foreach (var producto in request.Productos)
            {
                var idDetalle = Guid.NewGuid();
                var consumoDetalle = await DescontarStockPepsAsync(producto.IdItem, producto.Cantidad);
                consumos.AddRange(consumoDetalle.Select(c => new VentaDetalleLote
                {
                    Id = Guid.NewGuid(),
                    IdVentaDetalle = idDetalle,
                    IdLote = c.IdLote,
                    CantidadDescontada = c.Cantidad
                }));

                var precioUnitario = preciosPorItem[producto.IdItem];
                detalles.Add(new VentaDetalle
                {
                    Id = idDetalle,
                    IdVenta = idVenta,
                    IdItem = producto.IdItem,
                    Cantidad = producto.Cantidad,
                    PrecioUnitario = precioUnitario,
                    Subtotal = producto.Cantidad * precioUnitario
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
            await _loteRepository.RegistrarConsumoAsync(consumos);
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
                await ReponerStockPepsAsync(detalle.Id, detalle.IdItem, detalle.Cantidad);
            }

            venta.Anulada = true;
            venta.MotivoAnulacion = request.Motivo.Trim();
            turno.TotalIngresosSistema -= venta.TotalPagado;

            await _ventaRepository.GuardarCambiosAsync();

            return MapearDto(venta);
        }

        // Simulación 2026-07-13 (pedido explícito del usuario, presentación el mismo día): un
        // pedido pagado por QR desde la vitrina pública todavía no tiene webhook bancario real
        // (CLAUDE.md lo deja pendiente hasta conseguir permisos de API de pruebas de un banco) —
        // mientras tanto, la confirmación manual/simulada de PagoQrService debe reflejarse igual
        // en el arqueo del turno abierto, para que la demo muestre el flujo completo. A propósito
        // NO se descuenta stock PEPS aquí: el pedido puede confirmarse antes de que la torta
        // exista físicamente en Mercado Campesino (eso pasa recién en producción/despacho), así
        // que forzar el descuento ahora rompería con un "stock insuficiente" falso.
        public async Task RegistrarVentaSimuladaPorPedidoQrAsync(Guid idPedido, Guid? idItemProductoBase, decimal monto)
        {
            if (monto <= 0)
            {
                return;
            }

            var turno = (await _turnoRepository.ObtenerAbiertosAsync())
                .OrderByDescending(t => t.FechaApertura)
                .FirstOrDefault();

            if (turno is null)
            {
                // Sin caja abierta no hay dónde reflejar el ingreso todavía — el pago QR queda
                // confirmado igual, simplemente no impacta arqueo hasta que se abra un turno.
                return;
            }

            var idVenta = Guid.NewGuid();
            var detalles = new List<VentaDetalle>();

            if (idItemProductoBase is not null)
            {
                detalles.Add(new VentaDetalle
                {
                    Id = Guid.NewGuid(),
                    IdVenta = idVenta,
                    IdItem = idItemProductoBase.Value,
                    Cantidad = 1,
                    PrecioUnitario = monto,
                    Subtotal = monto
                });
            }

            var venta = new VentaPos
            {
                Id = idVenta,
                IdTurno = turno.Id,
                FechaHora = DateTime.UtcNow,
                TotalPagado = monto,
                MetodoPago = "QR",
                Anulada = false,
                Detalles = detalles
            };

            turno.TotalIngresosSistema += monto;

            await _ventaRepository.AgregarAsync(venta);
            await _ventaRepository.GuardarCambiosAsync();
        }

        private async Task<List<(Guid IdLote, decimal Cantidad)>> DescontarStockPepsAsync(Guid idItem, decimal cantidadRequerida)
        {
            var lotes = await _loteRepository.ObtenerDisponiblesParaVentaAsync(idItem, UbicacionVenta);

            var consumo = new List<(Guid IdLote, decimal Cantidad)>();
            var restante = cantidadRequerida;
            foreach (var lote in lotes)
            {
                if (restante <= 0) break;

                var descuento = Math.Min(lote.CantidadDisponible, restante);
                lote.CantidadDisponible -= descuento;
                restante -= descuento;
                consumo.Add((lote.Id, descuento));
            }

            if (restante > 0)
            {
                throw new ReglaNegocioException("Stock insuficiente para este producto.");
            }

            return consumo;
        }

        private async Task ReponerStockPepsAsync(Guid idVentaDetalle, Guid idItem, decimal cantidad)
        {
            var consumosRegistrados = await _loteRepository.ObtenerConsumosPorVentaDetalleAsync(idVentaDetalle);
            if (consumosRegistrados.Count > 0)
            {
                // Reversión exacta: se devuelve a cada lote precisamente lo que se le descontó.
                foreach (var consumo in consumosRegistrados)
                {
                    var lote = await _loteRepository.ObtenerPorIdAsync(consumo.IdLote);
                    if (lote is not null)
                    {
                        lote.CantidadDisponible += consumo.CantidadDescontada;
                    }
                }
                return;
            }

            // Venta registrada antes de que existiera el registro de consumo por lote (dato
            // legado): se aproxima reponiendo al lote más reciente con espacio disponible.
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
