using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Services
{
    public class EventoFestivoService : IEventoFestivoService
    {
        private readonly IEventoFestivoRepository _eventoFestivoRepository;

        public EventoFestivoService(IEventoFestivoRepository eventoFestivoRepository)
        {
            _eventoFestivoRepository = eventoFestivoRepository;
        }

        public async Task<EventoFestivoDto> CrearAsync(CrearEventoFestivoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NombreEvento))
            {
                throw new ReglaNegocioException("El nombre del evento es obligatorio.");
            }

            if (request.MultiplicadorDemanda <= 0)
            {
                throw new ReglaNegocioException("Ingrese un valor positivo para el multiplicador.");
            }

            var evento = new EventoFestivo
            {
                Id = Guid.NewGuid(),
                NombreEvento = request.NombreEvento.Trim(),
                FechaEvento = request.FechaEvento.Date,
                MultiplicadorDemanda = request.MultiplicadorDemanda,
                Activo = true,
                FechaRegistro = DateTime.UtcNow
            };

            await _eventoFestivoRepository.AgregarAsync(evento);
            await _eventoFestivoRepository.GuardarCambiosAsync();

            return MapearDto(evento);
        }

        public async Task<List<EventoFestivoDto>> ObtenerTodosAsync()
        {
            var eventos = await _eventoFestivoRepository.ObtenerTodosAsync();
            return eventos.Select(MapearDto).ToList();
        }

        private static EventoFestivoDto MapearDto(EventoFestivo evento) => new()
        {
            Id = evento.Id,
            NombreEvento = evento.NombreEvento,
            FechaEvento = evento.FechaEvento,
            MultiplicadorDemanda = evento.MultiplicadorDemanda,
            Activo = evento.Activo,
            FechaRegistro = evento.FechaRegistro
        };
    }
}
