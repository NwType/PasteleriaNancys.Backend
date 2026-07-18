using PasteleriaNancys.Application.Inventario.Dtos;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IItemCatalogoService
    {
        Task<ItemCatalogoDto> CrearAsync(CrearItemCatalogoRequest request);
        Task<List<ItemCatalogoDto>> ObtenerTodosAsync();
        Task<List<ItemCatalogoDto>> ObtenerInsumosPersonalizacionAsync();
        Task<List<(ItemCatalogoDto Item, decimal StockDisponible)>> ObtenerCatalogoPublicoConStockAsync();
        Task<(ItemCatalogoDto Item, decimal StockDisponible)> ObtenerCatalogoPublicoConStockPorIdAsync(Guid id);
        Task<ItemCatalogoDto> ObtenerPorIdAsync(Guid id);
        Task<ItemCatalogoDto> ActualizarAsync(Guid id, ActualizarItemCatalogoRequest request);
        Task DesactivarAsync(Guid id);
        Task<ItemCatalogoDto> ActualizarImagenUrlAsync(Guid id, string imagenUrl);
    }
}
