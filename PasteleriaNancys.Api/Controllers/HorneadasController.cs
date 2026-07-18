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
    public class HorneadasController : ControllerBase
    {
        private readonly IConsumoService _consumoService;

        public HorneadasController(IConsumoService consumoService)
        {
            _consumoService = consumoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<HorneadaDto>>> ObtenerTodas()
        {
            return Ok(await _consumoService.ObtenerHorneadasAsync());
        }

        [HttpPost]
        public async Task<ActionResult<HorneadaDto>> Registrar(RegistrarHorneadaRequest request)
        {
            var idUsuario = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var horneada = await _consumoService.RegistrarHorneadaAsync(idUsuario, request);
            return Ok(horneada);
        }
    }
}
