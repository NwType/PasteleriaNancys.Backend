namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class CrearProveedorRequest
    {
        public string NombreEmpresa { get; set; } = string.Empty;
        public string? Contacto { get; set; }
        public string? Telefono { get; set; }
    }
}
