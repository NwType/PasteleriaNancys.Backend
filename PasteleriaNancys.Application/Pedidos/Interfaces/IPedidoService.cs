using PasteleriaNancys.Application.Pedidos.Dtos;

namespace PasteleriaNancys.Application.Pedidos.Interfaces
{
    public interface IPedidoService
    {
        Task<PedidoDto> CrearAsync(CrearPedidoWebRequest request);
        Task<PedidoDto> RegistrarPresencialAsync(Guid idUsuarioVendedora, CrearPedidoWebRequest request);
        Task<PedidoDto> ConsultarEstadoAsync(string whatsApp, string codigoReferencia);
        Task<List<PedidoDto>> ObtenerPendientesAsync();
        Task<List<PedidoDto>> ObtenerTodosAsync(string? estado, DateTime? fechaEntrega);
        Task<PedidoDto> CambiarEstadoAsync(Guid idPedido, CambiarEstadoPedidoRequest request);
        Task<PedidoDto> CancelarAsync(Guid idPedido, CancelarPedidoRequest request);
        Task<List<TablaPrecioPorcionesDto>> ObtenerTablaPrecioPorcionesAsync(Guid idItemTerminado);
    }
}
