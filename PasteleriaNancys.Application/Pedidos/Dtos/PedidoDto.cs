namespace PasteleriaNancys.Application.Pedidos.Dtos
{
    public class PedidoDto
    {
        public Guid Id { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string WhatsApp { get; set; } = string.Empty;
        public DateTime FechaSolicitud { get; set; }
        public DateTime FechaEntregaSolicitada { get; set; }
        public decimal TotalCotizado { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? CodigoQrReferencia { get; set; }
        public string? Observaciones { get; set; }
        public Guid? IdUsuarioVendedora { get; set; }
        public ConfiguracionPedidoDto Configuracion { get; set; } = new();
        public PagoQrDto? PagoQrActual { get; set; }
    }
}
