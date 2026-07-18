using PasteleriaNancys.Domain.Pedidos;

namespace PasteleriaNancys.Application.Pedidos.Interfaces
{
    public interface IPedidoRepository
    {
        Task<PedidoCliente?> ObtenerPorIdAsync(Guid id);
        Task<PedidoCliente?> ObtenerPorWhatsAppYCodigoAsync(string whatsApp, string codigoReferencia);
        Task<List<PedidoCliente>> ObtenerPendientesAsync();
        Task<List<PedidoCliente>> ObtenerTodosAsync(string? estado, DateTime? fechaEntrega);
        Task AgregarAsync(PedidoCliente pedido);
        Task GuardarCambiosAsync();
    }
}
