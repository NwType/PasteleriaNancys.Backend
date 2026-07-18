using Microsoft.Extensions.DependencyInjection;
using PasteleriaNancys.Application.Pedidos.Interfaces;
using PasteleriaNancys.Infrastructure.Pedidos.Repositories;

namespace PasteleriaNancys.Infrastructure.Pedidos
{
    public static class PedidosInfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddPedidosInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IPedidoRepository, PedidoRepository>();
            services.AddScoped<IPagoQrRepository, PagoQrRepository>();
            services.AddScoped<ITablaPrecioPorcionesRepository, TablaPrecioPorcionesRepository>();
            services.AddScoped<IPortafolioImagenRepository, PortafolioImagenRepository>();

            return services;
        }
    }
}
