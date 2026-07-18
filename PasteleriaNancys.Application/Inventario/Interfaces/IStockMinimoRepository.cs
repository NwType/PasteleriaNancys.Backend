using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IStockMinimoRepository
    {
        Task<StockMinimo?> ObtenerPorItemAsync(Guid idItem);
        Task<StockMinimo?> ObtenerPorIdAsync(Guid id);
        Task<List<StockMinimo>> ObtenerTodosAsync();
        Task AgregarAsync(StockMinimo stockMinimo);
        void Eliminar(StockMinimo stockMinimo);
        Task GuardarCambiosAsync();
    }
}
