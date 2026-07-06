using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Inventario.Repositories
{
    public class RecetaRepository : IRecetaRepository
    {
        private readonly ApplicationDbContext _context;

        public RecetaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RecetaItem?> ObtenerPorIdAsync(Guid id) =>
            await _context.RecetasItem.FirstOrDefaultAsync(r => r.Id == id);

        public async Task<RecetaItem?> ObtenerPorParAsync(Guid idItemTerminado, Guid idItemInsumo) =>
            await _context.RecetasItem.FirstOrDefaultAsync(r =>
                r.IdItemTerminado == idItemTerminado && r.IdItemInsumo == idItemInsumo);

        public async Task<List<RecetaItem>> ObtenerPorProductoTerminadoAsync(Guid idItemTerminado) =>
            await _context.RecetasItem
                .Include(r => r.ItemInsumo)
                .Where(r => r.IdItemTerminado == idItemTerminado)
                .AsNoTracking()
                .ToListAsync();

        public async Task<List<RecetaItem>> ObtenerPorInsumoAsync(Guid idItemInsumo) =>
            await _context.RecetasItem
                .Include(r => r.ItemTerminado)
                .Where(r => r.IdItemInsumo == idItemInsumo)
                .AsNoTracking()
                .ToListAsync();

        public async Task AgregarAsync(RecetaItem receta) =>
            await _context.RecetasItem.AddAsync(receta);

        public void Eliminar(RecetaItem receta) =>
            _context.RecetasItem.Remove(receta);

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();
    }
}
