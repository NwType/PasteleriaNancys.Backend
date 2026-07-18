namespace PasteleriaNancys.Application.Pedidos.Dtos
{
    public class PortafolioImagenDto
    {
        public Guid Id { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string ImagenUrl { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
