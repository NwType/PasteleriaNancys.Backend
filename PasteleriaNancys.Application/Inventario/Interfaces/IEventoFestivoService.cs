using PasteleriaNancys.Application.Inventario.Dtos;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IEventoFestivoService
    {
        Task<EventoFestivoDto> CrearAsync(CrearEventoFestivoRequest request);
        Task<List<EventoFestivoDto>> ObtenerTodosAsync();
    }
}
