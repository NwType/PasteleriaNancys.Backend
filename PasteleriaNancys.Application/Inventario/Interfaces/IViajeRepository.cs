using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IViajeRepository
    {
        Task<ViajeDespacho?> ObtenerPorIdAsync(Guid id);
        Task<List<ViajeDespacho>> ObtenerTodosAsync();
        Task AgregarAsync(ViajeDespacho viaje);
        Task AgregarDetalleAsync(ViajeDetalle detalle);
        Task GuardarCambiosAsync();
    }
}
