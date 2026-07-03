namespace PasteleriaNancys.Application.Seguridad.Dtos
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiraEn { get; set; }
        public UsuarioDto Usuario { get; set; } = null!;
    }
}
