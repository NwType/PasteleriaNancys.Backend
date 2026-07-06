using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Caja.Dtos;
using PasteleriaNancys.Application.Caja.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/turnos/{idTurno:guid}/gastos")]
    [Authorize(Roles = "Vendedora")]
    public class GastosController : ControllerBase
    {
        private readonly IGastoService _gastoService;

        public GastosController(IGastoService gastoService)
        {
            _gastoService = gastoService;
        }

        [HttpPost]
        public async Task<ActionResult<GastoDto>> Registrar(Guid idTurno, RegistrarGastoRequest request)
        {
            var gasto = await _gastoService.RegistrarAsync(idTurno, request);
            return CreatedAtAction(nameof(Registrar), new { idTurno }, gasto);
        }
    }
}
