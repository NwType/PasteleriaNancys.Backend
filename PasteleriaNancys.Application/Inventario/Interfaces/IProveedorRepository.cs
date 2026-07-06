using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IProveedorRepository
    {
        Task<Proveedor?> ObtenerPorIdAsync(Guid id);
        Task<Proveedor?> ObtenerPorNombreAsync(string nombreEmpresa);
        Task<List<Proveedor>> ObtenerTodosAsync();
        Task AgregarAsync(Proveedor proveedor);
        Task GuardarCambiosAsync();
    }
}
