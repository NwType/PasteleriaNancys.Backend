using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Seguridad.Dtos;
using PasteleriaNancys.Application.Seguridad.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IRolService _rolService;

        public RolesController(IRolService rolService)
        {
            _rolService = rolService;
        }

        [HttpGet]
        public async Task<ActionResult<List<RolDto>>> ObtenerTodos()
        {
            return Ok(await _rolService.ObtenerTodosAsync());
        }

        [HttpGet("{idRol:int}")]
        public async Task<ActionResult<RolDto>> ObtenerPorId(int idRol)
        {
            return Ok(await _rolService.ObtenerPorIdAsync(idRol));
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<RolDto>> Crear(CrearRolRequest request)
        {
            var rol = await _rolService.CrearAsync(request);
            return CreatedAtAction(nameof(ObtenerPorId), new { idRol = rol.IdRol }, rol);
        }

        [HttpPut("{idRol:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<RolDto>> Actualizar(int idRol, ActualizarRolRequest request)
        {
            return Ok(await _rolService.ActualizarAsync(idRol, request));
        }

        [HttpDelete("{idRol:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int idRol)
        {
            await _rolService.EliminarAsync(idRol);
            return NoContent();
        }
    }
}
