namespace PasteleriaNancys.Application.Reportes.Dtos
{
    public class InsumoRequeridoDto
    {
        public Guid IdItem { get; set; }
        public string CodigoReferencia { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal CantidadNecesaria { get; set; }
        public decimal StockDisponible { get; set; }
        public decimal CantidadFaltante { get; set; }
    }
}
