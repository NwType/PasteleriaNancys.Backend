using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PasteleriaNancys.Application.Seguridad;
using PasteleriaNancys.Application.Seguridad.Interfaces;
using PasteleriaNancys.Infrastructure.Seguridad.Repositories;
using PasteleriaNancys.Infrastructure.Seguridad.Security;

namespace PasteleriaNancys.Infrastructure.Seguridad
{
    public static class SeguridadInfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddSeguridadInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            services.Configure<SeguridadSettings>(configuration.GetSection("Seguridad"));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<SeguridadSettings>>().Value);

            services.AddScoped<IRolRepository, RolRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

            return services;
        }
    }
}
