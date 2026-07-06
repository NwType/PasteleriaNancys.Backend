using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Inventario.Repositories
{
    public class LoteRepository : ILoteRepository
    {
        private readonly ApplicationDbContext _context;

        public LoteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LotePeps?> ObtenerPorIdAsync(Guid id) =>
            await _context.LotesPeps.FirstOrDefaultAsync(l => l.Id == id);

        public async Task<List<LotePeps>> ObtenerTodosAsync() =>
            await _context.LotesPeps.AsNoTracking().ToListAsync();

        public async Task AgregarAsync(LotePeps lote) =>
            await _context.LotesPeps.AddAsync(lote);

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();

        public async Task<List<LotePepsOrdenado>> ObtenerProximosACaducarAsync(int diasLimite) =>
            await _context.LotesPepsOrdenados
                .Where(l => l.DiasParaCaducar != null && l.DiasParaCaducar <= diasLimite)
                .AsNoTracking()
                .ToListAsync();

        public async Task<decimal> ObtenerStockDisponibleTotalAsync(Guid idItem) =>
            await _context.LotesPeps
                .Where(l => l.IdItem == idItem && l.Estado != "Baja")
                .SumAsync(l => (decimal?)l.CantidadDisponible) ?? 0m;

        public async Task<List<Guid>> ObtenerProveedoresPorItemAsync(Guid idItem) =>
            await _context.LotesPeps
                .Where(l => l.IdItem == idItem && l.IdProveedor != null)
                .GroupBy(l => l.IdProveedor!.Value)
                .OrderByDescending(g => g.Max(l => l.FechaElaboracion))
                .Select(g => g.Key)
                .ToListAsync();

        public async Task<List<LotePeps>> ObtenerDisponiblesParaVentaAsync(Guid idItem, string ubicacion) =>
            await _context.LotesPeps
                .Where(l => l.IdItem == idItem
                    && l.Ubicacion == ubicacion
                    && l.Estado != "Baja"
                    && l.CantidadDisponible > 0)
                .OrderBy(l => l.FechaElaboracion)
                .ToListAsync();

        public async Task<List<LotePeps>> ObtenerParaReponerAsync(Guid idItem) =>
            await _context.LotesPeps
                .Where(l => l.IdItem == idItem
                    && l.Estado != "Baja"
                    && l.CantidadDisponible < l.CantidadInicial)
                .OrderByDescending(l => l.FechaElaboracion)
                .ToListAsync();
    }
}
