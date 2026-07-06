using PasteleriaNancys.Application.Inventario.Dtos;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IItemCatalogoService
    {
        Task<ItemCatalogoDto> CrearAsync(CrearItemCatalogoRequest request);
        Task<List<ItemCatalogoDto>> ObtenerTodosAsync();
        Task<ItemCatalogoDto> ObtenerPorIdAsync(Guid id);
        Task<ItemCatalogoDto> ActualizarAsync(Guid id, ActualizarItemCatalogoRequest request);
        Task DesactivarAsync(Guid id);
    }
}
