using PasteleriaNancys.Application.Seguridad.Dtos;

namespace PasteleriaNancys.Application.Seguridad.Interfaces
{
    public interface IUsuarioService
    {
        Task<UsuarioDto> CrearAsync(CrearUsuarioRequest request);
        Task<List<UsuarioDto>> ObtenerTodosAsync();
        Task<UsuarioDto> ObtenerPorIdAsync(Guid id);
        Task<UsuarioDto> ActualizarAsync(Guid id, ActualizarUsuarioRequest request);
        Task DesactivarAsync(Guid id);
        Task DesbloquearAsync(Guid id);
    }
}
