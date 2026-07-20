using System.Text.RegularExpressions;
using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Application.Pedidos.Dtos;
using PasteleriaNancys.Application.Pedidos.Interfaces;
using PasteleriaNancys.Domain.Inventario;
using PasteleriaNancys.Domain.Pedidos;

namespace PasteleriaNancys.Application.Pedidos.Services
{
    public class PedidoService : IPedidoService
    {
        private static readonly Regex WhatsAppRegex = new(@"^\+?\d{7,15}$", RegexOptions.Compiled);
        private static readonly Regex SoloDigitosRegex = new(@"^\d*$", RegexOptions.Compiled);

        private static readonly Dictionary<string, string> TransicionesValidas = new()
        {
            ["Confirmado"] = "En Producción",
            ["En Producción"] = "Listo para Entrega",
            ["Listo para Entrega"] = "Entregado"
        };

        private static readonly string[] TiposMasaValidos = { "Vainilla", "Chocolate", "Mixto" };
        private static readonly string[] TiposCremaValidos = { "Mascrean", "CremaPil", "Fondant" };

        private const decimal PrecioVelaNormal = 1m;
        private const decimal PrecioPorDigitoVelaNumero = 3m;

        private readonly IPedidoRepository _pedidoRepository;
        private readonly IPagoQrRepository _pagoQrRepository;
        private readonly IPagoQrService _pagoQrService;
        private readonly IItemCatalogoRepository _itemCatalogoRepository;
        private readonly ILoteRepository _loteRepository;
        private readonly ITablaPrecioPorcionesRepository _tablaPrecioPorcionesRepository;
        private readonly IConsumoService _consumoService;

        public PedidoService(
            IPedidoRepository pedidoRepository,
            IPagoQrRepository pagoQrRepository,
            IPagoQrService pagoQrService,
            IItemCatalogoRepository itemCatalogoRepository,
            ILoteRepository loteRepository,
            ITablaPrecioPorcionesRepository tablaPrecioPorcionesRepository,
            IConsumoService consumoService)
        {
            _pedidoRepository = pedidoRepository;
            _pagoQrRepository = pagoQrRepository;
            _pagoQrService = pagoQrService;
            _itemCatalogoRepository = itemCatalogoRepository;
            _loteRepository = loteRepository;
            _tablaPrecioPorcionesRepository = tablaPrecioPorcionesRepository;
            _consumoService = consumoService;
        }

        public Task<PedidoDto> CrearAsync(CrearPedidoWebRequest request) =>
            CrearInternoAsync(request, idUsuarioVendedora: null);

        public Task<PedidoDto> RegistrarPresencialAsync(Guid idUsuarioVendedora, CrearPedidoWebRequest request) =>
            CrearInternoAsync(request, idUsuarioVendedora);

