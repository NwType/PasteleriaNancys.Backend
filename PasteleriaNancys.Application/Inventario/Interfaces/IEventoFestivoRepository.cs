using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IEventoFestivoRepository
    {
        Task<EventoFestivo?> ObtenerPorIdAsync(Guid id);
        Task<List<EventoFestivo>> ObtenerTodosAsync();
        Task<EventoFestivo?> ObtenerProximoAsync(DateTime desde, DateTime hasta);
        Task AgregarAsync(EventoFestivo evento);
        Task GuardarCambiosAsync();
    }
}
