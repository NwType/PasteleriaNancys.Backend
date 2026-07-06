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

        public async Task AgregarAsync(VentaPos venta) =>
            await _context.VentasPos.AddAsync(venta);

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();
    }
}
