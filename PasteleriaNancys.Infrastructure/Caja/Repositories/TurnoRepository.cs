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

        public async Task<List<Turno>> ObtenerAbiertosAsync() =>
            await _context.Turnos
                .Where(t => t.Estado == "Abierto")
                .OrderBy(t => t.FechaApertura)
                .ToListAsync();

        public async Task<List<Turno>> ObtenerCerradosAsync(DateTime? desde, DateTime? hasta, Guid? idUsuarioResponsable)
        {
            var query = _context.Turnos.Where(t => t.Estado == "Cerrado");

            if (desde.HasValue)
            {
                query = query.Where(t => t.FechaCierre >= desde.Value);
            }

            if (hasta.HasValue)
            {
                query = query.Where(t => t.FechaCierre <= hasta.Value);
            }

            if (idUsuarioResponsable.HasValue)
            {
                query = query.Where(t => t.IdUsuarioResponsable == idUsuarioResponsable.Value);
            }

            return await query.OrderByDescending(t => t.FechaCierre).ToListAsync();
        }

        public async Task AgregarAsync(Turno turno) =>
            await _context.Turnos.AddAsync(turno);

        public async Task GuardarCambiosAsync() =>
            await _context.GuardarConControlDeConcurrenciaAsync(
                "Ya existe un turno abierto para este usuario, o el turno fue modificado por otro proceso al mismo tiempo. Vuelva a intentarlo.");
    }
}
