using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AlertasController : ControllerBase
    {
        private readonly IAlertaService _alertaService;

        public AlertasController(IAlertaService alertaService)
        {
            _alertaService = alertaService;
        }

        [HttpGet("insumos-criticos")]
        public async Task<ActionResult<List<InsumoCriticoDto>>> ObtenerInsumosCriticos()
        {
            return Ok(await _alertaService.ConsultarInsumosCriticosAsync());
        }

        [HttpGet("productos-afectados/{idInsumo:guid}")]
        public async Task<ActionResult<List<ProductoAfectadoDto>>> ObtenerProductosAfectados(Guid idInsumo)
        {
            return Ok(await _alertaService.ConsultarProductosAfectadosAsync(idInsumo));
        }
    }
}