        private async Task<PedidoDto> CrearInternoAsync(CrearPedidoWebRequest request, Guid? idUsuarioVendedora)
        {
            if (string.IsNullOrWhiteSpace(request.NombreCliente))
            {
                throw new ReglaNegocioException("El nombre del cliente es obligatorio.");
            }

            if (!WhatsAppRegex.IsMatch(request.WhatsApp))
            {
                throw new ReglaNegocioException("El número de WhatsApp no tiene un formato válido.");
            }

            if (!EsFechaEntregaValida(request.FechaEntregaSolicitada))
            {
                throw new ReglaNegocioException("La fecha de entrega debe ser al menos 2 días hábiles después de la solicitud.");
            }

            var item = await _itemCatalogoRepository.ObtenerPorIdAsync(request.IdItemProductoBase)
                ?? throw new NoEncontradoException($"No se encontró el producto con id {request.IdItemProductoBase}.");

            if (item.Tipo != "Terminado" || !item.Activo)
            {
                throw new ReglaNegocioException("El producto seleccionado no está disponible.");
            }

            // El stock de insumos base de la receta (harina, huevos, etc.) NO bloquea la toma del
            // pedido: el horneado se hace por lotes según la Proyección de Horneado, no por pedido
            // individual, y los insumos se reponen antes del corte de horneado del día. Solo se
            // valida disponibilidad de los insumos de personalización que el cliente elige
            // explícitamente (crema/relleno/color) — ver ValidarInsumoAsync.

            var saborMasaNombre = item.Nombre;
            Guid? idInsumoSaborMasa = null;
            if (request.IdInsumoSaborMasa.HasValue)
            {
                var insumoSaborMasa = await ValidarInsumoAsync(request.IdInsumoSaborMasa.Value);
                saborMasaNombre = insumoSaborMasa.Nombre;
                idInsumoSaborMasa = insumoSaborMasa.Id;
            }

            var tipoRellenoNombre = "Según receta clásica";
            Guid? idInsumoRelleno = null;
            ItemCatalogo? insumoRelleno = null;
            if (request.IdInsumoRelleno.HasValue)
            {
                insumoRelleno = await ValidarInsumoAsync(request.IdInsumoRelleno.Value, "Relleno");
                tipoRellenoNombre = insumoRelleno.Nombre;
                idInsumoRelleno = insumoRelleno.Id;
            }

            var esTortaPersonalizable = item.EsPersonalizable;
            TablaPrecioPorciones? precioPorciones = null;
            ItemCatalogo? insumoCrema = null;
            ItemCatalogo? insumoColor = null;
            var colorDecoracionNombre = request.ColorDecoracion;

            if (esTortaPersonalizable)
            {
                precioPorciones = await ValidarNumeroPorcionesAsync(item.Id, request.NumeroPorciones);
                ValidarTipoMasa(request.TipoMasa);
                ValidarTipoCrema(request.TipoCrema);

                if (!request.IdInsumoCrema.HasValue)
                {
                    throw new ReglaNegocioException("Debe seleccionar el insumo de crema para la torta.");
                }
                insumoCrema = await ValidarInsumoAsync(request.IdInsumoCrema.Value, "Crema");
                if (insumoCrema.TipoCremaAsociado != request.TipoCrema)
                {
                    throw new ReglaNegocioException(
                        $"'{insumoCrema.Nombre}' no corresponde al tipo de crema '{request.TipoCrema}' seleccionado.");
                }

                if (request.IdInsumoColorDecoracion.HasValue)
                {
                    insumoColor = await ValidarInsumoAsync(request.IdInsumoColorDecoracion.Value, "Colorante");
                    colorDecoracionNombre = insumoColor.Nombre;
                }
            }

            if (request.CantidadVelasNormales < 0)
            {
                throw new ReglaNegocioException("La cantidad de velas normales no puede ser negativa.");
            }
            if (!string.IsNullOrEmpty(request.VelaNumerica) && !SoloDigitosRegex.IsMatch(request.VelaNumerica))
            {
                throw new ReglaNegocioException("La vela de número solo puede contener dígitos.");
            }

            var totalCotizado = CalcularPrecio(
                item, esTortaPersonalizable, precioPorciones, request.TipoCrema, insumoCrema,
                insumoRelleno, insumoColor, request.CantidadVelasNormales, request.VelaNumerica);

            var pedido = new PedidoCliente
            {
                Id = Guid.NewGuid(),
                NombreCliente = request.NombreCliente.Trim(),
                WhatsApp = request.WhatsApp.Trim(),
                FechaSolicitud = DateTime.UtcNow,
                FechaEntregaSolicitada = request.FechaEntregaSolicitada,
                TotalCotizado = totalCotizado,
                Estado = "Pendiente QR",
                Observaciones = request.Observaciones,
                IdUsuarioVendedora = idUsuarioVendedora
            };
            pedido.CodigoQrReferencia = $"PED-{pedido.Id:N}"[..12].ToUpperInvariant();

            pedido.Configuracion = new PedidoConfiguracion
            {
                Id = Guid.NewGuid(),
                IdPedido = pedido.Id,
                IdItemProductoBase = item.Id,
                IdInsumoSaborMasa = idInsumoSaborMasa,
                IdInsumoRelleno = idInsumoRelleno,
                SaborMasa = saborMasaNombre,
                TipoRelleno = tipoRellenoNombre,
                TamanoRacion = request.TamanoRacion,
                ColorDecoracion = colorDecoracionNombre,
                DedicatoriaDetalle = request.DedicatoriaDetalle,
                ImagenReferenciaUrl = request.ImagenReferenciaUrl,
                PorcentajeAnticipo = request.PorcentajeAnticipo,
                NumeroPorciones = esTortaPersonalizable ? request.NumeroPorciones : null,
                TipoMasa = esTortaPersonalizable ? request.TipoMasa : null,
                TipoCrema = esTortaPersonalizable ? request.TipoCrema : null,
                IdInsumoCrema = insumoCrema?.Id,
                IdInsumoColorDecoracion = insumoColor?.Id,
                CantidadVelasNormales = request.CantidadVelasNormales,
                VelaNumerica = request.VelaNumerica
            };

            await _pedidoRepository.AgregarAsync(pedido);
            await _pedidoRepository.GuardarCambiosAsync();

            var pagoDto = await _pagoQrService.GenerarAsync(pedido);

            return MapearDto(pedido, pagoDto);
        }

