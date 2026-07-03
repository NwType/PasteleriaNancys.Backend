namespace PasteleriaNancys.Application.Seguridad.Dtos
{
    public class ActualizarUsuarioRequest
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public int IdRol { get; set; }
    }
}
