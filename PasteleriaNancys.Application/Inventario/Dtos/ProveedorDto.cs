namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class ProveedorDto
    {
        public Guid Id { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public string? Contacto { get; set; }
        public string? Telefono { get; set; }
        public bool Activo { get; set; }
    }
}
