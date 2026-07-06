using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Caja.Interfaces;
using PasteleriaNancys.Domain.Caja;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Caja.Repositories
{
    public class TurnoRepository : ITurnoRepository
    {
        private readonly ApplicationDbContext _context;

        public TurnoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Turno?> ObtenerPorIdAsync(Guid id) =>
            await _context.Turnos.FirstOrDefaultAsync(t => t.Id == id);

        public async Task<Turno?> ObtenerAbiertoPorUsuarioAsync(Guid idUsuario) =>
            await _context.Turnos.FirstOrDefaultAsync(t =>
                t.IdUsuarioResponsable == idUsuario && t.Estado == "Abierto");

        public async Task AgregarAsync(Turno turno) =>
            await _context.Turnos.AddAsync(turno);

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();
    }
}
