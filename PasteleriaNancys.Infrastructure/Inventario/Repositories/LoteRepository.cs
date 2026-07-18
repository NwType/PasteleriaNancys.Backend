using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Caja;
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

        // Sin AsNoTracking a propósito: LoteService.ObtenerTodosAsync sincroniza y persiste el
        // estado vigente (Crítico/Baja por vencimiento) sobre estas mismas entidades.
        public async Task<List<LotePeps>> ObtenerTodosAsync() =>
            await _context.LotesPeps.ToListAsync();

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

        public async Task<decimal> ObtenerStockDisponiblePorUbicacionAsync(Guid idItem, string ubicacion) =>
            await _context.LotesPeps
                .Where(l => l.IdItem == idItem && l.Ubicacion == ubicacion && l.Estado != "Baja")
                .SumAsync(l => (decimal?)l.CantidadDisponible) ?? 0m;

        public async Task<List<Guid>> ObtenerProveedoresPorItemAsync(Guid idItem) =>
            await _context.LotesPeps
                .Where(l => l.IdItem == idItem && l.IdProveedor != null)
                .GroupBy(l => l.IdProveedor!.Value)
                .OrderByDescending(g => g.Max(l => l.FechaElaboracion))
                .Select(g => g.Key)
                .ToListAsync();

        // Bloqueo duro contra vencidos, independiente de si ya se sincronizó el Estado a "Baja"
        // (LoteService.SincronizarEstadosVencidosAsync corre en otras pantallas, no siempre antes
        // de una venta) — PEPS nunca debe poder vender ni consumir un lote ya caducado.
        public async Task<List<LotePeps>> ObtenerDisponiblesParaVentaAsync(Guid idItem, string ubicacion)
        {
            var hoy = DateTime.UtcNow.Date;
            return await _context.LotesPeps
                .Where(l => l.IdItem == idItem
                    && l.Ubicacion == ubicacion
                    && l.Estado != "Baja"
                    && l.CantidadDisponible > 0
                    && l.FechaCaducidad >= hoy)
                .OrderBy(l => l.FechaElaboracion)
                .ToListAsync();
        }

        public async Task<List<LotePeps>> ObtenerParaReponerAsync(Guid idItem) =>
            await _context.LotesPeps
                .Where(l => l.IdItem == idItem
                    && l.Estado != "Baja"
                    && l.CantidadDisponible < l.CantidadInicial)
                .OrderByDescending(l => l.FechaElaboracion)
                .ToListAsync();

        public async Task RegistrarConsumoAsync(List<VentaDetalleLote> consumos) =>
            await _context.VentasDetalleLote.AddRangeAsync(consumos);

        public async Task<List<VentaDetalleLote>> ObtenerConsumosPorVentaDetalleAsync(Guid idVentaDetalle) =>
            await _context.VentasDetalleLote
                .Where(c => c.IdVentaDetalle == idVentaDetalle)
                .ToListAsync();
    }
}
