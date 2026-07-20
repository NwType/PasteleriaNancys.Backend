using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Inventario.Dtos;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Services
{
    /// <summary>
    /// Control de mermas (requisito del tutor, 2026-07-17). Dos casos:
    /// 1) Merma directa de stock — insumo dañado (huevos podridos), accidente, caducidad:
    ///    descuenta del lote indicado o por orden PEPS.
    /// 2) Producción fallida — una preparación salió mal antes de entrar a stock: se explota
    ///    la receta y se descuentan los componentes arruinados como merma. La reposición se
    ///    registra aparte como producción normal (Horneada/Viaje), que descuenta lo suyo por
    ///    receta — así quedan registrados tanto lo arruinado como lo repuesto.
    /// </summary>
    public class MermaService : IMermaService
    {
        private const string UbicacionProduccion = "San Mateo";
        private const string UbicacionVitrina = "Mercado Campesino";

        private static readonly string[] TiposValidos =
            { "Insumo dañado", "Producción fallida", "Caducidad", "Accidente", "Otro" };

        private readonly IMermaRepository _mermaRepository;
        private readonly IItemCatalogoRepository _itemCatalogoRepository;
        private readonly ILoteRepository _loteRepository;
        private readonly IRecetaRepository _recetaRepository;

        public MermaService(
            IMermaRepository mermaRepository,
            IItemCatalogoRepository itemCatalogoRepository,
            ILoteRepository loteRepository,
            IRecetaRepository recetaRepository)
        {
            _mermaRepository = mermaRepository;
            _itemCatalogoRepository = itemCatalogoRepository;
            _loteRepository = loteRepository;
            _recetaRepository = recetaRepository;
        }

        public async Task<List<MermaDto>> RegistrarAsync(Guid idUsuarioRegistro, RegistrarMermaRequest request)
        {
            ValidarComunes(request.Cantidad, request.TipoMerma);

            var item = await _itemCatalogoRepository.ObtenerPorIdAsync(request.IdItem)
                ?? throw new NoEncontradoException($"No se encontró el ítem con id {request.IdItem}.");

            List<Merma> mermas;
            if (request.IdLote is not null)
            {
                var lote = await _loteRepository.ObtenerPorIdAsync(request.IdLote.Value)
                    ?? throw new NoEncontradoException($"No se encontró el lote con id {request.IdLote}.");

                if (lote.IdItem != item.Id)
                {
                    throw new ReglaNegocioException($"El lote indicado no pertenece a '{item.Nombre}'.");
                }

                if (lote.CantidadDisponible < request.Cantidad)
                {
                    throw new ReglaNegocioException(
                        $"El lote solo tiene {lote.CantidadDisponible:0.###} {item.UnidadMedida} disponibles.");
                }

                lote.CantidadDisponible -= request.Cantidad;
                mermas = new List<Merma>
                {
                    CrearMerma(item, lote.Id, request.Cantidad, request.TipoMerma, request.Motivo?.Trim(), idUsuarioRegistro)
                };
            }
            else
            {
                mermas = await DescontarPepsComoMermaAsync(
                    item, request.Cantidad, request.TipoMerma, request.Motivo?.Trim(), idUsuarioRegistro);
            }

            foreach (var merma in mermas)
            {
                await _mermaRepository.AgregarAsync(merma);
            }

            await _mermaRepository.GuardarCambiosAsync();
            return mermas.Select(m => MapearDto(m, item)).ToList();
        }

        public async Task<List<MermaDto>> RegistrarProduccionFallidaAsync(Guid idUsuarioRegistro, RegistrarMermaProduccionRequest request)
        {
            if (request.Cantidad <= 0)
            {
                throw new ReglaNegocioException("La cantidad debe ser mayor a 0.");
            }

            var producto = await _itemCatalogoRepository.ObtenerPorIdAsync(request.IdItemProducto)
                ?? throw new NoEncontradoException($"No se encontró el ítem con id {request.IdItemProducto}.");

            if (producto.Tipo == "MateriaPrima")
            {
                throw new ReglaNegocioException(
                    "Para materia prima dañada use la merma directa — 'producción fallida' es para preparaciones con receta.");
            }

            var receta = await _recetaRepository.ObtenerPorProductoTerminadoAsync(producto.Id);
            if (receta.Count == 0)
            {
                throw new ReglaNegocioException(
                    $"'{producto.Nombre}' no tiene receta cargada — no se puede calcular qué insumos se arruinaron.");
            }

            var motivo = $"Producción fallida: {producto.Nombre} ×{request.Cantidad:0.##}"
                + (string.IsNullOrWhiteSpace(request.Motivo) ? string.Empty : $" — {request.Motivo.Trim()}");

            var mermas = new List<(Merma Merma, Domain.Inventario.ItemCatalogo Componente)>();
            foreach (var linea in receta)
            {
                var componente = await _itemCatalogoRepository.ObtenerPorIdAsync(linea.IdItemInsumo)
                    ?? throw new ReglaNegocioException(
                        $"La receta de '{producto.Nombre}' referencia un componente que ya no existe en el catálogo.");

                var perdido = await DescontarPepsComoMermaAsync(
                    componente, linea.CantidadRequerida * request.Cantidad, "Producción fallida", motivo, idUsuarioRegistro);
                mermas.AddRange(perdido.Select(m => (m, componente)));
            }

            foreach (var (merma, _) in mermas)
            {
                await _mermaRepository.AgregarAsync(merma);
            }

            await _mermaRepository.GuardarCambiosAsync();
            return mermas.Select(p => MapearDto(p.Merma, p.Componente)).ToList();
        }

        public async Task<List<MermaDto>> ObtenerTodasAsync()
        {
            var mermas = await _mermaRepository.ObtenerTodasAsync();
            return mermas
                .OrderByDescending(m => m.Fecha)
                .Select(m => MapearDto(m, m.Item))
                .ToList();
        }

        private static void ValidarComunes(decimal cantidad, string tipoMerma)
        {
            if (cantidad <= 0)
            {
                throw new ReglaNegocioException("La cantidad debe ser mayor a 0.");
            }

            if (!TiposValidos.Contains(tipoMerma))
            {
                throw new ReglaNegocioException(
                    $"Tipo de merma inválido. Use uno de: {string.Join(", ", TiposValidos)}.");
            }
        }

        // Descuenta la pérdida por orden PEPS — primero San Mateo (producción, donde viven los
        // insumos) y si no alcanza sigue en la vitrina (Mercado Campesino, donde puede estar el
        // producto terminado accidentado).
        private async Task<List<Merma>> DescontarPepsComoMermaAsync(
            Domain.Inventario.ItemCatalogo item, decimal cantidad, string tipoMerma, string? motivo, Guid idUsuarioRegistro)
        {
            var lotes = await _loteRepository.ObtenerDisponiblesParaVentaAsync(item.Id, UbicacionProduccion);
            lotes.AddRange(await _loteRepository.ObtenerDisponiblesParaVentaAsync(item.Id, UbicacionVitrina));

            var mermas = new List<Merma>();
            var restante = cantidad;

            foreach (var lote in lotes)
            {
                if (restante <= 0) break;

                var descuento = Math.Min(lote.CantidadDisponible, restante);
                lote.CantidadDisponible -= descuento;
                restante -= descuento;

                mermas.Add(CrearMerma(item, lote.Id, descuento, tipoMerma, motivo, idUsuarioRegistro));
            }

            if (restante > 0)
            {
                throw new ReglaNegocioException(
                    $"Stock insuficiente de '{item.Nombre}' para registrar la merma: faltan {restante:0.###} {item.UnidadMedida}.");
            }

            return mermas;
        }

        private static Merma CrearMerma(
            Domain.Inventario.ItemCatalogo item, Guid idLote, decimal cantidad, string tipoMerma, string? motivo, Guid idUsuarioRegistro) => new()
        {
            Id = Guid.NewGuid(),
            IdItem = item.Id,
            IdLote = idLote,
            Cantidad = cantidad,
            TipoMerma = tipoMerma,
            Motivo = motivo,
            Fecha = DateTime.UtcNow,
            IdUsuarioRegistro = idUsuarioRegistro
        };

        private static MermaDto MapearDto(Merma merma, Domain.Inventario.ItemCatalogo item) => new()
        {
            Id = merma.Id,
            IdItem = merma.IdItem,
            CodigoReferencia = item.CodigoReferencia,
            NombreItem = item.Nombre,
            UnidadMedida = item.UnidadMedida,
            IdLote = merma.IdLote,
            Cantidad = merma.Cantidad,
            TipoMerma = merma.TipoMerma,
            Motivo = merma.Motivo,
            Fecha = merma.Fecha
        };
    }
}
