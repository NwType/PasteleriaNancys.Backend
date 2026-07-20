using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Pedidos.Dtos;
using PasteleriaNancys.Application.Pedidos.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/pedidos-web")]
    public class PedidosWebController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;

        public PedidosWebController(IPedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<PedidoDto>> Crear(CrearPedidoWebRequest request)
        {
            var pedido = await _pedidoService.CrearAsync(request);
            return CreatedAtAction(nameof(ConsultarEstado), new { whatsapp = pedido.WhatsApp, codigo = pedido.CodigoQrReferencia }, pedido);
        }

        [HttpPost("presencial")]
        [Authorize(Roles = "Vendedora")]
        public async Task<ActionResult<PedidoDto>> RegistrarPresencial(CrearPedidoWebRequest request)
        {
            var idUsuarioVendedora = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var pedido = await _pedidoService.RegistrarPresencialAsync(idUsuarioVendedora, request);
            return CreatedAtAction(nameof(ConsultarEstado), new { whatsapp = pedido.WhatsApp, codigo = pedido.CodigoQrReferencia }, pedido);
        }

        [HttpGet("estado")]
        [AllowAnonymous]
        public async Task<ActionResult<PedidoDto>> ConsultarEstado([FromQuery] string whatsapp, [FromQuery] string codigo)
        {
            return Ok(await _pedidoService.ConsultarEstadoAsync(whatsapp, codigo));
        }

        [HttpGet("pendientes")]
        [Authorize(Roles = "Vendedora")]
        public async Task<ActionResult<List<PedidoDto>>> ObtenerPendientes()
        {
            return Ok(await _pedidoService.ObtenerPendientesAsync());
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<List<PedidoDto>>> ObtenerTodos([FromQuery] string? estado, [FromQuery] DateTime? fechaEntrega)
        {
            return Ok(await _pedidoService.ObtenerTodosAsync(estado, fechaEntrega));
        }

        [HttpPatch("{id:guid}/estado")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<PedidoDto>> CambiarEstado(Guid id, CambiarEstadoPedidoRequest request)
        {
            // Quién ejecuta el cambio — queda como IdUsuarioRegistro en los consumos que
            // dispara el pase a "En Producción" de una torta personalizable.
            var idUsuario = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _pedidoService.CambiarEstadoAsync(id, idUsuario, request));
        }

        [HttpPost("{id:guid}/cancelar")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<PedidoDto>> Cancelar(Guid id, CancelarPedidoRequest request)
        {
            return Ok(await _pedidoService.CancelarAsync(id, request));
        }
    }
}
