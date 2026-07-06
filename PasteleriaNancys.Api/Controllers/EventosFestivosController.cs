using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventosFestivosController : ControllerBase
    {
        private readonly IEventoFestivoService _eventoFestivoService;

        public EventosFestivosController(IEventoFestivoService eventoFestivoService)
        {
            _eventoFestivoService = eventoFestivoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<EventoFestivoDto>>> ObtenerTodos()
        {
            return Ok(await _eventoFestivoService.ObtenerTodosAsync());
        }

        [HttpPost]
        [Authorize(Roles = "Encargado de Almacen")]
        public async Task<ActionResult<EventoFestivoDto>> Crear(CrearEventoFestivoRequest request)
        {
            return Ok(await _eventoFestivoService.CrearAsync(request));
        }
    }
}
