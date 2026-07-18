using System;
using System.Collections.Generic;

namespace PasteleriaNancys.Domain.Pedidos
{
    public class PedidoCliente
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

        // Propiedades de navegación
        public PedidoConfiguracion? Configuracion { get; set; }
        public ICollection<PagoQr> PagosQr { get; set; } = new List<PagoQr>();
    }
}
