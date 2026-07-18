using Microsoft.Extensions.DependencyInjection;
using PasteleriaNancys.Application.Administracion.Interfaces;
using PasteleriaNancys.Application.Administracion.Services;

namespace PasteleriaNancys.Application.Administracion
{
    public static class AdministracionApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddAdministracionApplication(this IServiceCollection services)
        {
            services.AddScoped<IAdministracionService, AdministracionService>();

            return services;
        }
    }
}
