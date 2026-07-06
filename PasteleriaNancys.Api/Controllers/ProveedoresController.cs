using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProveedoresController : ControllerBase
    {
        private readonly IProveedorService _proveedorService;

        public ProveedoresController(IProveedorService proveedorService)
        {
            _proveedorService = proveedorService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProveedorDto>>> ObtenerTodos()
        {
            return Ok(await _proveedorService.ObtenerTodosAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProveedorDto>> ObtenerPorId(Guid id)
        {
            return Ok(await _proveedorService.ObtenerPorIdAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = "Encargado de Almacen")]
        public async Task<ActionResult<ProveedorDto>> Crear(CrearProveedorRequest request)
        {
            var proveedor = await _proveedorService.CrearAsync(request);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = proveedor.Id }, proveedor);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Encargado de Almacen")]
        public async Task<ActionResult<ProveedorDto>> Actualizar(Guid id, ActualizarProveedorRequest request)
        {
            return Ok(await _proveedorService.ActualizarAsync(id, request));
        }

        [HttpPatch("{id:guid}/desactivar")]
        [Authorize(Roles = "Encargado de Almacen")]
        public async Task<IActionResult> Desactivar(Guid id)
        {
            await _proveedorService.DesactivarAsync(id);
            return NoContent();
        }
    }
}
