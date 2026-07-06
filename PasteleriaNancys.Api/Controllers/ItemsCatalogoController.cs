using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ItemsCatalogoController : ControllerBase
    {
        private readonly IItemCatalogoService _itemCatalogoService;

        public ItemsCatalogoController(IItemCatalogoService itemCatalogoService)
        {
            _itemCatalogoService = itemCatalogoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ItemCatalogoDto>>> ObtenerTodos()
        {
            return Ok(await _itemCatalogoService.ObtenerTodosAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ItemCatalogoDto>> ObtenerPorId(Guid id)
        {
            return Ok(await _itemCatalogoService.ObtenerPorIdAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = "Encargado de Almacen")]
        public async Task<ActionResult<ItemCatalogoDto>> Crear(CrearItemCatalogoRequest request)
        {
            var item = await _itemCatalogoService.CrearAsync(request);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = item.Id }, item);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Encargado de Almacen")]
        public async Task<ActionResult<ItemCatalogoDto>> Actualizar(Guid id, ActualizarItemCatalogoRequest request)
        {
            return Ok(await _itemCatalogoService.ActualizarAsync(id, request));
        }

        [HttpPatch("{id:guid}/desactivar")]
        [Authorize(Roles = "Encargado de Almacen")]
        public async Task<IActionResult> Desactivar(Guid id)
        {
            await _itemCatalogoService.DesactivarAsync(id);
            return NoContent();
        }
    }
}
