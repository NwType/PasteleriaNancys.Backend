namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class CrearItemCatalogoRequest
    {
        public string CodigoReferencia { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
    }
}
