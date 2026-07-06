using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StockMinimoController : ControllerBase
    {
        private readonly IStockMinimoService _stockMinimoService;

        public StockMinimoController(IStockMinimoService stockMinimoService)
        {
            _stockMinimoService = stockMinimoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<StockMinimoDto>>> ObtenerTodos()
        {
            return Ok(await _stockMinimoService.ObtenerTodosAsync());
        }

        [HttpPost]
        [Authorize(Roles = "Encargado de Almacen")]
        public async Task<ActionResult<StockMinimoDto>> Configurar(ConfigurarStockMinimoRequest request)
        {
            return Ok(await _stockMinimoService.ConfigurarAsync(request));
        }
    }
}
