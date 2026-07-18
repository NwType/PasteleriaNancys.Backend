using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Inventario.Repositories
{
    public class HorneadaRepository : IHorneadaRepository
    {
        private readonly ApplicationDbContext _context;

        public HorneadaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Horneada>> ObtenerTodosAsync() =>
            await _context.Horneadas
                .Include(h => h.Consumos)
                    .ThenInclude(c => c.Item)
                .ToListAsync();

        public async Task AgregarAsync(Horneada horneada) =>
            await _context.Horneadas.AddAsync(horneada);

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();
    }
}
