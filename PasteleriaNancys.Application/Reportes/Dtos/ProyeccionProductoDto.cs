namespace PasteleriaNancys.Application.Reportes.Dtos
{
    public class ProyeccionProductoDto
    {
        public Guid IdItem { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int UnidadesPedidosConfirmados { get; set; }
        public decimal StockFaltanteVitrina { get; set; }
        public decimal PromedioVentasDiarias { get; set; }
        public decimal MultiplicadorAplicado { get; set; }
        public string? NombreEventoFestivo { get; set; }
        public int TotalHornear { get; set; }
    }
}
