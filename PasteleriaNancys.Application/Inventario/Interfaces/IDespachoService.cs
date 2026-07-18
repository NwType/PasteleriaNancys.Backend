using PasteleriaNancys.Application.Inventario.Dtos;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IDespachoService
    {
        Task<ViajeDto> CrearViajeAsync(CrearViajeRequest request);
        Task<List<ViajeDto>> ObtenerTodosAsync();
        Task<ViajeDto> ObtenerPorIdAsync(Guid id);
        Task<ViajeDto> AgregarProductoAsync(Guid idViaje, AgregarProductoAlViajeRequest request);
        Task<ViajeDto> ConfirmarEntregaAsync(Guid idViaje);
    }
}
