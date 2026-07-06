using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IRecetaRepository
    {
        Task<RecetaItem?> ObtenerPorIdAsync(Guid id);
        Task<RecetaItem?> ObtenerPorParAsync(Guid idItemTerminado, Guid idItemInsumo);
        Task<List<RecetaItem>> ObtenerPorProductoTerminadoAsync(Guid idItemTerminado);
        Task<List<RecetaItem>> ObtenerPorInsumoAsync(Guid idItemInsumo);
        Task AgregarAsync(RecetaItem receta);
        void Eliminar(RecetaItem receta);
        Task GuardarCambiosAsync();
    }
}
