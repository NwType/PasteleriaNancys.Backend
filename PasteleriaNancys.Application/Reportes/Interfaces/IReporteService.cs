using PasteleriaNancys.Application.Reportes.Dtos;

namespace PasteleriaNancys.Application.Reportes.Interfaces
{
    public interface IReporteService
    {
        Task<ProyeccionHorneadoDto> ObtenerProyeccionHorneadoAsync();
    }
}
