using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Inventario.Repositories
{
    public class ConsumoInsumoRepository : IConsumoInsumoRepository
    {
        private readonly ApplicationDbContext _context;

        public ConsumoInsumoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ConsumoInsumo>> ObtenerTodosAsync() =>
            await _context.ConsumosInsumo
                .Include(c => c.Item)
                .ToListAsync();

        public async Task AgregarAsync(ConsumoInsumo consumo) =>
            await _context.ConsumosInsumo.AddAsync(consumo);

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();
    }
}
