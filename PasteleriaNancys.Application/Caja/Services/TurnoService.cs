using PasteleriaNancys.Application.Caja.Dtos;
using PasteleriaNancys.Application.Caja.Interfaces;
using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Domain.Caja;

namespace PasteleriaNancys.Application.Caja.Services
{
    public class TurnoService : ITurnoService
    {
        private const decimal UmbralDiferenciaPorcentaje = 0.05m;

        private readonly ITurnoRepository _turnoRepository;
        private readonly IVentaRepository _ventaRepository;
        private readonly IGastoRepository _gastoRepository;

        public TurnoService(
            ITurnoRepository turnoRepository,
            IVentaRepository ventaRepository,
            IGastoRepository gastoRepository)
        {
            _turnoRepository = turnoRepository;
            _ventaRepository = ventaRepository;
            _gastoRepository = gastoRepository;
        }

        public async Task<TurnoDto> AperturarAsync(Guid idUsuarioResponsable, AperturarTurnoRequest request)
        {
            var turnoAbierto = await _turnoRepository.ObtenerAbiertoPorUsuarioAsync(idUsuarioResponsable);
            if (turnoAbierto is not null)
            {
                throw new ConflictoException("Ya existe un turno activo. Ciérrelo antes de abrir uno nuevo.");
            }

            if (request.SaldoInicial < 0)
            {
                throw new ReglaNegocioException("El saldo inicial no puede ser negativo.");
            }

            var turno = new Turno
            {
                Id = Guid.NewGuid(),
                IdUsuarioResponsable = idUsuarioResponsable,
                FechaApertura = DateTime.UtcNow,
                SaldoInicial = request.SaldoInicial,
                TotalIngresosSistema = 0,
                TotalGastosExtras = 0,
                Estado = "Abierto"
            };

            await _turnoRepository.AgregarAsync(turno);
            await _turnoRepository.GuardarCambiosAsync();

            return MapearDto(turno);
        }

        public async Task<ResumenTurnoDto> ObtenerResumenAsync(Guid idTurno)
        {
            var turno = await _turnoRepository.ObtenerPorIdAsync(idTurno)
                ?? throw new NoEncontradoException($"No se encontró el turno con id {idTurno}.");

            return new ResumenTurnoDto
            {
                IdTurno = turno.Id,
                SaldoInicial = turno.SaldoInicial,
                TotalIngresosSistema = turno.TotalIngresosSistema,
                TotalGastosExtras = turno.TotalGastosExtras,
                TotalEsperado = CalcularTotalEsperado(turno),
                Estado = turno.Estado
            };
        }

        public async Task<CierreTurnoDto> CerrarAsync(Guid idTurno, CerrarTurnoRequest request)
        {
            var turno = await _turnoRepository.ObtenerPorIdAsync(idTurno)
                ?? throw new NoEncontradoException($"No se encontró el turno con id {idTurno}.");

            if (turno.Estado != "Abierto")
            {
                throw new ReglaNegocioException("El turno ya se encuentra cerrado.");
            }

            var totalEsperado = CalcularTotalEsperado(turno);
            var diferencia = request.MontoFisicoContado - totalEsperado;

            turno.Estado = "Cerrado";
            turno.FechaCierre = DateTime.UtcNow;
            turno.DiferenciaArqueo = diferencia;

            await _turnoRepository.GuardarCambiosAsync();

            var diferenciaSignificativa = Math.Abs(diferencia) > Math.Abs(totalEsperado) * UmbralDiferenciaPorcentaje;

            return new CierreTurnoDto
            {
                IdTurno = turno.Id,
                TotalEsperado = totalEsperado,
                MontoFisicoContado = request.MontoFisicoContado,
                DiferenciaArqueo = diferencia,
                DiferenciaSignificativa = diferenciaSignificativa,
                Estado = turno.Estado,
                FechaCierre = turno.FechaCierre!.Value
            };
        }

        public async Task<HistorialTurnoDto> ConsultarHistorialAsync(Guid idTurno)
        {
            var turno = await _turnoRepository.ObtenerPorIdAsync(idTurno)
                ?? throw new NoEncontradoException($"No se encontró el turno con id {idTurno}.");

            var ventas = await _ventaRepository.ObtenerPorTurnoAsync(idTurno);
            var gastos = await _gastoRepository.ObtenerPorTurnoAsync(idTurno);

            return new HistorialTurnoDto
            {
                IdTurno = turno.Id,
                TotalIngresos = turno.TotalIngresosSistema,
                TotalGastos = turno.TotalGastosExtras,
                Ventas = ventas.Select(v => new VentaDto
                {
                    Id = v.Id,
                    IdTurno = v.IdTurno,
                    FechaHora = v.FechaHora,
                    TotalPagado = v.TotalPagado,
                    MetodoPago = v.MetodoPago,
                    Anulada = v.Anulada,
                    MotivoAnulacion = v.MotivoAnulacion,
                    Detalles = v.Detalles.Select(d => new VentaDetalleDto
                    {
                        Id = d.Id,
                        IdItem = d.IdItem,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal
                    }).ToList()
                }).ToList(),
                Gastos = gastos.Select(g => new GastoDto
                {
                    Id = g.Id,
                    IdTurno = g.IdTurno,
                    Concepto = g.Concepto,
                    Monto = g.Monto,
                    FechaHora = g.FechaHora
                }).ToList()
            };
        }

        private static decimal CalcularTotalEsperado(Turno turno) =>
            turno.SaldoInicial + turno.TotalIngresosSistema - turno.TotalGastosExtras;

        private static TurnoDto MapearDto(Turno turno) => new()
        {
            Id = turno.Id,
            IdUsuarioResponsable = turno.IdUsuarioResponsable,
            FechaApertura = turno.FechaApertura,
            FechaCierre = turno.FechaCierre,
            SaldoInicial = turno.SaldoInicial,
            TotalIngresosSistema = turno.TotalIngresosSistema,
            TotalGastosExtras = turno.TotalGastosExtras,
            DiferenciaArqueo = turno.DiferenciaArqueo,
            Estado = turno.Estado
        };
    }
}
