using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Services
{
    public class RecetaService : IRecetaService
    {
        private readonly IRecetaRepository _recetaRepository;
        private readonly IItemCatalogoRepository _itemCatalogoRepository;

        public RecetaService(IRecetaRepository recetaRepository, IItemCatalogoRepository itemCatalogoRepository)
        {
            _recetaRepository = recetaRepository;
            _itemCatalogoRepository = itemCatalogoRepository;
        }

        public async Task<RecetaItemDto> CrearAsync(CrearRecetaItemRequest request)
        {
            if (request.CantidadRequerida <= 0)
            {
                throw new ReglaNegocioException("La cantidad requerida debe ser mayor a cero.");
            }

            var itemTerminado = await _itemCatalogoRepository.ObtenerPorIdAsync(request.IdItemTerminado)
                ?? throw new NoEncontradoException($"No se encontró el ítem con id {request.IdItemTerminado}.");

            var itemInsumo = await _itemCatalogoRepository.ObtenerPorIdAsync(request.IdItemInsumo)
                ?? throw new NoEncontradoException($"No se encontró el ítem con id {request.IdItemInsumo}.");

            if (itemTerminado.Tipo != "Terminado")
            {
                throw new ReglaNegocioException("El ítem terminado debe ser de tipo 'Terminado'.");
            }

            if (itemInsumo.Tipo != "MateriaPrima")
            {
                throw new ReglaNegocioException("El insumo debe ser de tipo 'MateriaPrima'.");
            }

            if (await _recetaRepository.ObtenerPorParAsync(request.IdItemTerminado, request.IdItemInsumo) is not null)
            {
                throw new ConflictoException("Ya existe una receta con ese producto e insumo.");
            }

            var receta = new RecetaItem
            {
                Id = Guid.NewGuid(),
                IdItemTerminado = request.IdItemTerminado,
                IdItemInsumo = request.IdItemInsumo,
                CantidadRequerida = request.CantidadRequerida
            };

            await _recetaRepository.AgregarAsync(receta);
            await _recetaRepository.GuardarCambiosAsync();

            return MapearDto(receta);
        }

        public async Task<List<RecetaItemDto>> CrearVariasAsync(CrearRecetaMultipleRequest request)
        {
            var itemTerminado = await _itemCatalogoRepository.ObtenerPorIdAsync(request.IdItemTerminado)
                ?? throw new NoEncontradoException($"No se encontró el ítem con id {request.IdItemTerminado}.");

            if (itemTerminado.Tipo != "Terminado")
            {
                throw new ReglaNegocioException("El ítem terminado debe ser de tipo 'Terminado'.");
            }

            var creadas = new List<RecetaItemDto>();

            foreach (var linea in request.Lineas)
            {
                if (linea.CantidadRequerida <= 0)
                {
                    throw new ReglaNegocioException("La cantidad requerida debe ser mayor a cero en todas las filas.");
                }

                var itemInsumo = await _itemCatalogoRepository.ObtenerPorIdAsync(linea.IdItemInsumo)
                    ?? throw new NoEncontradoException($"No se encontró el ítem con id {linea.IdItemInsumo}.");

                if (itemInsumo.Tipo != "MateriaPrima")
                {
                    throw new ReglaNegocioException($"'{itemInsumo.Nombre}' debe ser un insumo de tipo 'MateriaPrima'.");
                }

                if (await _recetaRepository.ObtenerPorParAsync(request.IdItemTerminado, linea.IdItemInsumo) is not null)
                {
                    // Ya existe esa combinación producto+insumo — se omite en vez de fallar todo el lote.
                    continue;
                }

                var receta = new RecetaItem
                {
                    Id = Guid.NewGuid(),
                    IdItemTerminado = request.IdItemTerminado,
                    IdItemInsumo = linea.IdItemInsumo,
                    CantidadRequerida = linea.CantidadRequerida
                };

                await _recetaRepository.AgregarAsync(receta);
                creadas.Add(MapearDto(receta));
            }

            await _recetaRepository.GuardarCambiosAsync();

            return creadas;
        }

        public async Task<List<RecetaItemDto>> ObtenerPorProductoTerminadoAsync(Guid idItemTerminado)
        {
            var recetas = await _recetaRepository.ObtenerPorProductoTerminadoAsync(idItemTerminado);
            return recetas.Select(MapearDto).ToList();
        }

        public async Task EliminarAsync(Guid id)
        {
            var receta = await _recetaRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró la receta con id {id}.");

            _recetaRepository.Eliminar(receta);
            await _recetaRepository.GuardarCambiosAsync();
        }

        private static RecetaItemDto MapearDto(RecetaItem receta) => new()
        {
            Id = receta.Id,
            IdItemTerminado = receta.IdItemTerminado,
            IdItemInsumo = receta.IdItemInsumo,
            CantidadRequerida = receta.CantidadRequerida
        };
    }
}
