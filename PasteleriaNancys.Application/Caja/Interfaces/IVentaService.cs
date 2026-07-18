using PasteleriaNancys.Application.Caja.Dtos;

namespace PasteleriaNancys.Application.Caja.Interfaces
{
    public interface IVentaService
    {
        Task<VentaDto> ObtenerPorIdAsync(Guid id);
        Task<VentaDto> RegistrarAsync(RegistrarVentaRequest request);
        Task<VentaDto> AnularAsync(Guid idVenta, AnularVentaRequest request);

        // Puente Pedidos→Caja para pagos QR confirmados (simulado hasta tener webhook bancario
        // real, ver CLAUDE.md). No descuenta stock PEPS — el pedido puede estar pagado antes de
        // que la torta exista físicamente en vitrina.
        Task RegistrarVentaSimuladaPorPedidoQrAsync(Guid idPedido, Guid? idItemProductoBase, decimal monto);
    }
}
