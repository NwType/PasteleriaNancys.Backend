using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Caja.Dtos;
using PasteleriaNancys.Application.Caja.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VentasController : ControllerBase
    {
        private readonly IVentaService _ventaService;

        public VentasController(IVentaService ventaService)
        {
            _ventaService = ventaService;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<VentaDto>> ObtenerPorId(Guid id)
        {
            return Ok(await _ventaService.ObtenerPorIdAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = "Vendedora")]
        public async Task<ActionResult<VentaDto>> Registrar(RegistrarVentaRequest request)
        {
            var venta = await _ventaService.RegistrarAsync(request);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = venta.Id }, venta);
        }

        [HttpPost("{id:guid}/anular")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<VentaDto>> Anular(Guid id, AnularVentaRequest request)
        {
            return Ok(await _ventaService.AnularAsync(id, request));
        }
    }
}
