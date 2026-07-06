using PasteleriaNancys.Application.Caja.Dtos;

namespace PasteleriaNancys.Application.Caja.Interfaces
{
    public interface IGastoService
    {
        Task<GastoDto> RegistrarAsync(Guid idTurno, RegistrarGastoRequest request);
    }
}
