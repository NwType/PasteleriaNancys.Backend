using PasteleriaNancys.Application.Caja.Dtos;
using PasteleriaNancys.Application.Caja.Interfaces;
using PasteleriaNancys.Application.Common.Exceptions;
using PasteleriaNancys.Domain.Caja;

namespace PasteleriaNancys.Application.Caja.Services
{
    public class GastoService : IGastoService
    {
        private readonly IGastoRepository _gastoRepository;
        private readonly ITurnoRepository _turnoRepository;

        public GastoService(IGastoRepository gastoRepository, ITurnoRepository turnoRepository)
        {
            _gastoRepository = gastoRepository;
            _turnoRepository = turnoRepository;
        }

        public async Task<GastoDto> RegistrarAsync(Guid idTurno, RegistrarGastoRequest request)
        {
            var turno = await _turnoRepository.ObtenerPorIdAsync(idTurno)
                ?? throw new NoEncontradoException($"No se encontró el turno con id {idTurno}.");

            if (turno.Estado != "Abierto")
            {
                throw new ReglaNegocioException("Debe existir un turno abierto para registrar gastos.");
            }

            if (request.Monto <= 0)
            {
                throw new ReglaNegocioException("El monto ingresado no es válido.");
            }

            var gasto = new GastoExtra
            {
                Id = Guid.NewGuid(),
                IdTurno = idTurno,
                Concepto = request.Concepto.Trim(),
                Monto = request.Monto,
                FechaHora = DateTime.UtcNow
            };

            turno.TotalGastosExtras += request.Monto;

            await _gastoRepository.AgregarAsync(gasto);
            await _gastoRepository.GuardarCambiosAsync();

            return MapearDto(gasto);
        }

        private static GastoDto MapearDto(GastoExtra gasto) => new()
        {
            Id = gasto.Id,
            IdTurno = gasto.IdTurno,
            Concepto = gasto.Concepto,
            Monto = gasto.Monto,
            FechaHora = gasto.FechaHora
        };
    }
}
