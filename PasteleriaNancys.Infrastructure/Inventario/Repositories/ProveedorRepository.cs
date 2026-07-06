using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Inventario.Repositories
{
    public class ProveedorRepository : IProveedorRepository
    {
        private readonly ApplicationDbContext _context;

        public ProveedorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Proveedor?> ObtenerPorIdAsync(Guid id) =>
            await _context.Proveedores.FirstOrDefaultAsync(p => p.Id == id);

        public async Task<Proveedor?> ObtenerPorNombreAsync(string nombreEmpresa) =>
            await _context.Proveedores.FirstOrDefaultAsync(p => p.NombreEmpresa == nombreEmpresa);

        public async Task<List<Proveedor>> ObtenerTodosAsync() =>
            await _context.Proveedores.AsNoTracking().ToListAsync();

        public async Task AgregarAsync(Proveedor proveedor) =>
            await _context.Proveedores.AddAsync(proveedor);

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();
    }
}
