using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DespachosController : ControllerBase
    {
        private readonly IDespachoService _despachoService;

        public DespachosController(IDespachoService despachoService)
        {
            _despachoService = despachoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ViajeDto>>> ObtenerTodos()
        {
            return Ok(await _despachoService.ObtenerTodosAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ViajeDto>> ObtenerPorId(Guid id)
        {
            return Ok(await _despachoService.ObtenerPorIdAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = "Encargado de Almacen,Administrador")]
        public async Task<ActionResult<ViajeDto>> CrearViaje(CrearViajeRequest request)
        {
            var viaje = await _despachoService.CrearViajeAsync(request);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = viaje.Id }, viaje);
        }

        [HttpPost("{id:guid}/productos")]
        [Authorize(Roles = "Encargado de Almacen,Administrador")]
        public async Task<ActionResult<ViajeDto>> AgregarProducto(Guid id, AgregarProductoAlViajeRequest request)
        {
            return Ok(await _despachoService.AgregarProductoAsync(id, request));
        }

        [HttpPatch("{id:guid}/confirmar-entrega")]
        [Authorize(Roles = "Vendedora,Administrador,Encargado de Almacen")]
        public async Task<ActionResult<ViajeDto>> ConfirmarEntrega(Guid id)
        {
            return Ok(await _despachoService.ConfirmarEntregaAsync(id));
        }
    }
}
