namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class ActualizarItemCatalogoRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public int? NumeroPorciones { get; set; }
        // EsPersonalizable se deriva de Categoria == "Tortas Personalizables" en el servicio.
        public string? CategoriaPersonalizacion { get; set; }
        public string? TipoCremaAsociado { get; set; }
        public string? Descripcion { get; set; }
        public string? ColorDecoracion { get; set; }
        public string? TipoMasa { get; set; }
        public Guid? IdInsumoRelleno { get; set; }
        public Guid? IdInsumoCrema { get; set; }
    }
}
