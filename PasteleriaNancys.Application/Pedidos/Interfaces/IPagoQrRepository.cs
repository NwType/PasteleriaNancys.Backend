using PasteleriaNancys.Domain.Pedidos;

namespace PasteleriaNancys.Application.Pedidos.Interfaces
{
    public interface IPagoQrRepository
    {
        Task<PagoQr?> ObtenerPorIdAsync(Guid id);
        Task<PagoQr?> ObtenerMasRecientePorPedidoAsync(Guid idPedido);
        Task AgregarAsync(PagoQr pago);
        Task GuardarCambiosAsync();
    }
}