        public async Task<List<TablaPrecioPorcionesDto>> ObtenerTablaPrecioPorcionesAsync(Guid idItemTerminado)
        {
            var tabla = await _tablaPrecioPorcionesRepository.ObtenerActivosPorProductoAsync(idItemTerminado);
            return tabla.Select(t => new TablaPrecioPorcionesDto
            {
                NumeroPorciones = t.NumeroPorciones,
                Precio = t.Precio,
                Descripcion = t.Descripcion
            }).ToList();
        }

        private static decimal CalcularPrecio(
            ItemCatalogo item,
            bool esTortaPersonalizable,
            TablaPrecioPorciones? precioPorciones,
            string? tipoCrema,
            ItemCatalogo? insumoCrema,
            ItemCatalogo? insumoRelleno,
            ItemCatalogo? insumoColor,
            int cantidadVelasNormales,
            string? velaNumerica)
        {
            decimal total;
            if (esTortaPersonalizable && precioPorciones is not null)
            {
                total = precioPorciones.Precio;
                if (tipoCrema == "CremaPil")
                {
                    total *= 2;
                }
                else if (tipoCrema == "Fondant" && insumoCrema is not null)
                {
                    total += insumoCrema.PrecioUnitario;
                }

                total += insumoRelleno?.PrecioUnitario ?? 0;
                total += insumoColor?.PrecioUnitario ?? 0;
            }
            else
            {
                total = item.PrecioUnitario;
            }

            total += cantidadVelasNormales * PrecioVelaNormal;
            total += (velaNumerica?.Length ?? 0) * PrecioPorDigitoVelaNumero;

            return total;
        }

        private async Task<TablaPrecioPorciones> ValidarNumeroPorcionesAsync(Guid idItemTerminado, int? numeroPorciones)
        {
            if (!numeroPorciones.HasValue)
            {
                throw new ReglaNegocioException("Debe indicar el número de porciones para esta torta.");
            }

            var precio = await _tablaPrecioPorcionesRepository.ObtenerPorProductoYPorcionesAsync(idItemTerminado, numeroPorciones.Value)
                ?? throw new ReglaNegocioException(
                    $"No existe una opción de {numeroPorciones.Value} porciones para esta torta. Consulte la tabla de precios disponible.");

            if (!precio.Activo)
            {
                throw new ReglaNegocioException($"La opción de {numeroPorciones.Value} porciones no está disponible actualmente.");
            }

            return precio;
        }

        private static void ValidarTipoMasa(string? tipoMasa)
        {
            if (string.IsNullOrWhiteSpace(tipoMasa) || !TiposMasaValidos.Contains(tipoMasa))
            {
                throw new ReglaNegocioException($"Tipo de masa inválido. Valores permitidos: {string.Join(", ", TiposMasaValidos)}.");
            }
        }

        private static void ValidarTipoCrema(string? tipoCrema)
        {
            if (string.IsNullOrWhiteSpace(tipoCrema) || !TiposCremaValidos.Contains(tipoCrema))
            {
                throw new ReglaNegocioException($"Tipo de crema inválido. Valores permitidos: {string.Join(", ", TiposCremaValidos)}.");
            }
        }

