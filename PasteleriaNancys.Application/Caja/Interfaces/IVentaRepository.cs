using PasteleriaNancys.Domain.Caja;

namespace PasteleriaNancys.Application.Caja.Interfaces
{
    public interface IVentaRepository
    {
        Task<VentaPos?> ObtenerPorIdAsync(Guid id);
        Task<List<VentaPos>> ObtenerPorTurnoAsync(Guid idTurno);
        Task<decimal> ObtenerCantidadVendidaAsync(Guid idItem, DateTime desde, DateTime hasta);
        Task<List<VentaPos>> ObtenerPorRangoAsync(DateTime desde, DateTime hasta);
        Task AgregarAsync(VentaPos venta);
        Task GuardarCambiosAsync();
    }
}
