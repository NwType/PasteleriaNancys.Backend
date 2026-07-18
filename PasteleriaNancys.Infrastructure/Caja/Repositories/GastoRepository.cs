using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Caja.Interfaces;
using PasteleriaNancys.Domain.Caja;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Caja.Repositories
{
    public class GastoRepository : IGastoRepository
    {
        private readonly ApplicationDbContext _context;

        public GastoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<GastoExtra>> ObtenerPorTurnoAsync(Guid idTurno) =>
            await _context.GastosExtra
                .Where(g => g.IdTurno == idTurno)
                .OrderByDescending(g => g.FechaHora)
                .ToListAsync();

        public async Task AgregarAsync(GastoExtra gasto) =>
            await _context.GastosExtra.AddAsync(gasto);

        public async Task GuardarCambiosAsync() =>
            await _context.GuardarConControlDeConcurrenciaAsync(
                "El turno fue modificado por otro gasto al mismo tiempo. Vuelva a intentarlo.");
    }
}
