using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LotesController : ControllerBase
    {
        private readonly ILoteService _loteService;

        public LotesController(ILoteService loteService)
        {
            _loteService = loteService;
        }

        [HttpGet]
        public async Task<ActionResult<List<LoteDto>>> ObtenerTodos()
        {
            return Ok(await _loteService.ObtenerTodosAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<LoteDto>> ObtenerPorId(Guid id)
        {
            return Ok(await _loteService.ObtenerPorIdAsync(id));
        }

        [HttpGet("proximos-a-caducar")]
        public async Task<ActionResult<List<LoteProximoACaducarDto>>> ConsultarProximosACaducar()
        {
            return Ok(await _loteService.ConsultarProximosACaducarAsync());
        }

        [HttpPost]
        [Authorize(Roles = "Encargado de Almacen")]
        public async Task<ActionResult<LoteDto>> Registrar(RegistrarLoteRequest request)
        {
            var lote = await _loteService.RegistrarAsync(request);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = lote.Id }, lote);
        }

        [HttpPatch("{id:guid}/estado")]
        [Authorize(Roles = "Encargado de Almacen")]
        public async Task<ActionResult<LoteDto>> ActualizarEstado(Guid id, ActualizarEstadoLoteRequest request)
        {
            return Ok(await _loteService.ActualizarEstadoAsync(id, request));
        }
    }
}
