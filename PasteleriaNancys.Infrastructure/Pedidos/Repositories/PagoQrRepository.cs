using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Pedidos.Interfaces;
using PasteleriaNancys.Domain.Pedidos;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Pedidos.Repositories
{
    public class PagoQrRepository : IPagoQrRepository
    {
        private readonly ApplicationDbContext _context;

        public PagoQrRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagoQr?> ObtenerPorIdAsync(Guid id) =>
            await _context.PagosQr.FirstOrDefaultAsync(p => p.Id == id);

        public async Task<PagoQr?> ObtenerMasRecientePorPedidoAsync(Guid idPedido) =>
            await _context.PagosQr
                .Where(p => p.IdPedido == idPedido)
                .OrderByDescending(p => p.FechaGeneracion)
                .FirstOrDefaultAsync();

        public async Task AgregarAsync(PagoQr pago) =>
            await _context.PagosQr.AddAsync(pago);

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();
    }
}
