using Microsoft.Extensions.DependencyInjection;
using PasteleriaNancys.Application.Seguridad.Interfaces;
using PasteleriaNancys.Application.Seguridad.Services;

namespace PasteleriaNancys.Application.Seguridad
{
    public static class SeguridadApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddSeguridadApplication(this IServiceCollection services)
        {
            services.AddScoped<IRolService, RolService>();
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
