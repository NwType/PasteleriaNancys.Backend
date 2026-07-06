using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecetasController : ControllerBase
    {
        private readonly IRecetaService _recetaService;

        public RecetasController(IRecetaService recetaService)
        {
            _recetaService = recetaService;
        }

        [HttpGet("por-producto/{idItemTerminado:guid}")]
        public async Task<ActionResult<List<RecetaItemDto>>> ObtenerPorProductoTerminado(Guid idItemTerminado)
        {
            return Ok(await _recetaService.ObtenerPorProductoTerminadoAsync(idItemTerminado));
        }

        [HttpPost]
        [Authorize(Roles = "Encargado de Almacen")]
        public async Task<ActionResult<RecetaItemDto>> Crear(CrearRecetaItemRequest request)
        {
            var receta = await _recetaService.CrearAsync(request);
            return Ok(receta);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Encargado de Almacen")]
        public async Task<IActionResult> Eliminar(Guid id)
        {
            await _recetaService.EliminarAsync(id);
            return NoContent();
        }
    }
}
