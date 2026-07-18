using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ItemsCatalogoController : ControllerBase
    {
        private static readonly string[] ContentTypesPermitidos = { "image/jpeg", "image/png", "image/webp" };
        private const long TamanoMaximoBytes = 5 * 1024 * 1024;

        private readonly IItemCatalogoService _itemCatalogoService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ItemsCatalogoController(IItemCatalogoService itemCatalogoService, IWebHostEnvironment webHostEnvironment)
        {
            _itemCatalogoService = itemCatalogoService;
            _webHostEnvironment = webHostEnvironment;
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

        [HttpPost("{id:guid}/imagen")]
        [Authorize(Roles = "Encargado de Almacen")]
        public async Task<ActionResult<ItemCatalogoDto>> SubirImagen(Guid id, IFormFile archivo)
        {
            if (archivo is null || archivo.Length == 0)
            {
                throw new ReglaNegocioException("Debe adjuntar un archivo de imagen.");
            }

            if (archivo.Length > TamanoMaximoBytes)
            {
                throw new ReglaNegocioException("La imagen no puede superar los 5 MB.");
            }

            if (!ContentTypesPermitidos.Contains(archivo.ContentType))
            {
                throw new ReglaNegocioException("Formato de imagen no permitido. Use JPEG, PNG o WEBP.");
            }

            var extension = Path.GetExtension(archivo.FileName);
            var nombreArchivo = $"{id:N}{extension}";
            var carpetaDestino = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "productos");
            Directory.CreateDirectory(carpetaDestino);
            var rutaDestino = Path.Combine(carpetaDestino, nombreArchivo);

            await using (var stream = new FileStream(rutaDestino, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            var imagenUrl = $"/uploads/productos/{nombreArchivo}";
            return Ok(await _itemCatalogoService.ActualizarImagenUrlAsync(id, imagenUrl));
        }
    }
}
