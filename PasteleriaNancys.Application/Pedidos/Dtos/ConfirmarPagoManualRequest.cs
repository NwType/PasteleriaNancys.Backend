namespace PasteleriaNancys.Application.Pedidos.Dtos
{
    // Confirmación manual por el Administrador (CU: verifica el comprobante enviado por
    // WhatsApp y confirma) — distinto del webhook bancario real, que todavía no existe
    // (ver CLAUDE.md, pendiente de permisos de API de pruebas de un banco).
    public class ConfirmarPagoManualRequest
    {
        public decimal MontoConfirmado { get; set; }
        public string CanalPago { get; set; } = string.Empty;
    }
}
