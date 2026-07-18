namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class ViajeDetalleDto
    {
        public Guid Id { get; set; }
        public Guid IdLote { get; set; }
        public decimal CantidadEnviada { get; set; }
        public Guid IdItem { get; set; }
        public string NombreItem { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string? ColorDecoracion { get; set; }
    }
}
