using PasteleriaNancys.Application.Inventario.Dtos;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface ILoteService
    {
        Task<LoteDto> RegistrarAsync(RegistrarLoteRequest request);
        Task<List<LoteDto>> ObtenerTodosAsync();
        Task<LoteDto> ObtenerPorIdAsync(Guid id);
        Task<LoteDto> ActualizarEstadoAsync(Guid id, ActualizarEstadoLoteRequest request);
        Task<List<LoteProximoACaducarDto>> ConsultarProximosACaducarAsync();
    }
}
