using System;

namespace PasteleriaNancys.Domain.Pedidos
{
    public class PagoQr
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

        // Propiedad de navegación
        public PedidoCliente Pedido { get; set; } = null!;
    }
}
