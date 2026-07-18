using PasteleriaNancys.Application.Administracion.Dtos;
using PasteleriaNancys.Application.Administracion.Interfaces;
using PasteleriaNancys.Application.Caja.Dtos;
using PasteleriaNancys.Application.Caja.Interfaces;
using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Application.Inventario.Interfaces;
using PasteleriaNancys.Application.Pedidos.Interfaces;
using PasteleriaNancys.Application.Seguridad.Interfaces;
using PasteleriaNancys.Domain.Caja;
using PasteleriaNancys.Domain.Seguridad;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PasteleriaNancys.Application.Administracion.Services
{
    public class AdministracionService : IAdministracionService
    {
        private const decimal UmbralDiferenciaPorcentaje = 0.05m;
        private static readonly string[] PeriodosValidos = { "dia", "semana", "mes" };

        // Bolivia es UTC-4 todo el año (sin horario de verano). FechaHora/FechaApertura etc. se
        // guardan en UTC; "hoy"/"esta semana"/"este mes" deben calcularse sobre el día calendario
        // local de la pastelería, no sobre la medianoche UTC (que cae a las 8pm hora local).
        private const int OffsetBoliviaHoras = -4;

        private static DateTime AhoraLocalBolivia() => DateTime.UtcNow.AddHours(OffsetBoliviaHoras);

        private static DateTime LocalAUtc(DateTime local) => local.AddHours(-OffsetBoliviaHoras);

        private readonly ITurnoRepository _turnoRepository;
        private readonly IVentaRepository _ventaRepository;
        private readonly ITurnoService _turnoService;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IAlertaService _alertaService;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IItemCatalogoRepository _itemCatalogoRepository;

        public AdministracionService(
            ITurnoRepository turnoRepository,
            IVentaRepository ventaRepository,
            ITurnoService turnoService,
            IPedidoRepository pedidoRepository,
            IAlertaService alertaService,
            IUsuarioRepository usuarioRepository,
            IItemCatalogoRepository itemCatalogoRepository)
        {
            _turnoRepository = turnoRepository;
            _ventaRepository = ventaRepository;
            _turnoService = turnoService;
            _pedidoRepository = pedidoRepository;
            _alertaService = alertaService;
            _usuarioRepository = usuarioRepository;
            _itemCatalogoRepository = itemCatalogoRepository;
        }

        public async Task<List<ArqueoAuditoriaDto>> ConsultarArqueosAsync(
            DateTime? desde, DateTime? hasta, Guid? idUsuarioResponsable, bool? soloDiferenciasSignificativas)
        {
            var turnos = await _turnoRepository.ObtenerCerradosAsync(desde, hasta, idUsuarioResponsable);
            var usuarios = await ObtenerMapaUsuariosAsync();

            var resultado = turnos.Select(t => MapearArqueoDto(t, usuarios)).ToList();

            if (soloDiferenciasSignificativas == true)
            {
                resultado = resultado.Where(a => a.DiferenciaSignificativa).ToList();
            }

            return resultado;
        }

        public async Task<ArqueoAuditoriaDetalleDto> ConsultarArqueoDetalleAsync(Guid idTurno)
        {
            var turno = await _turnoRepository.ObtenerPorIdAsync(idTurno)
                ?? throw new NoEncontradoException($"No se encontró el turno con id {idTurno}.");

            var usuarios = await ObtenerMapaUsuariosAsync();
            var historial = await _turnoService.ConsultarHistorialAsync(idTurno);

            var totalIngresosCalculado = historial.Ventas.Where(v => !v.Anulada).Sum(v => v.TotalPagado);
            var totalGastosCalculado = historial.Gastos.Sum(g => g.Monto);
            var driftDetectado = totalIngresosCalculado != turno.TotalIngresosSistema
                || totalGastosCalculado != turno.TotalGastosExtras;

            return new ArqueoAuditoriaDetalleDto
            {
                Arqueo = MapearArqueoDto(turno, usuarios),
                Ventas = historial.Ventas,
                Gastos = historial.Gastos,
                TotalIngresosCalculado = totalIngresosCalculado,
                TotalGastosCalculado = totalGastosCalculado,
                DriftDetectado = driftDetectado
            };
        }

        public async Task<DashboardDto> ObtenerDashboardAsync()
        {
            var hoyLocal = AhoraLocalBolivia().Date;
            var hoy = LocalAUtc(hoyLocal);
            var manana = LocalAUtc(hoyLocal.AddDays(1));

            var ventasHoy = await _ventaRepository.ObtenerPorRangoAsync(hoy, manana);
            var pedidosPendientes = await _pedidoRepository.ObtenerPendientesAsync();
            var turnosAbiertos = await _turnoRepository.ObtenerAbiertosAsync();
            var insumosCriticos = await _alertaService.ConsultarInsumosCriticosAsync();
            var usuarios = await ObtenerMapaUsuariosAsync();

            return new DashboardDto
            {
                TotalVentasHoy = ventasHoy.Sum(v => v.TotalPagado),
                NumeroVentasHoy = ventasHoy.Count,
                PedidosPendientes = pedidosPendientes.Count,
                TurnosActivos = turnosAbiertos.Select(t => MapearTurnoActivoDto(t, usuarios)).ToList(),
                InsumosCriticosActivos = insumosCriticos.Count,
                SinActividadHoy = ventasHoy.Count == 0
            };
        }

        public async Task<ReporteVentasDto> ConsultarReporteVentasAsync(string periodo, DateTime? fechaReferencia)
        {
            var (desdeUtc, hastaUtc) = CalcularRangoPeriodo(periodo, fechaReferencia);
            var ventas = await _ventaRepository.ObtenerPorRangoAsync(desdeUtc, hastaUtc);
            var usuarios = await ObtenerMapaUsuariosAsync();
            var items = (await _itemCatalogoRepository.ObtenerTodosAsync()).ToDictionary(i => i.Id, i => i.Nombre);

            // Se muestran las fechas en hora local (el rango de consulta ya está en UTC internamente).
            var desdeLocal = desdeUtc.AddHours(OffsetBoliviaHoras);
            var hastaLocal = hastaUtc.AddHours(OffsetBoliviaHoras);

            return ConstruirReporte(periodo, desdeLocal, hastaLocal, ventas, usuarios, items);
        }

        public async Task<byte[]> GenerarReporteVentasPdfAsync(string periodo, DateTime? fechaReferencia)
        {
            var reporte = await ConsultarReporteVentasAsync(periodo, fechaReferencia);
            return GenerarPdf(reporte);
        }

        private static ArqueoAuditoriaDto MapearArqueoDto(Turno turno, Dictionary<Guid, Usuario> usuarios)
        {
            var totalEsperado = turno.SaldoInicial + turno.TotalIngresosSistema - turno.TotalGastosExtras;
            var diferenciaSignificativa = turno.DiferenciaArqueo.HasValue &&
                Math.Abs(turno.DiferenciaArqueo.Value) > Math.Abs(totalEsperado) * UmbralDiferenciaPorcentaje;

            usuarios.TryGetValue(turno.IdUsuarioResponsable, out var usuario);

            return new ArqueoAuditoriaDto
            {
                IdTurno = turno.Id,
                IdUsuarioResponsable = turno.IdUsuarioResponsable,
                NombreResponsable = usuario?.NombreCompleto ?? "(usuario no encontrado)",
                CorreoResponsable = usuario?.Correo ?? string.Empty,
                FechaApertura = turno.FechaApertura,
                FechaCierre = turno.FechaCierre,
                SaldoInicial = turno.SaldoInicial,
                TotalIngresosSistema = turno.TotalIngresosSistema,
                TotalGastosExtras = turno.TotalGastosExtras,
                TotalEsperado = totalEsperado,
                MontoFisicoContado = turno.MontoFisicoContado,
                DiferenciaArqueo = turno.DiferenciaArqueo,
                DiferenciaSignificativa = diferenciaSignificativa,
                Estado = turno.Estado
            };
        }

        private static TurnoActivoDto MapearTurnoActivoDto(Turno turno, Dictionary<Guid, Usuario> usuarios)
        {
            usuarios.TryGetValue(turno.IdUsuarioResponsable, out var usuario);

            return new TurnoActivoDto
            {
                IdTurno = turno.Id,
                IdUsuarioResponsable = turno.IdUsuarioResponsable,
                NombreResponsable = usuario?.NombreCompleto ?? "(usuario no encontrado)",
                FechaApertura = turno.FechaApertura,
                SaldoInicial = turno.SaldoInicial,
                TotalIngresosSistema = turno.TotalIngresosSistema,
                TotalGastosExtras = turno.TotalGastosExtras,
                TotalEsperadoParcial = turno.SaldoInicial + turno.TotalIngresosSistema - turno.TotalGastosExtras
            };
        }

        private async Task<Dictionary<Guid, Usuario>> ObtenerMapaUsuariosAsync() =>
            (await _usuarioRepository.ObtenerTodosAsync()).ToDictionary(u => u.Id);

        private static (DateTime Desde, DateTime Hasta) CalcularRangoPeriodo(string periodo, DateTime? fechaReferencia)
        {
            var periodoNormalizado = periodo.Trim().ToLowerInvariant();
            if (!PeriodosValidos.Contains(periodoNormalizado))
            {
                throw new ReglaNegocioException("Periodo inválido. Use 'dia', 'semana' o 'mes'.");
            }

            var referenciaLocal = (fechaReferencia ?? AhoraLocalBolivia()).Date;

            var (desdeLocal, hastaLocal) = periodoNormalizado switch
            {
                "dia" => (referenciaLocal, referenciaLocal.AddDays(1)),
                "semana" => (referenciaLocal.AddDays(-6), referenciaLocal.AddDays(1)),
                "mes" => (new DateTime(referenciaLocal.Year, referenciaLocal.Month, 1),
                          new DateTime(referenciaLocal.Year, referenciaLocal.Month, 1).AddMonths(1)),
                _ => throw new ReglaNegocioException("Periodo inválido. Use 'dia', 'semana' o 'mes'.")
            };

            return (LocalAUtc(desdeLocal), LocalAUtc(hastaLocal));
        }

        private static ReporteVentasDto ConstruirReporte(
            string periodo,
            DateTime desde,
            DateTime hasta,
            List<VentaPos> ventas,
            Dictionary<Guid, Usuario> usuarios,
            Dictionary<Guid, string> items)
        {
            var ventasPorProducto = ventas
                .SelectMany(v => v.Detalles)
                .GroupBy(d => d.IdItem)
                .Select(g => new VentaPorProductoDto
                {
                    IdItem = g.Key,
                    Nombre = items.TryGetValue(g.Key, out var nombre) ? nombre : "(producto no encontrado)",
                    CantidadVendida = g.Sum(d => d.Cantidad),
                    TotalVendido = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(p => p.TotalVendido)
                .ToList();

            var ventasPorMetodoPago = ventas
                .GroupBy(v => v.MetodoPago)
                .Select(g => new VentaPorMetodoPagoDto
                {
                    MetodoPago = g.Key,
                    NumeroVentas = g.Count(),
                    TotalVendido = g.Sum(v => v.TotalPagado)
                })
                .OrderByDescending(p => p.TotalVendido)
                .ToList();

            var ventasPorVendedora = ventas
                .GroupBy(v => v.Turno.IdUsuarioResponsable)
                .Select(g =>
                {
                    usuarios.TryGetValue(g.Key, out var usuario);
                    return new VentaPorVendedoraDto
                    {
                        IdUsuario = g.Key,
                        NombreVendedora = usuario?.NombreCompleto ?? "(usuario no encontrado)",
                        NumeroVentas = g.Count(),
                        TotalVendido = g.Sum(v => v.TotalPagado)
                    };
                })
                .OrderByDescending(p => p.TotalVendido)
                .ToList();

            return new ReporteVentasDto
            {
                Periodo = periodo,
                Desde = desde,
                Hasta = hasta,
                TotalVentas = ventas.Sum(v => v.TotalPagado),
                NumeroVentas = ventas.Count,
                SinVentas = ventas.Count == 0,
                VentasPorProducto = ventasPorProducto,
                VentasPorMetodoPago = ventasPorMetodoPago,
                VentasPorVendedora = ventasPorVendedora
            };
        }

        private static byte[] GenerarPdf(ReporteVentasDto reporte)
        {
            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(column =>
                    {
                        column.Item().Text("Pastelería Nancy's - Reporte de Ventas").FontSize(16).Bold();
                        column.Item().Text($"Período: {reporte.Periodo} ({reporte.Desde:dd/MM/yyyy} - {reporte.Hasta.AddDays(-1):dd/MM/yyyy})");
                    });

                    page.Content().Column(column =>
                    {
                        column.Spacing(10);

                        column.Item().Text($"Total de ventas: Bs. {reporte.TotalVentas:N2}");
                        column.Item().Text($"Número de ventas: {reporte.NumeroVentas}");

                        if (reporte.SinVentas)
                        {
                            column.Item().Text("Sin ventas en el período seleccionado.").Italic();
                            return;
                        }

                        column.Item().PaddingTop(10).Text("Ventas por producto").Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Producto").Bold();
                                header.Cell().Text("Cantidad").Bold();
                                header.Cell().Text("Total (Bs.)").Bold();
                            });

                            foreach (var producto in reporte.VentasPorProducto)
                            {
                                table.Cell().Text(producto.Nombre);
                                table.Cell().Text(producto.CantidadVendida.ToString("N2"));
                                table.Cell().Text(producto.TotalVendido.ToString("N2"));
                            }
                        });

                        column.Item().PaddingTop(10).Text("Ventas por método de pago").Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Método de pago").Bold();
                                header.Cell().Text("N° ventas").Bold();
                                header.Cell().Text("Total (Bs.)").Bold();
                            });

                            foreach (var metodo in reporte.VentasPorMetodoPago)
                            {
                                table.Cell().Text(metodo.MetodoPago);
                                table.Cell().Text(metodo.NumeroVentas.ToString());
                                table.Cell().Text(metodo.TotalVendido.ToString("N2"));
                            }
                        });

                        column.Item().PaddingTop(10).Text("Ventas por vendedora").Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Vendedora").Bold();
                                header.Cell().Text("N° ventas").Bold();
                                header.Cell().Text("Total (Bs.)").Bold();
                            });

                            foreach (var vendedora in reporte.VentasPorVendedora)
                            {
                                table.Cell().Text(vendedora.NombreVendedora);
                                table.Cell().Text(vendedora.NumeroVentas.ToString());
                                table.Cell().Text(vendedora.TotalVendido.ToString("N2"));
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Generado el ");
                        text.Span(DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")).Bold();
                    });
                });
            }).GeneratePdf();
        }
    }
}
