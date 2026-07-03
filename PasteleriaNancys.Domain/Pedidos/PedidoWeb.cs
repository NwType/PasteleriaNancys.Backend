using System;

namespace PasteleriaNancys.Domain.Pedidos
{
    public class PedidoWeb
    {
        public Guid Id { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime FechaEntrega { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
