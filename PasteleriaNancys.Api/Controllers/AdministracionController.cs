using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Administracion.Dtos;
using PasteleriaNancys.Application.Administracion.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/administracion")]
    [Authorize(Roles = "Administrador")]
    public class AdministracionController : ControllerBase
    {
        private readonly IAdministracionService _administracionService;

        public AdministracionController(IAdministracionService administracionService)
        {
            _administracionService = administracionService;
        }

        [HttpGet("arqueos")]
        public async Task<ActionResult<List<ArqueoAuditoriaDto>>> ConsultarArqueos(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta,
            [FromQuery] Guid? idUsuarioResponsable,
            [FromQuery] bool? soloDiferenciasSignificativas)
        {
            return Ok(await _administracionService.ConsultarArqueosAsync(desde, hasta, idUsuarioResponsable, soloDiferenciasSignificativas));
        }

        [HttpGet("arqueos/{idTurno:guid}")]
        public async Task<ActionResult<ArqueoAuditoriaDetalleDto>> ConsultarArqueoDetalle(Guid idTurno)
        {
            return Ok(await _administracionService.ConsultarArqueoDetalleAsync(idTurno));
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardDto>> ObtenerDashboard()
        {
            return Ok(await _administracionService.ObtenerDashboardAsync());
        }

        [HttpGet("reporte-ventas")]
        public async Task<ActionResult<ReporteVentasDto>> ConsultarReporteVentas(
            [FromQuery] string periodo = "dia", [FromQuery] DateTime? fecha = null)
        {
            return Ok(await _administracionService.ConsultarReporteVentasAsync(periodo, fecha));
        }

        [HttpGet("reporte-ventas/pdf")]
        public async Task<IActionResult> GenerarReporteVentasPdf(
            [FromQuery] string periodo = "dia", [FromQuery] DateTime? fecha = null)
        {
            var pdf = await _administracionService.GenerarReporteVentasPdfAsync(periodo, fecha);
            return File(pdf, "application/pdf", $"reporte-ventas-{periodo}.pdf");
        }
    }
}
