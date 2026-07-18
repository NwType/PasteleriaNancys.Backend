using System.Text.RegularExpressions;
using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Services
{
    public class ItemCatalogoService : IItemCatalogoService
    {
        private const int LongitudMaximaNombre = 150;
        private const int LongitudMaximaCategoria = 80;
        private const int LongitudMaximaDescripcion = 2000;

        // El catálogo público (vitrina/POS) debe mostrar solo lo que físicamente está en el
        // punto de venta y se puede vender ya — no el total incluyendo lo que sigue en
        // producción (San Mateo) esperando que se confirme el viaje. Antes usaba el stock total
        // de todas las ubicaciones, lo que podía mostrar más de lo realmente vendible.
        private const string UbicacionVitrina = "Mercado Campesino";

        // Todo producto Terminado se vende por unidad (una torta) — nunca varía, así que no se le
        // pregunta al usuario. Coincide con el valor real ya usado por la mayoría del catálogo.
        private const string UnidadMedidaTerminado = "unidad";

        private static readonly string[] TiposValidos = { "Terminado", "MateriaPrima" };
        private static readonly string[] CategoriasPersonalizacionValidas = { "Relleno", "Crema", "Colorante" };
        private static readonly string[] TiposCremaValidos = { "Mascrean", "CremaPil", "Fondant" };
        private static readonly string[] TiposMasaValidos = { "Vainilla", "Chocolate", "Mixto" };
        private static readonly string[] CategoriasTerminadoValidas = { "Tortas Clásicas", "Tortas Personalizables" };
        private static readonly string[] CategoriasMateriaPrimaValidas =
            { "Harinas y Secos", "Lácteos y Cremas", "Colorantes y Jaleas", "Rellenos", "Empaques" };

        // Lista cerrada — antes era texto libre y cualquier variante ("Kg", "kilo", "Kilogramos")
        // rompía la trazabilidad de recetas/consumo, que compara por igualdad exacta de string.
        private static readonly string[] UnidadesMedidaValidas = { "kg", "unidad" };

        // Derivados de las 6 jaleas reales cargadas en Colorantes y Jaleas (Frutilla=Rojo,
        // Naranja=Naranja, Limón=Verde, Piña=Amarillo, Uva=Morado, Chicle=Azul — decisión ya
        // confirmada por el usuario, 2026-07-13) + "Blanco" para tortas sin colorante artificial.
        // Solo aplica a tortas de vitrina (receta fija) — las personalizables ya no llevan esto,
        // el color lo elige el cliente por pedido.
        private static readonly string[] ColoresDecoracionValidos =
            { "Blanco", "Rojo", "Naranja", "Verde", "Amarillo", "Morado", "Azul" };

        private const string PrefijoCodigoTerminado = "PT-TORTA-";
        private const string PrefijoCodigoMateriaPrima = "MP-";

        private readonly IItemCatalogoRepository _itemCatalogoRepository;
        private readonly ILoteRepository _loteRepository;
        private readonly IRecetaRepository _recetaRepository;

        public ItemCatalogoService(
            IItemCatalogoRepository itemCatalogoRepository,
            ILoteRepository loteRepository,
            IRecetaRepository recetaRepository)
        {
            _itemCatalogoRepository = itemCatalogoRepository;
            _loteRepository = loteRepository;
            _recetaRepository = recetaRepository;
        }

        public async Task<ItemCatalogoDto> CrearAsync(CrearItemCatalogoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                throw new ReglaNegocioException("El nombre es obligatorio.");
            }

            if (!TiposValidos.Contains(request.Tipo))
            {
                throw new ReglaNegocioException("El tipo debe ser 'Terminado' o 'MateriaPrima'.");
            }

            if (request.Tipo == "Terminado" && (request.NumeroPorciones is null || request.NumeroPorciones <= 0))
            {
                throw new ReglaNegocioException("El número de porciones es obligatorio y debe ser mayor a 0 para un producto terminado.");
            }

            // Se deriva de la categoría en vez de preguntarse dos veces (la categoría "Tortas
            // Personalizables" ya responde esto) — y el precio de una personalizable no se fija
            // aquí, se calcula por porciones vía Tabla_Precio_Porciones.
            var esPersonalizable = request.Tipo == "Terminado" && request.Categoria == "Tortas Personalizables";
            var esVitrina = request.Tipo == "Terminado" && !esPersonalizable;
            var precioUnitario = esPersonalizable ? 0 : request.PrecioUnitario;

            if (precioUnitario < 0)
            {
                throw new ReglaNegocioException("El precio unitario no puede ser negativo.");
            }

            ValidarLongitudes(request.Nombre, request.Categoria, request.Descripcion);
            ValidarCategoria(request.Tipo, request.Categoria);
            ValidarUnidadMedida(request.Tipo, request.UnidadMedida);
            ValidarColorDecoracion(esVitrina, request.ColorDecoracion);
            ValidarCategoriaPersonalizacion(request.Tipo, request.CategoriaPersonalizacion);
            ValidarTipoCremaAsociado(request.CategoriaPersonalizacion, request.TipoCremaAsociado);
            await ValidarRecetaFijaAsync(esVitrina, request.TipoMasa, request.IdInsumoRelleno, request.IdInsumoCrema);

            var codigo = await GenerarCodigoAsync(request.Tipo);

            var item = new ItemCatalogo
            {
                Id = Guid.NewGuid(),
                CodigoReferencia = codigo,
                Nombre = request.Nombre.Trim(),
                Categoria = request.Categoria.Trim(),
                Tipo = request.Tipo,
                UnidadMedida = request.Tipo == "Terminado" ? UnidadMedidaTerminado : request.UnidadMedida.Trim(),
                PrecioUnitario = precioUnitario,
                NumeroPorciones = request.Tipo == "Terminado" ? request.NumeroPorciones : null,
                EsPersonalizable = esPersonalizable,
                CategoriaPersonalizacion = request.Tipo == "MateriaPrima" ? request.CategoriaPersonalizacion : null,
                TipoCremaAsociado = request.Tipo == "MateriaPrima" && request.CategoriaPersonalizacion == "Crema" ? request.TipoCremaAsociado : null,
                Descripcion = request.Descripcion,
                ColorDecoracion = esVitrina ? request.ColorDecoracion : null,
                TipoMasa = esVitrina ? request.TipoMasa : null,
                IdInsumoRelleno = esVitrina ? request.IdInsumoRelleno : null,
                IdInsumoCrema = esVitrina ? request.IdInsumoCrema : null,
                Activo = true
            };

            await _itemCatalogoRepository.AgregarAsync(item);
            await _itemCatalogoRepository.GuardarCambiosAsync();

            return MapearDto(item);
        }

        public async Task<List<ItemCatalogoDto>> ObtenerTodosAsync()
        {
            var items = await _itemCatalogoRepository.ObtenerTodosAsync();
            var lookup = items.ToDictionary(i => i.Id);
            return items.Select(i => MapearDto(i, lookup)).ToList();
        }

        public async Task<List<ItemCatalogoDto>> ObtenerInsumosPersonalizacionAsync()
        {
            var candidatos = (await _itemCatalogoRepository.ObtenerTodosAsync())
                .Where(i => i.Tipo == "MateriaPrima" && i.Activo && i.CategoriaPersonalizacion is not null)
                .ToList();

            var disponibles = new List<ItemCatalogo>();
            foreach (var insumo in candidatos)
            {
                var stock = await _loteRepository.ObtenerStockDisponibleTotalAsync(insumo.Id);
                if (stock > 0)
                {
                    disponibles.Add(insumo);
                }
            }

            return disponibles.Select(i => MapearDto(i)).ToList();
        }

        public async Task<List<(ItemCatalogoDto Item, decimal StockDisponible)>> ObtenerCatalogoPublicoConStockAsync()
        {
            var todos = await _itemCatalogoRepository.ObtenerTodosAsync();
            var lookup = todos.ToDictionary(i => i.Id);
            var items = todos.Where(i => i.Tipo == "Terminado" && i.Activo).ToList();

            var resultado = new List<(ItemCatalogoDto, decimal)>();
            foreach (var item in items)
            {
                var stock = await _loteRepository.ObtenerStockDisponiblePorUbicacionAsync(item.Id, UbicacionVitrina);
                resultado.Add((MapearDto(item, lookup), stock));
            }

            return resultado;
        }

        public async Task<(ItemCatalogoDto Item, decimal StockDisponible)> ObtenerCatalogoPublicoConStockPorIdAsync(Guid id)
        {
            var item = await _itemCatalogoRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró el producto con id {id}.");

            if (item.Tipo != "Terminado" || !item.Activo)
            {
                throw new NoEncontradoException($"No se encontró el producto con id {id}.");
            }

            var lookup = new Dictionary<Guid, ItemCatalogo>();
            if (item.IdInsumoRelleno is not null || item.IdInsumoCrema is not null)
            {
                lookup = (await _itemCatalogoRepository.ObtenerTodosAsync()).ToDictionary(i => i.Id);
            }

            var stock = await _loteRepository.ObtenerStockDisponiblePorUbicacionAsync(item.Id, UbicacionVitrina);
            return (MapearDto(item, lookup), stock);
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

            if (request.Tipo == "Terminado" && (request.NumeroPorciones is null || request.NumeroPorciones <= 0))
            {
                throw new ReglaNegocioException("El número de porciones es obligatorio y debe ser mayor a 0 para un producto terminado.");
            }

            var esPersonalizable = request.Tipo == "Terminado" && request.Categoria == "Tortas Personalizables";
            var esVitrina = request.Tipo == "Terminado" && !esPersonalizable;
            var precioUnitario = esPersonalizable ? 0 : request.PrecioUnitario;

            if (precioUnitario < 0)
            {
                throw new ReglaNegocioException("El precio unitario no puede ser negativo.");
            }

            ValidarLongitudes(request.Nombre, request.Categoria, request.Descripcion);
            ValidarCategoria(request.Tipo, request.Categoria);
            ValidarUnidadMedida(request.Tipo, request.UnidadMedida);
            ValidarColorDecoracion(esVitrina, request.ColorDecoracion);
            ValidarCategoriaPersonalizacion(request.Tipo, request.CategoriaPersonalizacion);
            ValidarTipoCremaAsociado(request.CategoriaPersonalizacion, request.TipoCremaAsociado);
            await ValidarRecetaFijaAsync(esVitrina, request.TipoMasa, request.IdInsumoRelleno, request.IdInsumoCrema);

            if (request.Tipo != item.Tipo)
            {
                var comoTerminado = await _recetaRepository.ObtenerPorProductoTerminadoAsync(item.Id);
                var comoInsumo = await _recetaRepository.ObtenerPorInsumoAsync(item.Id);
                if (comoTerminado.Count > 0 || comoInsumo.Count > 0)
                {
                    throw new ReglaNegocioException(
                        $"No se puede cambiar el tipo de '{item.Nombre}': tiene recetas asociadas que dependen de su tipo actual.");
                }
            }

            item.Nombre = request.Nombre.Trim();
            item.Categoria = request.Categoria.Trim();
            item.Tipo = request.Tipo;
            item.UnidadMedida = request.Tipo == "Terminado" ? UnidadMedidaTerminado : request.UnidadMedida.Trim();
            item.PrecioUnitario = precioUnitario;
            item.NumeroPorciones = request.Tipo == "Terminado" ? request.NumeroPorciones : null;
            item.EsPersonalizable = esPersonalizable;
            item.CategoriaPersonalizacion = request.Tipo == "MateriaPrima" ? request.CategoriaPersonalizacion : null;
            item.TipoCremaAsociado = request.Tipo == "MateriaPrima" && request.CategoriaPersonalizacion == "Crema" ? request.TipoCremaAsociado : null;
            item.Descripcion = request.Descripcion;
            item.ColorDecoracion = esVitrina ? request.ColorDecoracion : null;
            item.TipoMasa = esVitrina ? request.TipoMasa : null;
            item.IdInsumoRelleno = esVitrina ? request.IdInsumoRelleno : null;
            item.IdInsumoCrema = esVitrina ? request.IdInsumoCrema : null;

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

        private static void ValidarCategoria(string tipo, string categoria)
        {
            var categoriasValidas = tipo == "Terminado" ? CategoriasTerminadoValidas : CategoriasMateriaPrimaValidas;
            if (!categoriasValidas.Contains(categoria))
            {
                throw new ReglaNegocioException(
                    $"Categoría inválida para {(tipo == "Terminado" ? "un producto terminado" : "un insumo")}. Use una de: {string.Join(", ", categoriasValidas)}.");
            }
        }

        private static void ValidarLongitudes(string nombre, string categoria, string? descripcion)
        {
            if (nombre.Length > LongitudMaximaNombre)
            {
                throw new ReglaNegocioException($"El nombre no puede superar los {LongitudMaximaNombre} caracteres.");
            }

            if (categoria.Length > LongitudMaximaCategoria)
            {
                throw new ReglaNegocioException($"La categoría no puede superar los {LongitudMaximaCategoria} caracteres.");
            }

            if (descripcion?.Length > LongitudMaximaDescripcion)
            {
                throw new ReglaNegocioException($"La descripción no puede superar los {LongitudMaximaDescripcion} caracteres.");
            }
        }

        private static void ValidarUnidadMedida(string tipo, string? unidadMedida)
        {
            if (tipo != "MateriaPrima")
            {
                return;
            }

            if (unidadMedida is null || !UnidadesMedidaValidas.Contains(unidadMedida))
            {
                throw new ReglaNegocioException(
                    $"Unidad de medida inválida. Use una de: {string.Join(", ", UnidadesMedidaValidas)}.");
            }
        }

        private static void ValidarColorDecoracion(bool esVitrina, string? colorDecoracion)
        {
            if (!esVitrina || colorDecoracion is null)
            {
                return;
            }

            if (!ColoresDecoracionValidos.Contains(colorDecoracion))
            {
                throw new ReglaNegocioException(
                    $"Color de decoración inválido. Use uno de: {string.Join(", ", ColoresDecoracionValidos)}.");
            }
        }

        // PT-TORTA-NNN / MP-NNN, autonumerado — el usuario ya no lo escribe a mano. Busca el
        // número más alto ya usado con ese prefijo exacto y sigue desde ahí; códigos antiguos con
        // otro esquema (ej. MP-CARA-001, de antes de que esto se automatizara) se ignoran, no
        // se migran.
        private async Task<string> GenerarCodigoAsync(string tipo)
        {
            var prefijo = tipo == "Terminado" ? PrefijoCodigoTerminado : PrefijoCodigoMateriaPrima;
            var patron = new Regex($"^{Regex.Escape(prefijo)}(\\d+)$");

            var maximo = (await _itemCatalogoRepository.ObtenerTodosAsync())
                .Select(i => patron.Match(i.CodigoReferencia))
                .Where(m => m.Success)
                .Select(m => int.Parse(m.Groups[1].Value))
                .DefaultIfEmpty(0)
                .Max();

            return $"{prefijo}{maximo + 1:D3}";
        }

        private static void ValidarCategoriaPersonalizacion(string tipo, string? categoriaPersonalizacion)
        {
            if (tipo != "MateriaPrima" || categoriaPersonalizacion is null)
            {
                return;
            }

            if (!CategoriasPersonalizacionValidas.Contains(categoriaPersonalizacion))
            {
                throw new ReglaNegocioException(
                    $"Categoría de personalización inválida. Valores permitidos: {string.Join(", ", CategoriasPersonalizacionValidas)}.");
            }
        }

        private static void ValidarTipoCremaAsociado(string? categoriaPersonalizacion, string? tipoCremaAsociado)
        {
            if (categoriaPersonalizacion != "Crema" || tipoCremaAsociado is null)
            {
                return;
            }

            if (!TiposCremaValidos.Contains(tipoCremaAsociado))
            {
                throw new ReglaNegocioException(
                    $"Tipo de crema asociado inválido. Valores permitidos: {string.Join(", ", TiposCremaValidos)}.");
            }
        }

        private async Task ValidarRecetaFijaAsync(bool esVitrina, string? tipoMasa, Guid? idInsumoRelleno, Guid? idInsumoCrema)
        {
            if (!esVitrina)
            {
                return;
            }

            if (tipoMasa is not null && !TiposMasaValidos.Contains(tipoMasa))
            {
                throw new ReglaNegocioException(
                    $"Tipo de masa inválido. Valores permitidos: {string.Join(", ", TiposMasaValidos)}.");
            }

            if (idInsumoRelleno is not null)
            {
                await ValidarInsumoRecetaFijaAsync(idInsumoRelleno.Value, "Relleno");
            }

            if (idInsumoCrema is not null)
            {
                await ValidarInsumoRecetaFijaAsync(idInsumoCrema.Value, "Crema");
            }
        }

        private async Task ValidarInsumoRecetaFijaAsync(Guid idInsumo, string categoriaEsperada)
        {
            var insumo = await _itemCatalogoRepository.ObtenerPorIdAsync(idInsumo)
                ?? throw new NoEncontradoException($"No se encontró el insumo con id {idInsumo}.");

            if (insumo.Tipo != "MateriaPrima" || insumo.CategoriaPersonalizacion != categoriaEsperada || !insumo.Activo)
            {
                throw new ReglaNegocioException($"'{insumo.Nombre}' no es un insumo válido de tipo '{categoriaEsperada}'.");
            }
        }

        private static ItemCatalogoDto MapearDto(ItemCatalogo item, IReadOnlyDictionary<Guid, ItemCatalogo>? lookup = null) => new()
        {
            Id = item.Id,
            CodigoReferencia = item.CodigoReferencia,
            Nombre = item.Nombre,
            Categoria = item.Categoria,
            Tipo = item.Tipo,
            UnidadMedida = item.UnidadMedida,
            PrecioUnitario = item.PrecioUnitario,
            NumeroPorciones = item.NumeroPorciones,
            EsPersonalizable = item.EsPersonalizable,
            CategoriaPersonalizacion = item.CategoriaPersonalizacion,
            TipoCremaAsociado = item.TipoCremaAsociado,
            ImagenUrl = item.ImagenUrl,
            Descripcion = item.Descripcion,
            ColorDecoracion = item.ColorDecoracion,
            TipoMasa = item.TipoMasa,
            IdInsumoRelleno = item.IdInsumoRelleno,
            IdInsumoCrema = item.IdInsumoCrema,
            NombreInsumoRelleno = item.IdInsumoRelleno is not null && lookup is not null && lookup.TryGetValue(item.IdInsumoRelleno.Value, out var relleno)
                ? relleno.Nombre : null,
            NombreInsumoCrema = item.IdInsumoCrema is not null && lookup is not null && lookup.TryGetValue(item.IdInsumoCrema.Value, out var crema)
                ? crema.Nombre : null,
            Activo = item.Activo
        };

        public async Task<ItemCatalogoDto> ActualizarImagenUrlAsync(Guid id, string imagenUrl)
        {
            var item = await _itemCatalogoRepository.ObtenerPorIdAsync(id)
                ?? throw new NoEncontradoException($"No se encontró el ítem con id {id}.");

            item.ImagenUrl = imagenUrl;
            await _itemCatalogoRepository.GuardarCambiosAsync();

            return MapearDto(item);
        }
    }
}
