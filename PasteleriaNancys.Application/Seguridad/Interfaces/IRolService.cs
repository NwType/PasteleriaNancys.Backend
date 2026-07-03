using PasteleriaNancys.Application.Seguridad.Dtos;

namespace PasteleriaNancys.Application.Seguridad.Interfaces
{
    public interface IRolService
    {
        Task<RolDto> CrearAsync(CrearRolRequest request);
        Task<List<RolDto>> ObtenerTodosAsync();
        Task<RolDto> ObtenerPorIdAsync(int idRol);
        Task<RolDto> ActualizarAsync(int idRol, ActualizarRolRequest request);
        Task EliminarAsync(int idRol);
    }
}
