namespace PasteleriaNancys.Application.Seguridad.Dtos
{
    public class LoginRequest
    {
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
