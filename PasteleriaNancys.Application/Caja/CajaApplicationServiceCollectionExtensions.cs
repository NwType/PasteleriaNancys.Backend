using Microsoft.Extensions.DependencyInjection;
using PasteleriaNancys.Application.Caja.Interfaces;
using PasteleriaNancys.Application.Caja.Services;

namespace PasteleriaNancys.Application.Caja
{
    public static class CajaApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddCajaApplication(this IServiceCollection services)
        {
            services.AddScoped<ITurnoService, TurnoService>();
            services.AddScoped<IVentaService, VentaService>();
            services.AddScoped<IGastoService, GastoService>();

            return services;
        }
    }
}
