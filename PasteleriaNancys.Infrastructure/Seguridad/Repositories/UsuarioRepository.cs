using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Seguridad.Interfaces;
using PasteleriaNancys.Domain.Seguridad;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Seguridad.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ApplicationDbContext _context;

        public UsuarioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> ObtenerPorIdAsync(Guid id) =>
            await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);

        public async Task<Usuario?> ObtenerPorCorreoAsync(string correo) =>
            await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Correo == correo);

        public async Task<List<Usuario>> ObtenerTodosAsync() =>
            await _context.Usuarios.Include(u => u.Rol).AsNoTracking().ToListAsync();

        public async Task<bool> ExisteAlgunoAsync() =>
            await _context.Usuarios.AnyAsync();

        public async Task AgregarAsync(Usuario usuario) =>
            await _context.Usuarios.AddAsync(usuario);

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();
    }
}
