using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Seguridad.Interfaces;
using PasteleriaNancys.Domain.Seguridad;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Seguridad.Repositories
{
    public class RolRepository : IRolRepository
    {
        private readonly ApplicationDbContext _context;

        public RolRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Rol?> ObtenerPorIdAsync(int idRol) =>
            await _context.Roles.FirstOrDefaultAsync(r => r.IdRol == idRol);

        public async Task<List<Rol>> ObtenerTodosAsync() =>
            await _context.Roles.AsNoTracking().ToListAsync();

        public async Task AgregarAsync(Rol rol) =>
            await _context.Roles.AddAsync(rol);

        public void Eliminar(Rol rol) =>
            _context.Roles.Remove(rol);

        public async Task<bool> TieneUsuariosAsync(int idRol) =>
            await _context.Usuarios.AnyAsync(u => u.IdRol == idRol);

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();
    }
}
