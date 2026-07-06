using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IStockMinimoRepository
    {
        Task<StockMinimo?> ObtenerPorItemAsync(Guid idItem);
        Task<List<StockMinimo>> ObtenerTodosAsync();
        Task AgregarAsync(StockMinimo stockMinimo);
        Task GuardarCambiosAsync();
    }
}
