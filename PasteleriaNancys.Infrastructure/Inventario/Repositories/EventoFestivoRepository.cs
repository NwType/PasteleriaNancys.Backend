using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;
using PasteleriaNancys.Infrastructure.Data;

namespace PasteleriaNancys.Infrastructure.Inventario.Repositories
{
    public class EventoFestivoRepository : IEventoFestivoRepository
    {
        private readonly ApplicationDbContext _context;

        public EventoFestivoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EventoFestivo?> ObtenerPorIdAsync(Guid id) =>
            await _context.EventosFestivos.FirstOrDefaultAsync(e => e.Id == id);

        public async Task<List<EventoFestivo>> ObtenerTodosAsync() =>
            await _context.EventosFestivos.AsNoTracking().ToListAsync();

        public async Task<EventoFestivo?> ObtenerProximoAsync(DateTime desde, DateTime hasta) =>
            await _context.EventosFestivos
                .Where(e => e.Activo && e.FechaEvento >= desde && e.FechaEvento <= hasta)
                .OrderBy(e => e.FechaEvento)
                .FirstOrDefaultAsync();

        public async Task AgregarAsync(EventoFestivo evento) =>
            await _context.EventosFestivos.AddAsync(evento);

        public async Task GuardarCambiosAsync() =>
            await _context.SaveChangesAsync();
    }
}
