using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Pedidos.Interfaces;
using PasteleriaNancys.Domain.Pedidos;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Pedidos.Repositories
{
    public class TablaPrecioPorcionesRepository : ITablaPrecioPorcionesRepository
    {
        private readonly ApplicationDbContext _context;

        public TablaPrecioPorcionesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TablaPrecioPorciones?> ObtenerPorProductoYPorcionesAsync(Guid idItemTerminado, int numeroPorciones) =>
            await _context.TablaPrecioPorciones
                .FirstOrDefaultAsync(t => t.IdItemTerminado == idItemTerminado && t.NumeroPorciones == numeroPorciones);

        public async Task<List<TablaPrecioPorciones>> ObtenerActivosPorProductoAsync(Guid idItemTerminado) =>
            await _context.TablaPrecioPorciones
                .Where(t => t.IdItemTerminado == idItemTerminado && t.Activo)
                .OrderBy(t => t.NumeroPorciones)
                .ToListAsync();
    }
}
