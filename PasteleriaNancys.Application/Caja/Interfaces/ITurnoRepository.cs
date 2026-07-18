using PasteleriaNancys.Domain.Caja;

namespace PasteleriaNancys.Application.Caja.Interfaces
{
    public interface ITurnoRepository
    {
        Task<Turno?> ObtenerPorIdAsync(Guid id);
        Task<Turno?> ObtenerAbiertoPorUsuarioAsync(Guid idUsuario);
        Task<List<Turno>> ObtenerAbiertosAsync();
        Task<List<Turno>> ObtenerCerradosAsync(DateTime? desde, DateTime? hasta, Guid? idUsuarioResponsable);
        Task AgregarAsync(Turno turno);
        Task GuardarCambiosAsync();
    }
}
