using PasteleriaNancys.Application.Administracion.Dtos;

namespace PasteleriaNancys.Application.Administracion.Interfaces
{
    public interface IAdministracionService
    {
        // CU-27: Auditar arqueos de caja
        Task<List<ArqueoAuditoriaDto>> ConsultarArqueosAsync(
            DateTime? desde, DateTime? hasta, Guid? idUsuarioResponsable, bool? soloDiferenciasSignificativas);

        Task<ArqueoAuditoriaDetalleDto> ConsultarArqueoDetalleAsync(Guid idTurno);

        // CU-29: Ver dashboard general
        Task<DashboardDto> ObtenerDashboardAsync();

        // CU-30: Ver reporte de ventas
        Task<ReporteVentasDto> ConsultarReporteVentasAsync(string periodo, DateTime? fechaReferencia);
        Task<byte[]> GenerarReporteVentasPdfAsync(string periodo, DateTime? fechaReferencia);
    }
}
