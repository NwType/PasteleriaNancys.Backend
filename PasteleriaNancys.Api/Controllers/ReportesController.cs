using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Reportes.Dtos;
using PasteleriaNancys.Application.Reportes.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/reportes")]
    [Authorize(Roles = "Encargado de Almacen")]
    public class ReportesController : ControllerBase
    {
        private readonly IReporteService _reporteService;

        public ReportesController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        [HttpGet("proyeccion-horneado")]
        public async Task<ActionResult<ProyeccionHorneadoDto>> ObtenerProyeccionHorneado()
        {
            return Ok(await _reporteService.ObtenerProyeccionHorneadoAsync());
        }
    }
}
