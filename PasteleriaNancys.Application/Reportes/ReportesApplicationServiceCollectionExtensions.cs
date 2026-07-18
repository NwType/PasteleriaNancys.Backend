using Microsoft.Extensions.DependencyInjection;
using PasteleriaNancys.Application.Reportes.Interfaces;
using PasteleriaNancys.Application.Reportes.Services;

namespace PasteleriaNancys.Application.Reportes
{
    public static class ReportesApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddReportesApplication(this IServiceCollection services)
        {
            services.AddScoped<IReporteService, ReporteService>();

            return services;
        }
    }
}
