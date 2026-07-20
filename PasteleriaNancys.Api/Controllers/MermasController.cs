using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Encargado de Almacen,Administrador")]
    public class MermasController : ControllerBase
    {
        private readonly IMermaService _mermaService;

        public MermasController(IMermaService mermaService)
        {
            _mermaService = mermaService;
        }

        [HttpGet]
        public async Task<ActionResult<List<MermaDto>>> ObtenerTodas()
        {
            return Ok(await _mermaService.ObtenerTodasAsync());
        }

        // Merma directa de stock: insumo dañado (ej. 5 huevos podridos de un maple),
        // accidente con un producto, lote caducado descartado.
        [HttpPost]
        public async Task<ActionResult<List<MermaDto>>> Registrar(RegistrarMermaRequest request)
        {
            var idUsuario = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _mermaService.RegistrarAsync(idUsuario, request));
        }

        // Producción fallida: explota la receta del producto y descuenta los componentes
        // arruinados como merma. La reposición se registra aparte como producción normal.
        [HttpPost("produccion-fallida")]
        public async Task<ActionResult<List<MermaDto>>> RegistrarProduccionFallida(RegistrarMermaProduccionRequest request)
        {
            var idUsuario = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _mermaService.RegistrarProduccionFallidaAsync(idUsuario, request));
        }
    }
}
