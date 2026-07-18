using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Inventario.Repositories
{
    public class StockMinimoRepository : IStockMinimoRepository
    {
        private readonly ApplicationDbContext _context;

        public StockMinimoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StockMinimo?> ObtenerPorItemAsync(Guid idItem) =>
            await _context.StockMinimos.FirstOrDefaultAsync(s => s.IdItem == idItem);

        public async Task<StockMinimo?> ObtenerPorIdAsync(Guid id) =>
            await _context.StockMinimos.FirstOrDefaultAsync(s => s.Id == id);

        public async Task<List<StockMinimo>> ObtenerTodosAsync() =>
            await _context.StockMinimos.AsNoTracking().ToListAsync();

        public async Task AgregarAsync(StockMinimo stockMinimo) =>
            await _context.StockMinimos.AddAsync(stockMinimo);

        public void Eliminar(StockMinimo stockMinimo) =>
            _context.StockMinimos.Remove(stockMinimo);

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();
    }
}
