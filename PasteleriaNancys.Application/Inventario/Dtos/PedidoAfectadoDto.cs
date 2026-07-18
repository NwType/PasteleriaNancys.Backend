namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class PedidoAfectadoDto
    {
        public Guid IdPedido { get; set; }
        public string? CodigoQrReferencia { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaEntregaSolicitada { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
    }
}
