using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Pedidos.Dtos;
using PasteleriaNancys.Application.Pedidos.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/portafolio")]
    public class PortafolioController : ControllerBase
    {
        private static readonly string[] ContentTypesPermitidos = { "image/jpeg", "image/png", "image/webp" };
        private const long TamanoMaximoBytes = 5 * 1024 * 1024;

        private readonly IPortafolioImagenService _portafolioImagenService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PortafolioController(IPortafolioImagenService portafolioImagenService, IWebHostEnvironment webHostEnvironment)
        {
            _portafolioImagenService = portafolioImagenService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<PortafolioImagenDto>>> Obtener([FromQuery] string? categoria)
        {
            if (!string.IsNullOrWhiteSpace(categoria))
            {
                return Ok(await _portafolioImagenService.ObtenerPorCategoriaAsync(categoria));
            }

            return Ok(await _portafolioImagenService.ObtenerTodasAsync());
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<PortafolioImagenDto>> Subir([FromForm] string categoria, [FromForm] string? descripcion, IFormFile archivo)
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
            var nombreArchivo = $"{Guid.NewGuid():N}{extension}";
            var carpetaDestino = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "portafolio");
            Directory.CreateDirectory(carpetaDestino);
            var rutaDestino = Path.Combine(carpetaDestino, nombreArchivo);

            await using (var stream = new FileStream(rutaDestino, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            var imagenUrl = $"/uploads/portafolio/{nombreArchivo}";
            var creada = await _portafolioImagenService.AgregarAsync(categoria, descripcion, imagenUrl);
            return CreatedAtAction(nameof(Obtener), creada);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Desactivar(Guid id)
        {
            await _portafolioImagenService.DesactivarAsync(id);
            return NoContent();
        }
    }
}
