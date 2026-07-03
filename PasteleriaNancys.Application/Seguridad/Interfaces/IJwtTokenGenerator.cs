using PasteleriaNancys.Domain.Seguridad;

namespace PasteleriaNancys.Application.Seguridad.Interfaces
{
    public interface IJwtTokenGenerator
    {
        (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario);
    }
}
