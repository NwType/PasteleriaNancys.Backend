using PasteleriaNancys.Application.Inventario.Dtos;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IProveedorService
    {
        Task<ProveedorDto> CrearAsync(CrearProveedorRequest request);
        Task<List<ProveedorDto>> ObtenerTodosAsync();
        Task<ProveedorDto> ObtenerPorIdAsync(Guid id);
        Task<ProveedorDto> ActualizarAsync(Guid id, ActualizarProveedorRequest request);
        Task DesactivarAsync(Guid id);
    }
}
