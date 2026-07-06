using System;

namespace PasteleriaNancys.Application.Caja.Dtos
{
    public class VentaDetalleDto
    {
        public Guid Id { get; set; }
        public Guid IdItem { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
