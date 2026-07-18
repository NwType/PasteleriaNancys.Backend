using PasteleriaNancys.Application.Inventario.Dtos;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IConsumoService
    {
        Task<HorneadaDto> RegistrarHorneadaAsync(Guid idUsuarioRegistro, RegistrarHorneadaRequest request);
        Task<List<HorneadaDto>> ObtenerHorneadasAsync();
        Task<ConsumoInsumoDto> RegistrarConsumoManualAsync(Guid idUsuarioRegistro, RegistrarConsumoRequest request);
        Task<List<ConsumoInsumoDto>> ObtenerConsumosManualesAsync();
    }
}
