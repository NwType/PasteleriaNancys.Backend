using Microsoft.Extensions.DependencyInjection;
using PasteleriaNancys.Application.Pedidos.Interfaces;
using PasteleriaNancys.Application.Pedidos.Services;

namespace PasteleriaNancys.Application.Pedidos
{
    public static class PedidosApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddPedidosApplication(this IServiceCollection services)
        {
            services.AddScoped<IPedidoService, PedidoService>();
            services.AddScoped<IPagoQrService, PagoQrService>();
            services.AddScoped<IPortafolioImagenService, PortafolioImagenService>();

            return services;
        }
    }
}
