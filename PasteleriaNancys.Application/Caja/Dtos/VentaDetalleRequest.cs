using System;

namespace PasteleriaNancys.Application.Caja.Dtos
{
    public class VentaDetalleRequest
    {
        public Guid IdItem { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
