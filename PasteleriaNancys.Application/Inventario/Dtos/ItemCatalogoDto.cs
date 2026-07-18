namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class ItemCatalogoDto
    {
        public Guid Id { get; set; }
        public string CodigoReferencia { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public int? NumeroPorciones { get; set; }
        public bool EsPersonalizable { get; set; }
        public string? CategoriaPersonalizacion { get; set; }
        public string? TipoCremaAsociado { get; set; }
        public string? ImagenUrl { get; set; }
        public string? Descripcion { get; set; }
        public string? ColorDecoracion { get; set; }
        public string? TipoMasa { get; set; }
        public Guid? IdInsumoRelleno { get; set; }
        public Guid? IdInsumoCrema { get; set; }
        public string? NombreInsumoRelleno { get; set; }
        public string? NombreInsumoCrema { get; set; }
        public bool Activo { get; set; }
    }
}
