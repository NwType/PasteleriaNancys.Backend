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

            ValidarTipos(itemTerminado, itemInsumo);
            await ValidarSinCiclosAsync(itemTerminado, itemInsumo);

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

            var creadas = new List<RecetaItemDto>();

            foreach (var linea in request.Lineas)
            {
                if (linea.CantidadRequerida <= 0)
                {
                    throw new ReglaNegocioException("La cantidad requerida debe ser mayor a cero en todas las filas.");
                }

                var itemInsumo = await _itemCatalogoRepository.ObtenerPorIdAsync(linea.IdItemInsumo)
                    ?? throw new NoEncontradoException($"No se encontró el ítem con id {linea.IdItemInsumo}.");

                ValidarTipos(itemTerminado, itemInsumo);
                await ValidarSinCiclosAsync(itemTerminado, itemInsumo);

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

        // Recetas anidadas (requisito del tutor, 2026-07-17): un producto Terminado o un
        // Intermedio (bizcocho, preparado) puede tener receta, y sus componentes pueden ser
        // materia prima u otros Intermedios — ej. Torta de Chocolate = Bizcocho de Chocolate
        // (que a su vez tiene su receta de harina/azúcar/huevo) + crema + jalea.
        private static void ValidarTipos(ItemCatalogo itemTerminado, ItemCatalogo itemInsumo)
        {
            if (itemTerminado.Tipo == "MateriaPrima")
            {
                throw new ReglaNegocioException(
                    "La materia prima no tiene receta: solo productos terminados o intermedios (bizcochos, preparados).");
            }

            if (itemInsumo.Tipo == "Terminado")
            {
                throw new ReglaNegocioException(
                    $"'{itemInsumo.Nombre}' es un producto terminado — un componente de receta debe ser materia prima o un intermedio.");
            }
        }

        // Evita ciclos en el árbol de recetas (A usa B, B usa A) — con recetas anidadas un ciclo
        // colgaría el descuento automático y la proyección, que recorren el árbol recursivamente.
        private async Task ValidarSinCiclosAsync(ItemCatalogo itemTerminado, ItemCatalogo itemInsumo)
        {
            if (itemTerminado.Id == itemInsumo.Id)
            {
                throw new ReglaNegocioException("Un ítem no puede ser componente de su propia receta.");
            }

            if (itemInsumo.Tipo != "Intermedio")
            {
                return;
            }

            var visitados = new HashSet<Guid>();
            var pendientes = new Stack<Guid>();
            pendientes.Push(itemInsumo.Id);

            while (pendientes.Count > 0)
            {
                var actual = pendientes.Pop();
                if (!visitados.Add(actual))
                {
                    continue;
                }

                foreach (var linea in await _recetaRepository.ObtenerPorProductoTerminadoAsync(actual))
                {
                    if (linea.IdItemInsumo == itemTerminado.Id)
                    {
                        throw new ReglaNegocioException(
                            $"No se puede agregar '{itemInsumo.Nombre}': su receta ya usa (directa o indirectamente) a '{itemTerminado.Nombre}', se formaría un ciclo.");
                    }

                    pendientes.Push(linea.IdItemInsumo);
                }
            }
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
