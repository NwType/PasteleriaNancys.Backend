namespace PasteleriaNancys.Application.Pedidos.Dtos
{
    public class PagoQrDto
    {
        public Guid Id { get; set; }
        public Guid IdPedido { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public DateTime? FechaPago { get; set; }
        public decimal MontoSolicitado { get; set; }
        public decimal? MontoConfirmado { get; set; }
        public decimal? Diferencia { get; set; }
        public string? CanalPago { get; set; }
        public string? CodigoRespuestaBanco { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime ExpiraEn { get; set; }

        /// <summary>
        /// Contenido simulado del código QR (no hay integración bancaria real todavía).
        /// No se persiste: se recalcula de forma determinística a partir del pago.
        /// </summary>
        public string PayloadSimulado { get; set; } = string.Empty;
    }
}
