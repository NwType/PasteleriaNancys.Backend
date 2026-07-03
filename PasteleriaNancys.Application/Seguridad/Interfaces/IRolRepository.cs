using PasteleriaNancys.Domain.Seguridad;

namespace PasteleriaNancys.Application.Seguridad.Interfaces
{
    public interface IRolRepository
    {
        Task<Rol?> ObtenerPorIdAsync(int idRol);
        Task<List<Rol>> ObtenerTodosAsync();
        Task AgregarAsync(Rol rol);
        void Eliminar(Rol rol);
        Task<bool> TieneUsuariosAsync(int idRol);
        Task GuardarCambiosAsync();
    }
}
