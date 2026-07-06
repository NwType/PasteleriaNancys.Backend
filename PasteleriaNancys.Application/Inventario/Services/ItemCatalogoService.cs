using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Services
{
    public class ItemCatalogoService : IItemCatalogoService
    {
        private static readonly string[] TiposValidos = { "Terminado", "MateriaPrima" };

        private readonly IItemCatalogoRepository _itemCatalogoRepository;

        public ItemCatalogoService(IItemCatalogoRepository itemCatalogoRepository)
        {
            _itemCatalogoRepository = itemCatalogoRepository;
        }

        public async Task<ItemCatalogoDto> CrearAsync(CrearItemCatalogoRequest request)
        {
            var codigo = request.CodigoReferencia.Trim();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                throw new ReglaNegocioException("El código de referencia es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                throw new ReglaNegocioException("El nombre es obligatorio.");
            }

            if (!TiposValidos.Contains(request.Tipo))
            {
                throw new ReglaNegocioException("El tipo debe ser 'Terminado' o 'MateriaPrima'.");
            }

            if (await _itemCatalogoRepository.ObtenerPorCodigoAsync(codigo) is not null)
            {
                throw new ConflictoException($"El código ya está en uso. Ingrese uno diferente ('{codigo}').");
            }

            var item = new ItemCatalogo
            {
                Id = Guid.NewGuid(),
                CodigoReferencia = codigo,
                Nombre = request.Nombre.Trim(),
                Categoria = request.Categoria.Trim(),
                Tipo = request.Tipo,
                UnidadMedida = request.UnidadMedida.Trim(),
                Activo = true
            };

            await _itemCatalogoRepository.AgregarAsync(item);
            await _itemCatalogoRepository.GuardarCambiosAsync();

            return MapearDto(item);
        }

        public async Task<List<ItemCatalogoDto>> ObtenerTodosAsync()
        {
            var items = await _itemCatalogoRepository.ObtenerTodosAsync();
            return items.Select(MapearDto).ToList();
        }

        public async Task<ItemCatalogoDto> ObtenerPorIdAsync(Guid id)
        {
            var item = await _itemCatalogoRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró el ítem con id {id}.");

            return MapearDto(item);
        }

        public async Task<ItemCatalogoDto> ActualizarAsync(Guid id, ActualizarItemCatalogoRequest request)
        {
            var item = await _itemCatalogoRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró el ítem con id {id}.");

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                throw new ReglaNegocioException("El nombre es obligatorio.");
            }

            if (!TiposValidos.Contains(request.Tipo))
            {
                throw new ReglaNegocioException("El tipo debe ser 'Terminado' o 'MateriaPrima'.");
            }

            item.Nombre = request.Nombre.Trim();
            item.Categoria = request.Categoria.Trim();
            item.Tipo = request.Tipo;
            item.UnidadMedida = request.UnidadMedida.Trim();

            await _itemCatalogoRepository.GuardarCambiosAsync();

            return MapearDto(item);
        }

        public async Task DesactivarAsync(Guid id)
        {
            var item = await _itemCatalogoRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró el ítem con id {id}.");

            item.Activo = false;
            await _itemCatalogoRepository.GuardarCambiosAsync();
        }

        private static ItemCatalogoDto MapearDto(ItemCatalogo item) => new()
        {
            Id = item.Id,
            CodigoReferencia = item.CodigoReferencia,
            Nombre = item.Nombre,
            Categoria = item.Categoria,
            Tipo = item.Tipo,
            UnidadMedida = item.UnidadMedida,
            Activo = item.Activo
        };
    }
}
