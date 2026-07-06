namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class InsumoCriticoDto
    {
        public Guid IdItem { get; set; }
        public string CodigoReferencia { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal StockActual { get; set; }
        public decimal CantidadMinima { get; set; }
        public string NivelAlerta { get; set; } = string.Empty;
        public ProveedorResumenDto? ProveedorSugerido { get; set; }
        public List<ProveedorResumenDto> ProveedoresAlternativos { get; set; } = new();
    }
}
