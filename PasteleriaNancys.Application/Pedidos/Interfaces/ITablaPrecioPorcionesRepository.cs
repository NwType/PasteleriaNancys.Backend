using PasteleriaNancys.Domain.Pedidos;

namespace PasteleriaNancys.Application.Pedidos.Interfaces
{
    public interface ITablaPrecioPorcionesRepository
    {
        Task<TablaPrecioPorciones?> ObtenerPorProductoYPorcionesAsync(Guid idItemTerminado, int numeroPorciones);
        Task<List<TablaPrecioPorciones>> ObtenerActivosPorProductoAsync(Guid idItemTerminado);
    }
}
