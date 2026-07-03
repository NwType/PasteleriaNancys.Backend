using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Seguridad.Dtos;
using PasteleriaNancys.Application.Seguridad.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<List<UsuarioDto>>> ObtenerTodos()
        {
            return Ok(await _usuarioService.ObtenerTodosAsync());
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<UsuarioDto>> ObtenerPorId(Guid id)
        {
            return Ok(await _usuarioService.ObtenerPorIdAsync(id));
        }

        // Sin [Authorize]: permite crear el primer usuario administrador antes de tener un token.
        // En un entorno productivo, restringir o proteger este endpoint una vez exista al menos un administrador.
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UsuarioDto>> Crear(CrearUsuarioRequest request)
        {
            var usuario = await _usuarioService.CrearAsync(request);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = usuario.Id }, usuario);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<UsuarioDto>> Actualizar(Guid id, ActualizarUsuarioRequest request)
        {
            return Ok(await _usuarioService.ActualizarAsync(id, request));
        }

        [HttpPatch("{id:guid}/desactivar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Desactivar(Guid id)
        {
            await _usuarioService.DesactivarAsync(id);
            return NoContent();
        }

        [HttpPatch("{id:guid}/desbloquear")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Desbloquear(Guid id)
        {
            await _usuarioService.DesbloquearAsync(id);
            return NoContent();
        }
    }
}
