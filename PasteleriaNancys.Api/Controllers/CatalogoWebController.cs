using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Application.Pedidos.Dtos;
using PasteleriaNancys.Application.Pedidos.Interfaces;

namespace PasteleriaNancys.Api.Controllers
{
    [ApiController]
    [Route("api/catalogo-web")]
    [AllowAnonymous]
    public class CatalogoWebController : ControllerBase
    {
        private readonly IItemCatalogoService _itemCatalogoService;
        private readonly IPedidoService _pedidoService;

        public CatalogoWebController(IItemCatalogoService itemCatalogoService, IPedidoService pedidoService)
        {
            _itemCatalogoService = itemCatalogoService;
            _pedidoService = pedidoService;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ItemCatalogoWebDto>> ObtenerProductoPublico(Guid id)
        {
            var (item, stock) = await _itemCatalogoService.ObtenerCatalogoPublicoConStockPorIdAsync(id);

            return Ok(new ItemCatalogoWebDto
            {
                Id = item.Id,
                Nombre = item.Nombre,
                Categoria = item.Categoria,
                UnidadMedida = item.UnidadMedida,
                PrecioUnitario = item.PrecioUnitario,
                NumeroPorciones = item.NumeroPorciones,
                EsPersonalizable = item.EsPersonalizable,
                ImagenUrl = item.ImagenUrl,
                Descripcion = item.Descripcion,
                ColorDecoracion = item.ColorDecoracion,
                TipoMasa = item.TipoMasa,
                NombreInsumoRelleno = item.NombreInsumoRelleno,
                NombreInsumoCrema = item.NombreInsumoCrema,
                StockDisponible = stock
            });
        }

        [HttpGet("{idItemTerminado:guid}/tabla-precio-porciones")]
        public async Task<ActionResult<List<TablaPrecioPorcionesDto>>> ObtenerTablaPrecioPorciones(Guid idItemTerminado)
        {
            return Ok(await _pedidoService.ObtenerTablaPrecioPorcionesAsync(idItemTerminado));
        }

        [HttpGet]
        public async Task<ActionResult<List<ItemCatalogoWebDto>>> ObtenerCatalogoPublico()
        {
            var items = await _itemCatalogoService.ObtenerCatalogoPublicoConStockAsync();

            var catalogoPublico = items
                .Select(x => new ItemCatalogoWebDto
                {
                    Id = x.Item.Id,
                    Nombre = x.Item.Nombre,
                    Categoria = x.Item.Categoria,
                    UnidadMedida = x.Item.UnidadMedida,
                    PrecioUnitario = x.Item.PrecioUnitario,
                    NumeroPorciones = x.Item.NumeroPorciones,
                    EsPersonalizable = x.Item.EsPersonalizable,
                    ImagenUrl = x.Item.ImagenUrl,
                    Descripcion = x.Item.Descripcion,
                    ColorDecoracion = x.Item.ColorDecoracion,
                    TipoMasa = x.Item.TipoMasa,
                    NombreInsumoRelleno = x.Item.NombreInsumoRelleno,
                    NombreInsumoCrema = x.Item.NombreInsumoCrema,
                    StockDisponible = x.StockDisponible
                })
                .ToList();

            return Ok(catalogoPublico);
        }

        [HttpGet("insumos-personalizacion")]
        public async Task<ActionResult<List<ItemCatalogoWebDto>>> ObtenerInsumosPersonalizacion()
        {
            var items = await _itemCatalogoService.ObtenerInsumosPersonalizacionAsync();

            var insumos = items
                .Select(i => new ItemCatalogoWebDto
                {
                    Id = i.Id,
                    Nombre = i.Nombre,
                    Categoria = i.Categoria,
                    UnidadMedida = i.UnidadMedida,
                    PrecioUnitario = i.PrecioUnitario,
                    CategoriaPersonalizacion = i.CategoriaPersonalizacion,
                    TipoCremaAsociado = i.TipoCremaAsociado
                })
                .ToList();

            return Ok(insumos);
        }
    }
}
