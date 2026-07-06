using PasteleriaNancys.Domain.Caja;

namespace PasteleriaNancys.Application.Caja.Interfaces
{
    public interface ITurnoRepository
    {
        Task<Turno?> ObtenerPorIdAsync(Guid id);
        Task<Turno?> ObtenerAbiertoPorUsuarioAsync(Guid idUsuario);
        Task AgregarAsync(Turno turno);
        Task GuardarCambiosAsync();
    }
}
