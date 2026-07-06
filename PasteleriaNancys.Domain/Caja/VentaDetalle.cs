using System;

namespace PasteleriaNancys.Domain.Caja
{
    public class VentaDetalle
    {
        public Guid Id { get; set; }
        public Guid IdVenta { get; set; }
        public Guid IdItem { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }

        // Propiedades de navegación
        public VentaPos Venta { get; set; } = null!;
    }
}
