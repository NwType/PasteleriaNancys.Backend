namespace PasteleriaNancys.Application.Pedidos.Dtos
{
    public class ConfirmarPagoWebhookRequest
    {
        public Guid IdPago { get; set; }
        public bool Aprobado { get; set; }
        public decimal MontoConfirmado { get; set; }
        public string CanalPago { get; set; } = string.Empty;
        public string? CodigoRespuestaBanco { get; set; }
    }
}
