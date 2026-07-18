using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Inventario.Repositories
{
    public class ViajeRepository : IViajeRepository
    {
        private readonly ApplicationDbContext _context;

        public ViajeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ViajeDespacho?> ObtenerPorIdAsync(Guid id) =>
            await _context.ViajesDespacho
                .Include(v => v.Detalles).ThenInclude(d => d.Lote).ThenInclude(l => l.Item)
                .FirstOrDefaultAsync(v => v.Id == id);

        public async Task<List<ViajeDespacho>> ObtenerTodosAsync() =>
            await _context.ViajesDespacho
                .Include(v => v.Detalles).ThenInclude(d => d.Lote).ThenInclude(l => l.Item)
                .AsNoTracking().ToListAsync();

        public async Task AgregarAsync(ViajeDespacho viaje) =>
            await _context.ViajesDespacho.AddAsync(viaje);

        public async Task AgregarDetalleAsync(ViajeDetalle detalle) =>
            await _context.ViajesDetalle.AddAsync(detalle);

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();
    }
}
