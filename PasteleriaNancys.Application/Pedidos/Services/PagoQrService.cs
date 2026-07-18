using PasteleriaNancys.Application.Caja.Interfaces;
using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Pedidos.Dtos;
using PasteleriaNancys.Application.Pedidos.Interfaces;
using PasteleriaNancys.Domain.Pedidos;

namespace PasteleriaNancys.Application.Pedidos.Services
{
    public class PagoQrService : IPagoQrService
    {
        private static readonly TimeSpan VigenciaQr = TimeSpan.FromMinutes(30);

        private readonly IPagoQrRepository _pagoQrRepository;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IVentaService _ventaService;

        public PagoQrService(
            IPagoQrRepository pagoQrRepository, IPedidoRepository pedidoRepository, IVentaService ventaService)
        {
            _pagoQrRepository = pagoQrRepository;
            _pedidoRepository = pedidoRepository;
            _ventaService = ventaService;
        }

        public async Task<PagoQrDto> GenerarAsync(PedidoCliente pedido)
        {
            var pago = new PagoQr
            {
                Id = Guid.NewGuid(),
                IdPedido = pedido.Id,
                FechaGeneracion = DateTime.UtcNow,
                MontoSolicitado = pedido.TotalCotizado,
                Estado = "Pendiente"
            };

            await _pagoQrRepository.AgregarAsync(pago);
            await _pagoQrRepository.GuardarCambiosAsync();

            return MapearDto(pago);
        }

        public async Task<PagoQrDto> ConfirmarPorWebhookAsync(ConfirmarPagoWebhookRequest request)
        {
            var pago = await _pagoQrRepository.ObtenerPorIdAsync(request.IdPago)
                ?? throw new NoEncontradoException($"No se encontró el pago QR con id {request.IdPago}.");

            if (pago.Estado != "Pendiente")
            {
                throw new ConflictoException("Este pago ya fue procesado.");
            }

            if (DateTime.UtcNow - pago.FechaGeneracion > VigenciaQr)
            {
                pago.Estado = "Expirado";
                await _pagoQrRepository.GuardarCambiosAsync();
                throw new ConflictoException("El código QR expiró. Genere uno nuevo para continuar.");
            }

            pago.FechaPago = DateTime.UtcNow;
            pago.MontoConfirmado = request.MontoConfirmado;
            pago.Diferencia = request.MontoConfirmado - pago.MontoSolicitado;
            pago.CanalPago = request.CanalPago;
            pago.CodigoRespuestaBanco = request.CodigoRespuestaBanco;
            pago.Estado = request.Aprobado ? "Confirmado" : "Rechazado";

            if (request.Aprobado)
            {
                var pedido = await _pedidoRepository.ObtenerPorIdAsync(pago.IdPedido)
                    ?? throw new NoEncontradoException($"No se encontró el pedido con id {pago.IdPedido}.");
                pedido.Estado = "Confirmado";

                // Simulación de arqueo (sin webhook bancario real todavía, ver CLAUDE.md): el
                // dinero de este pedido ya debe reflejarse en la caja del turno abierto.
                await _ventaService.RegistrarVentaSimuladaPorPedidoQrAsync(
                    pedido.Id, pedido.Configuracion?.IdItemProductoBase, request.MontoConfirmado);
            }

            await _pagoQrRepository.GuardarCambiosAsync();

            return MapearDto(pago);
        }

        public async Task<PagoQrDto> RegenerarAsync(Guid idPedido)
        {
            var pedido = await _pedidoRepository.ObtenerPorIdAsync(idPedido)
                ?? throw new NoEncontradoException($"No se encontró el pedido con id {idPedido}.");

            if (pedido.Estado != "Pendiente QR")
            {
                throw new ConflictoException("Solo se puede regenerar el QR de un pedido pendiente de pago.");
            }

            var pagoActual = await _pagoQrRepository.ObtenerMasRecientePorPedidoAsync(idPedido);
            if (pagoActual is not null && pagoActual.Estado == "Pendiente")
            {
                if (DateTime.UtcNow - pagoActual.FechaGeneracion <= VigenciaQr)
                {
                    throw new ConflictoException("El código QR actual todavía es válido.");
                }

                pagoActual.Estado = "Expirado";
            }

            return await GenerarAsync(pedido);
        }

        public static PagoQrDto MapearDto(PagoQr pago) => new()
        {
            Id = pago.Id,
            IdPedido = pago.IdPedido,
            FechaGeneracion = pago.FechaGeneracion,
            FechaPago = pago.FechaPago,
            MontoSolicitado = pago.MontoSolicitado,
            MontoConfirmado = pago.MontoConfirmado,
            Diferencia = pago.Diferencia,
            CanalPago = pago.CanalPago,
            CodigoRespuestaBanco = pago.CodigoRespuestaBanco,
            Estado = pago.Estado,
            ExpiraEn = pago.FechaGeneracion.Add(VigenciaQr),
            PayloadSimulado = $"QR-SIMULADO|pago={pago.Id:N}|monto={pago.MontoSolicitado:F2}"
        };
    }
}
