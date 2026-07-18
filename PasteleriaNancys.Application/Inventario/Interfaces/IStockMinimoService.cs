using PasteleriaNancys.Application.Inventario.Dtos;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IStockMinimoService
    {
        Task<StockMinimoDto> ConfigurarAsync(ConfigurarStockMinimoRequest request);
        Task<List<StockMinimoDto>> ObtenerTodosAsync();
        Task DesactivarAsync(Guid id);
        Task EliminarAsync(Guid id);
    }
}
