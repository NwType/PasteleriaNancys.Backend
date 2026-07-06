using PasteleriaNancys.Domain.Caja;

namespace PasteleriaNancys.Application.Caja.Interfaces
{
    public interface IGastoRepository
    {
        Task<List<GastoExtra>> ObtenerPorTurnoAsync(Guid idTurno);
        Task AgregarAsync(GastoExtra gasto);
        Task GuardarCambiosAsync();
    }
}
