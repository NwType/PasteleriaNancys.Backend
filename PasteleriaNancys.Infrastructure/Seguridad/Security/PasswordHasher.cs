using PasteleriaNancys.Application.Seguridad.Interfaces;

namespace PasteleriaNancys.Infrastructure.Seguridad.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password);

        public bool Verificar(string password, string hash) =>
            BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
