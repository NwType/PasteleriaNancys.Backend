using PasteleriaNancys.Application.Caja.Dtos;

namespace PasteleriaNancys.Application.Caja.Interfaces
{
    public interface ITurnoService
    {
        Task<TurnoDto> AperturarAsync(Guid idUsuarioResponsable, AperturarTurnoRequest request);
        Task<ResumenTurnoDto> ObtenerResumenAsync(Guid idTurno);
        Task<CierreTurnoDto> CerrarAsync(Guid idTurno, CerrarTurnoRequest request);
        Task<HistorialTurnoDto> ConsultarHistorialAsync(Guid idTurno);
    }
}
