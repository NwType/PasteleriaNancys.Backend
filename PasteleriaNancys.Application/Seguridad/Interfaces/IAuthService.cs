using PasteleriaNancys.Application.Seguridad.Dtos;

namespace PasteleriaNancys.Application.Seguridad.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}
