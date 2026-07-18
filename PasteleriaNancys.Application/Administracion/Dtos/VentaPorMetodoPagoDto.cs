namespace PasteleriaNancys.Application.Administracion.Dtos
{
    public class VentaPorMetodoPagoDto
    {
        public string MetodoPago { get; set; } = string.Empty;
        public int NumeroVentas { get; set; }
        public decimal TotalVendido { get; set; }
    }
}
