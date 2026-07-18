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
    public class ConsumosController : ControllerBase
    {
        private readonly IConsumoService _consumoService;

        public ConsumosController(IConsumoService consumoService)
        {
            _consumoService = consumoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ConsumoInsumoDto>>> ObtenerTodos()
        {
            return Ok(await _consumoService.ObtenerConsumosManualesAsync());
        }

        [HttpPost]
        public async Task<ActionResult<ConsumoInsumoDto>> Registrar(RegistrarConsumoRequest request)
        {
            var idUsuario = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var consumo = await _consumoService.RegistrarConsumoManualAsync(idUsuario, request);
            return Ok(consumo);
        }
    }
}
