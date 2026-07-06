using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface ILoteRepository
    {
        Task<LotePeps?> ObtenerPorIdAsync(Guid id);
        Task<List<LotePeps>> ObtenerTodosAsync();
        Task AgregarAsync(LotePeps lote);
        Task GuardarCambiosAsync();
        Task<List<LotePepsOrdenado>> ObtenerProximosACaducarAsync(int diasLimite);
        Task<decimal> ObtenerStockDisponibleTotalAsync(Guid idItem);
        Task<List<Guid>> ObtenerProveedoresPorItemAsync(Guid idItem);
    }
}
