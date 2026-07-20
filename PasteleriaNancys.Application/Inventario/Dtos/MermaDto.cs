namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class MermaDto
    {
        public Guid Id { get; set; }
        public Guid IdItem { get; set; }
        public string CodigoReferencia { get; set; } = string.Empty;
        public string NombreItem { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public Guid IdLote { get; set; }
        public decimal Cantidad { get; set; }
        public string TipoMerma { get; set; } = string.Empty;
        public string? Motivo { get; set; }
        public DateTime Fecha { get; set; }
    }
}
