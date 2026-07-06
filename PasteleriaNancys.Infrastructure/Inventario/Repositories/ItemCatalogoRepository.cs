using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Inventario.Repositories
{
    public class ItemCatalogoRepository : IItemCatalogoRepository
    {
        private readonly ApplicationDbContext _context;

        public ItemCatalogoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ItemCatalogo?> ObtenerPorIdAsync(Guid id) =>
            await _context.ItemsCatalogo.FirstOrDefaultAsync(i => i.Id == id);

        public async Task<ItemCatalogo?> ObtenerPorCodigoAsync(string codigoReferencia) =>
            await _context.ItemsCatalogo.FirstOrDefaultAsync(i => i.CodigoReferencia == codigoReferencia);

        public async Task<List<ItemCatalogo>> ObtenerTodosAsync() =>
            await _context.ItemsCatalogo.AsNoTracking().ToListAsync();

        public async Task AgregarAsync(ItemCatalogo item) =>
            await _context.ItemsCatalogo.AddAsync(item);

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();
    }
}
