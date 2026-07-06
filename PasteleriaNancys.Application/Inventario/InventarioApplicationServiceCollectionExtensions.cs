using Microsoft.Extensions.DependencyInjection;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Application.Inventario.Services;

namespace PasteleriaNancys.Application.Inventario
{
    public static class InventarioApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddInventarioApplication(this IServiceCollection services)
        {
            services.AddScoped<IItemCatalogoService, ItemCatalogoService>();
            services.AddScoped<IProveedorService, ProveedorService>();
            services.AddScoped<ILoteService, LoteService>();
            services.AddScoped<IDespachoService, DespachoService>();
            services.AddScoped<IStockMinimoService, StockMinimoService>();
            services.AddScoped<IRecetaService, RecetaService>();
            services.AddScoped<IEventoFestivoService, EventoFestivoService>();
            services.AddScoped<IAlertaService, AlertaService>();

            return services;
        }
    }
}
