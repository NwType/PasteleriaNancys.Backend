using Microsoft.Extensions.DependencyInjection;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Infrastructure.Inventario.Repositories;

namespace PasteleriaNancys.Infrastructure.Inventario
{
    public static class InventarioInfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInventarioInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IItemCatalogoRepository, ItemCatalogoRepository>();
            services.AddScoped<IProveedorRepository, ProveedorRepository>();
            services.AddScoped<ILoteRepository, LoteRepository>();
            services.AddScoped<IViajeRepository, ViajeRepository>();
            services.AddScoped<IStockMinimoRepository, StockMinimoRepository>();
            services.AddScoped<IRecetaRepository, RecetaRepository>();
            services.AddScoped<IEventoFestivoRepository, EventoFestivoRepository>();
            services.AddScoped<IHorneadaRepository, HorneadaRepository>();
            services.AddScoped<IConsumoInsumoRepository, ConsumoInsumoRepository>();

            return services;
        }
    }
}
