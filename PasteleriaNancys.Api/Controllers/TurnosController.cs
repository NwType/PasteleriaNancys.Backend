using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Caja.Dtos;
using PasteleriaNancys.Application.Caja.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TurnosController : ControllerBase
    {
        private readonly ITurnoService _turnoService;

        public TurnosController(ITurnoService turnoService)
        {
            _turnoService = turnoService;
        }

        [HttpPost]
        [Authorize(Roles = "Vendedora")]
        public async Task<ActionResult<TurnoDto>> Aperturar(AperturarTurnoRequest request)
        {
            var idUsuario = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var turno = await _turnoService.AperturarAsync(idUsuario, request);
            return CreatedAtAction(nameof(ObtenerResumen), new { id = turno.Id }, turno);
        }

        [HttpGet("{id:guid}/resumen")]
        public async Task<ActionResult<ResumenTurnoDto>> ObtenerResumen(Guid id)
        {
            return Ok(await _turnoService.ObtenerResumenAsync(id));
        }

        [HttpPost("{id:guid}/cerrar")]
        [Authorize(Roles = "Vendedora")]
        public async Task<ActionResult<CierreTurnoDto>> Cerrar(Guid id, CerrarTurnoRequest request)
        {
            return Ok(await _turnoService.CerrarAsync(id, request));
        }

        [HttpGet("{id:guid}/historial")]
        public async Task<ActionResult<HistorialTurnoDto>> ConsultarHistorial(Guid id)
        {
            return Ok(await _turnoService.ConsultarHistorialAsync(id));
        }
    }
}
