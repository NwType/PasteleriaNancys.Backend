using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Inventario.Repositories
{
    public class MermaRepository : IMermaRepository
    {
        private readonly ApplicationDbContext _context;

        public MermaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Merma>> ObtenerTodasAsync() =>
            await _context.Mermas
                .Include(m => m.Item)
                .AsNoTracking()
                .ToListAsync();

        public async Task AgregarAsync(Merma merma) =>
            await _context.Mermas.AddAsync(merma);

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();
    }
}
