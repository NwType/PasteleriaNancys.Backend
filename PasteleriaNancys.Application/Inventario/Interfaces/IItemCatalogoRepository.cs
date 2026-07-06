using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IItemCatalogoRepository
    {
        Task<ItemCatalogo?> ObtenerPorIdAsync(Guid id);
        Task<ItemCatalogo?> ObtenerPorCodigoAsync(string codigoReferencia);
        Task<List<ItemCatalogo>> ObtenerTodosAsync();
        Task AgregarAsync(ItemCatalogo item);
        Task GuardarCambiosAsync();
    }
}
