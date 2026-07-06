namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class ProductoAfectadoDto
    {
        public Guid IdItem { get; set; }
        public string CodigoReferencia { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }
}
