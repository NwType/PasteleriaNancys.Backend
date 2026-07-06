using PasteleriaNancys.Application.Inventario.Dtos;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IDespachoService
    {
        Task<ViajeDto> CrearViajeAsync(CrearViajeRequest request);
        Task<List<ViajeDto>> ObtenerTodosAsync();
        Task<ViajeDto> ObtenerPorIdAsync(Guid id);
        Task<ViajeDto> AgregarLoteAsync(Guid idViaje, AgregarLoteAlViajeRequest request);
        Task<ViajeDto> ConfirmarEntregaAsync(Guid idViaje);
    }
}
