namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class CrearItemCatalogoRequest
    {
        // El código de referencia (PT-TORTA-NNN / MP-NNN) se genera en el servidor
        // (ItemCatalogoService.GenerarCodigoAsync) — el usuario ya no lo escribe a mano.
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public int? NumeroPorciones { get; set; }
        // EsPersonalizable se deriva de Categoria == "Tortas Personalizables" en el servicio —
        // ya no es un campo aparte que el usuario tenga que responder dos veces.
        public string? CategoriaPersonalizacion { get; set; }
        public string? TipoCremaAsociado { get; set; }
        public string? Descripcion { get; set; }
        public string? ColorDecoracion { get; set; }
        public string? TipoMasa { get; set; }
        public Guid? IdInsumoRelleno { get; set; }
        public Guid? IdInsumoCrema { get; set; }
    }
}
