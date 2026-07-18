namespace PasteleriaNancys.Application.Administracion.Dtos
{
    public class ReporteVentasDto
    {
        public string Periodo { get; set; } = string.Empty;
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public decimal TotalVentas { get; set; }
        public int NumeroVentas { get; set; }
        public bool SinVentas { get; set; }
        public List<VentaPorProductoDto> VentasPorProducto { get; set; } = new();
        public List<VentaPorMetodoPagoDto> VentasPorMetodoPago { get; set; } = new();
        public List<VentaPorVendedoraDto> VentasPorVendedora { get; set; } = new();
    }
}
