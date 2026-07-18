using PasteleriaNancys.Application.Pedidos.Dtos;
using PasteleriaNancys.Domain.Pedidos;

namespace PasteleriaNancys.Application.Pedidos.Interfaces
{
    public interface IPagoQrService
    {
        Task<PagoQrDto> GenerarAsync(PedidoCliente pedido);
        Task<PagoQrDto> ConfirmarPorWebhookAsync(ConfirmarPagoWebhookRequest request);
        Task<PagoQrDto> RegenerarAsync(Guid idPedido);
    }
}
