using PasteleriaNancys.Domain.Seguridad;

namespace PasteleriaNancys.Application.Seguridad.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObtenerPorIdAsync(Guid id);
        Task<Usuario?> ObtenerPorCorreoAsync(string correo);
        Task<List<Usuario>> ObtenerTodosAsync();
        Task AgregarAsync(Usuario usuario);
        Task GuardarCambiosAsync();
    }
}
