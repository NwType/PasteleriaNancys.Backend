using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Services
{
    public class StockMinimoService : IStockMinimoService
    {
        private readonly IStockMinimoRepository _stockMinimoRepository;
        private readonly IItemCatalogoRepository _itemCatalogoRepository;

        public StockMinimoService(
            IStockMinimoRepository stockMinimoRepository,
            IItemCatalogoRepository itemCatalogoRepository)
        {
            _stockMinimoRepository = stockMinimoRepository;
            _itemCatalogoRepository = itemCatalogoRepository;
        }

        public async Task<StockMinimoDto> ConfigurarAsync(ConfigurarStockMinimoRequest request)
        {
            if (request.CantidadMinima <= 0)
            {
                throw new ReglaNegocioException("Ingrese un valor mayor a cero.");
            }

            _ = await _itemCatalogoRepository.ObtenerPorIdAsync(request.IdItem)
                ?? throw new NoEncontradoException($"No se encontró el ítem con id {request.IdItem}.");

            var stockMinimo = await _stockMinimoRepository.ObtenerPorItemAsync(request.IdItem);

            if (stockMinimo is null)
            {
                stockMinimo = new StockMinimo
                {
                    Id = Guid.NewGuid(),
                    IdItem = request.IdItem,
                    CantidadMinima = request.CantidadMinima,
                    Activo = true
                };
                await _stockMinimoRepository.AgregarAsync(stockMinimo);
            }
            else
            {
                stockMinimo.CantidadMinima = request.CantidadMinima;
            }

            await _stockMinimoRepository.GuardarCambiosAsync();

            return MapearDto(stockMinimo);
        }

        public async Task<List<StockMinimoDto>> ObtenerTodosAsync()
        {
            var registros = await _stockMinimoRepository.ObtenerTodosAsync();
            return registros.Select(MapearDto).ToList();
        }

        private static StockMinimoDto MapearDto(StockMinimo stockMinimo) => new()
        {
            Id = stockMinimo.Id,
            IdItem = stockMinimo.IdItem,
            CantidadMinima = stockMinimo.CantidadMinima,
            Activo = stockMinimo.Activo
        };
    }
}
