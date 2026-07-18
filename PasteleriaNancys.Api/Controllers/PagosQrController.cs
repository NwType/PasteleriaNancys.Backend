using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Pedidos.Dtos;
using PasteleriaNancys.Application.Pedidos.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/pagos-qr")]
    [Authorize(Roles = "Administrador")]
    public class PagosQrController : ControllerBase
    {
        private const string WebhookSecretHeader = "X-Webhook-Secret";

        private readonly IPagoQrService _pagoQrService;
        private readonly IConfiguration _configuration;

        public PagosQrController(IPagoQrService pagoQrService, IConfiguration configuration)
        {
            _pagoQrService = pagoQrService;
            _configuration = configuration;
        }

        // Para el webhook bancario real (todavía no existe, ver CLAUDE.md) — se autentica con
        // un secreto compartido en vez de JWT porque lo llama el banco, no un usuario logueado.
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<ActionResult<PagoQrDto>> Webhook(ConfirmarPagoWebhookRequest request)
        {
            var secretoEsperado = _configuration["Webhooks:PagoQrSecret"];
            var secretoRecibido = Request.Headers[WebhookSecretHeader].ToString();

            if (string.IsNullOrEmpty(secretoEsperado) || secretoRecibido != secretoEsperado)
            {
                return Unauthorized();
            }

            return Ok(await _pagoQrService.ConfirmarPorWebhookAsync(request));
        }

        // Confirmación manual real del negocio hoy: el Administrador verifica el comprobante
        // recibido por WhatsApp y confirma acá — esto ya refleja el ingreso en el arqueo del
        // turno abierto (VentaService.RegistrarVentaSimuladaPorPedidoQrAsync). El día que exista
        // el webhook bancario real, ambos caminos siguen convergiendo en el mismo servicio.
        [HttpPost("{idPago:guid}/confirmar")]
        public async Task<ActionResult<PagoQrDto>> ConfirmarManual(Guid idPago, ConfirmarPagoManualRequest request)
        {
            return Ok(await _pagoQrService.ConfirmarPorWebhookAsync(new ConfirmarPagoWebhookRequest
            {
                IdPago = idPago,
                Aprobado = true,
                MontoConfirmado = request.MontoConfirmado,
                CanalPago = request.CanalPago
            }));
        }

        [HttpPost("{idPedido:guid}/regenerar")]
        [AllowAnonymous]
        public async Task<ActionResult<PagoQrDto>> Regenerar(Guid idPedido)
        {
            return Ok(await _pagoQrService.RegenerarAsync(idPedido));
        }
    }
}
