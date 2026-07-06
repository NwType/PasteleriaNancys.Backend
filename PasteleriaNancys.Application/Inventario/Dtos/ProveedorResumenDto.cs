namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class ProveedorResumenDto
    {
        public Guid Id { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public string? Contacto { get; set; }
        public string? Telefono { get; set; }
    }
}
