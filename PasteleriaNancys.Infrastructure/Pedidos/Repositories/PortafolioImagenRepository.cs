using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Pedidos.Interfaces;
using PasteleriaNancys.Domain.Pedidos;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Pedidos.Repositories
{
    public class PortafolioImagenRepository : IPortafolioImagenRepository
    {
        private readonly ApplicationDbContext _context;

        public PortafolioImagenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PortafolioImagen?> ObtenerPorIdAsync(Guid id) =>
            await _context.PortafolioImagenes.FirstOrDefaultAsync(p => p.Id == id);

        public async Task<List<PortafolioImagen>> ObtenerPorCategoriaAsync(string categoria) =>
            await _context.PortafolioImagenes
                .Where(p => p.Categoria == categoria && p.Activo)
                .OrderBy(p => p.Orden).ThenBy(p => p.FechaCreacion)
                .ToListAsync();

        public async Task<List<PortafolioImagen>> ObtenerTodasAsync() =>
            await _context.PortafolioImagenes
                .OrderBy(p => p.Categoria).ThenBy(p => p.Orden).ThenBy(p => p.FechaCreacion)
                .ToListAsync();

        public async Task AgregarAsync(PortafolioImagen imagen) =>
            await _context.PortafolioImagenes.AddAsync(imagen);

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();
    }
}
