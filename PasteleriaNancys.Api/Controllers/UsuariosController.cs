using System.Security.Claims;
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

        // [AllowAnonymous] solo cubre el arranque en frío: si el sistema no tiene NINGÚN
        // usuario todavía, se permite crear el primer administrador sin token. En cuanto existe
        // al menos un usuario, se exige sesión con Rol = Administrador (ver chequeo abajo) —
        // así se cierra el hueco de que cualquiera pudiera crear cuentas Administrador después
        // del arranque inicial.
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UsuarioDto>> Crear(CrearUsuarioRequest request)
        {
            if (await _usuarioService.ExisteAlgunoAsync())
            {
                if (User.Identity?.IsAuthenticated != true)
                {
                    return Unauthorized();
                }

                if (!User.IsInRole("Administrador"))
                {
                    return Forbid();
                }
            }

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
            var idSolicitante = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _usuarioService.DesactivarAsync(id, idSolicitante);
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
