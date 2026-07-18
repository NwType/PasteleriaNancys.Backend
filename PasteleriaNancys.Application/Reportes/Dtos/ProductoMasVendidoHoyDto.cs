namespace PasteleriaNancys.Application.Reportes.Dtos
{
    public class ProductoMasVendidoHoyDto
    {
        public Guid IdItem { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal CantidadVendidaHoy { get; set; }
        public decimal StockActualVitrina { get; set; }
        public bool RecomiendaReponer { get; set; }
    }
}
