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

        // Solo poblados por ConsultarPanelAsync (panel consolidado); en
        // ConsultarInsumosCriticosAsync quedan vacíos para mantener esa consulta liviana.
        public List<ProductoAfectadoDto> ProductosAfectados { get; set; } = new();
        public List<PedidoAfectadoDto> PedidosAfectados { get; set; } = new();
    }
}
