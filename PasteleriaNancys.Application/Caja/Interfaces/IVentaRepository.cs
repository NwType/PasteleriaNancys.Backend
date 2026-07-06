using PasteleriaNancys.Domain.Caja;

namespace PasteleriaNancys.Application.Caja.Interfaces
{
    public interface IVentaRepository
    {
        Task<VentaPos?> ObtenerPorIdAsync(Guid id);
        Task<List<VentaPos>> ObtenerPorTurnoAsync(Guid idTurno);
        Task AgregarAsync(VentaPos venta);
        Task GuardarCambiosAsync();
    }
}