        public async Task<PedidoDto> ConsultarEstadoAsync(string whatsApp, string codigoReferencia)
        {
            var pedido = await _pedidoRepository.ObtenerPorWhatsAppYCodigoAsync(whatsApp, codigoReferencia)
                ?? throw new NoEncontradoException("No se encontró ningún pedido con esos datos.");

            return await MapearDtoConPagoAsync(pedido);
        }

        public async Task<List<PedidoDto>> ObtenerPendientesAsync()
        {
            var pedidos = await _pedidoRepository.ObtenerPendientesAsync();
            var resultado = new List<PedidoDto>();
            foreach (var pedido in pedidos)
            {
                resultado.Add(await MapearDtoConPagoAsync(pedido));
            }

            return resultado;
        }

        public async Task<List<PedidoDto>> ObtenerTodosAsync(string? estado, DateTime? fechaEntrega)
        {
            var pedidos = await _pedidoRepository.ObtenerTodosAsync(estado, fechaEntrega);
            var resultado = new List<PedidoDto>();
            foreach (var pedido in pedidos)
            {
                resultado.Add(await MapearDtoConPagoAsync(pedido));
            }

            return resultado;
        }

        public async Task<PedidoDto> CambiarEstadoAsync(Guid idPedido, Guid idUsuarioRegistro, CambiarEstadoPedidoRequest request)
        {
            var pedido = await _pedidoRepository.ObtenerPorIdAsync(idPedido)
                ?? throw new NoEncontradoException($"No se encontró el pedido con id {idPedido}.");

            if (!TransicionesValidas.TryGetValue(pedido.Estado, out var siguienteEstado) ||
                siguienteEstado != request.NuevoEstado)
            {
                throw new ReglaNegocioException(
                    $"No se puede cambiar el pedido de '{pedido.Estado}' a '{request.NuevoEstado}'.");
            }

            pedido.Estado = request.NuevoEstado;

            // Inventario automático de personalizables (pedido del usuario, 2026-07-18): entrar
            // a producción descuenta lo que el cliente eligió — bizcocho según masa y porciones
            // (del stock horneado en San Mateo) + crema/relleno/colorante. La transición
            // Confirmado→En Producción solo puede ocurrir UNA vez (TransicionesValidas), así que
            // no hay riesgo de doble descuento. Si falta stock, la excepción corta antes de
            // guardar: el pedido sigue 'Confirmado' y el error dice qué comprar/hornear.
            var config = pedido.Configuracion;
            if (request.NuevoEstado == "En Producción" &&
                config is { NumeroPorciones: > 0, TipoMasa: not null, IdInsumoCrema: not null })
            {
                await _consumoService.DescontarPorPedidoPersonalizableAsync(
                    pedido.Id,
                    $"{pedido.CodigoQrReferencia} — {config.SaborMasa} {config.NumeroPorciones}p de {pedido.NombreCliente}",
                    config.NumeroPorciones.Value,
                    config.TipoMasa,
                    config.IdInsumoCrema.Value,
                    config.IdInsumoRelleno,
                    config.IdInsumoColorDecoracion,
                    idUsuarioRegistro);
            }

            await _pedidoRepository.GuardarCambiosAsync();

            return await MapearDtoConPagoAsync(pedido);
        }

        public async Task<PedidoDto> CancelarAsync(Guid idPedido, CancelarPedidoRequest request)
        {
            var pedido = await _pedidoRepository.ObtenerPorIdAsync(idPedido)
                ?? throw new NoEncontradoException($"No se encontró el pedido con id {idPedido}.");

            if (pedido.Estado is "Entregado" or "Cancelado")
            {
                throw new ReglaNegocioException($"No se puede cancelar un pedido en estado '{pedido.Estado}'.");
            }

            if (string.IsNullOrWhiteSpace(request.Motivo))
            {
                throw new ReglaNegocioException("El motivo de cancelación es obligatorio.");
            }

            pedido.Estado = "Cancelado";
            pedido.Observaciones = request.Motivo.Trim();
            await _pedidoRepository.GuardarCambiosAsync();

            return await MapearDtoConPagoAsync(pedido);
        }

