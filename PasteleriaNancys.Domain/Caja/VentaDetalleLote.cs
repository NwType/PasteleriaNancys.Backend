using System;

namespace PasteleriaNancys.Domain.Caja
{
    /// <summary>
    /// Registra exactamente de qué lote (y cuánto) se descontó cada línea de venta, para poder
    /// revertir una anulación al lote correcto en vez de aproximar reponiendo al lote más reciente.
    /// </summary>
    public class VentaDetalleLote
    {
        public Guid Id { get; set; }
        public Guid IdVentaDetalle { get; set; }
        public Guid IdLote { get; set; }
        public decimal CantidadDescontada { get; set; }

        // Propiedad de navegación
        public VentaDetalle VentaDetalle { get; set; } = null!;
    }
}
