using Microsoft.Extensions.DependencyInjection;
using PasteleriaNancys.Application.Caja.Interfaces;
using PasteleriaNancys.Infrastructure.Caja.Repositories;

namespace PasteleriaNancys.Infrastructure.Caja
{
    public static class CajaInfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddCajaInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ITurnoRepository, TurnoRepository>();
            services.AddScoped<IVentaRepository, VentaRepository>();
            services.AddScoped<IGastoRepository, GastoRepository>();

            return services;
        }
    }
}
