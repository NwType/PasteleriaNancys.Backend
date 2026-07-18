using PasteleriaNancys.Application.Inventario.Dtos;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IRecetaService
    {
        Task<RecetaItemDto> CrearAsync(CrearRecetaItemRequest request);
        Task<List<RecetaItemDto>> CrearVariasAsync(CrearRecetaMultipleRequest request);
        Task<List<RecetaItemDto>> ObtenerPorProductoTerminadoAsync(Guid idItemTerminado);
        Task EliminarAsync(Guid id);
    }
}
