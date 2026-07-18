using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Caja.Interfaces;
using PasteleriaNancys.Domain.Caja;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Caja.Repositories
{
    public class VentaRepository : IVentaRepository
    {
        private readonly ApplicationDbContext _context;

        public VentaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<VentaPos?> ObtenerPorIdAsync(Guid id) =>
            await _context.VentasPos
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == id);

        public async Task<List<VentaPos>> ObtenerPorTurnoAsync(Guid idTurno) =>
            await _context.VentasPos
                .Include(v => v.Detalles)
                .Where(v => v.IdTurno == idTurno)
                .OrderByDescending(v => v.FechaHora)
                .ToListAsync();

        public async Task<decimal> ObtenerCantidadVendidaAsync(Guid idItem, DateTime desde, DateTime hasta) =>
            await _context.VentasDetalle
                .Where(d => d.IdItem == idItem
                    && !d.Venta.Anulada
                    && d.Venta.FechaHora >= desde
                    && d.Venta.FechaHora <= hasta)
                .SumAsync(d => (decimal?)d.Cantidad) ?? 0m;

        public async Task<List<VentaPos>> ObtenerPorRangoAsync(DateTime desde, DateTime hasta) =>
            await _context.VentasPos
                .Include(v => v.Detalles)
                .Include(v => v.Turno)
                .Where(v => !v.Anulada && v.FechaHora >= desde && v.FechaHora <= hasta)
                .OrderByDescending(v => v.FechaHora)
                .ToListAsync();

        public async Task AgregarAsync(VentaPos venta) =>
            await _context.VentasPos.AddAsync(venta);

        public async Task GuardarCambiosAsync() =>
            await _context.GuardarConControlDeConcurrenciaAsync(
                "El turno fue modificado por otra venta al mismo tiempo. Vuelva a intentarlo.");
    }
}
