using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Pedidos.Interfaces;
using PasteleriaNancys.Domain.Pedidos;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Pedidos.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly ApplicationDbContext _context;

        public PedidoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PedidoCliente?> ObtenerPorIdAsync(Guid id) =>
            await _context.PedidosCliente
                .Include(p => p.Configuracion)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<PedidoCliente?> ObtenerPorWhatsAppYCodigoAsync(string whatsApp, string codigoReferencia) =>
            await _context.PedidosCliente
                .Include(p => p.Configuracion)
                .FirstOrDefaultAsync(p => p.WhatsApp == whatsApp && p.CodigoQrReferencia == codigoReferencia);

        public async Task<List<PedidoCliente>> ObtenerPendientesAsync() =>
            await _context.PedidosCliente
                .Include(p => p.Configuracion)
                .Where(p => p.Estado != "Entregado" && p.Estado != "Cancelado")
                .OrderBy(p => p.FechaEntregaSolicitada)
                .ToListAsync();

        public async Task<List<PedidoCliente>> ObtenerTodosAsync(string? estado, DateTime? fechaEntrega)
        {
            var query = _context.PedidosCliente
                .Include(p => p.Configuracion)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(p => p.Estado == estado);
            }

            if (fechaEntrega.HasValue)
            {
                query = query.Where(p => p.FechaEntregaSolicitada.Date == fechaEntrega.Value.Date);
            }

            return await query.OrderByDescending(p => p.FechaSolicitud).ToListAsync();
        }

        public async Task AgregarAsync(PedidoCliente pedido) =>
            await _context.PedidosCliente.AddAsync(pedido);

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();
    }
}