        private async Task<ItemCatalogo> ValidarInsumoAsync(Guid idInsumo, string? categoriaEsperada = null)
        {
            var insumo = await _itemCatalogoRepository.ObtenerPorIdAsync(idInsumo)
                ?? throw new NoEncontradoException($"No se encontró el insumo con id {idInsumo}.");

            if (insumo.Tipo != "MateriaPrima" || !insumo.Activo)
            {
                throw new ReglaNegocioException($"'{insumo.Nombre}' no es un insumo válido para personalizar el pedido.");
            }

            if (categoriaEsperada is not null && insumo.CategoriaPersonalizacion != categoriaEsperada)
            {
                throw new ReglaNegocioException($"'{insumo.Nombre}' no es un insumo de tipo '{categoriaEsperada}'.");
            }

            // El catálogo público (ObtenerInsumosPersonalizacionAsync) ya solo lista insumos con
            // stock disponible, así que este chequeo es una red de seguridad ante condiciones de
            // carrera (stock que se agotó entre que el cliente cargó la página y confirmó el pedido).
            var stockActual = await _loteRepository.ObtenerStockDisponibleTotalAsync(insumo.Id);
            if (stockActual <= 0)
            {
                throw new ReglaNegocioException($"'{insumo.Nombre}' no tiene stock disponible en este momento.");
            }

            return insumo;
        }

        private async Task<PedidoDto> MapearDtoConPagoAsync(PedidoCliente pedido)
        {
            var pagoActual = await _pagoQrRepository.ObtenerMasRecientePorPedidoAsync(pedido.Id);
            return MapearDto(pedido, pagoActual is null ? null : PagoQrService.MapearDto(pagoActual));
        }

        private static bool EsFechaEntregaValida(DateTime fecha)
        {
            var minima = DateTime.UtcNow.Date;
            var diasHabiles = 0;
            while (diasHabiles < 2)
            {
                minima = minima.AddDays(1);
                if (minima.DayOfWeek != DayOfWeek.Saturday && minima.DayOfWeek != DayOfWeek.Sunday)
                {
                    diasHabiles++;
                }
            }

            return fecha.Date >= minima;
        }

        private static PedidoDto MapearDto(PedidoCliente pedido, PagoQrDto? pagoActual) => new()
        {
            Id = pedido.Id,
            NombreCliente = pedido.NombreCliente,
            WhatsApp = pedido.WhatsApp,
            FechaSolicitud = pedido.FechaSolicitud,
            FechaEntregaSolicitada = pedido.FechaEntregaSolicitada,
            TotalCotizado = pedido.TotalCotizado,
            Estado = pedido.Estado,
            CodigoQrReferencia = pedido.CodigoQrReferencia,
            Observaciones = pedido.Observaciones,
            IdUsuarioVendedora = pedido.IdUsuarioVendedora,
            Configuracion = pedido.Configuracion is null
                ? new ConfiguracionPedidoDto()
                : new ConfiguracionPedidoDto
                {
                    IdItemProductoBase = pedido.Configuracion.IdItemProductoBase,
                    IdInsumoSaborMasa = pedido.Configuracion.IdInsumoSaborMasa,
                    IdInsumoRelleno = pedido.Configuracion.IdInsumoRelleno,
                    SaborMasa = pedido.Configuracion.SaborMasa,
                    TipoRelleno = pedido.Configuracion.TipoRelleno,
                    TamanoRacion = pedido.Configuracion.TamanoRacion,
                    ColorDecoracion = pedido.Configuracion.ColorDecoracion,
                    DedicatoriaDetalle = pedido.Configuracion.DedicatoriaDetalle,
                    ImagenReferenciaUrl = pedido.Configuracion.ImagenReferenciaUrl,
                    PorcentajeAnticipo = pedido.Configuracion.PorcentajeAnticipo,
                    NumeroPorciones = pedido.Configuracion.NumeroPorciones,
                    TipoMasa = pedido.Configuracion.TipoMasa,
                    TipoCrema = pedido.Configuracion.TipoCrema,
                    IdInsumoCrema = pedido.Configuracion.IdInsumoCrema,
                    IdInsumoColorDecoracion = pedido.Configuracion.IdInsumoColorDecoracion,
                    CantidadVelasNormales = pedido.Configuracion.CantidadVelasNormales,
                    VelaNumerica = pedido.Configuracion.VelaNumerica
                },
            PagoQrActual = pagoActual
        };
    }
}
