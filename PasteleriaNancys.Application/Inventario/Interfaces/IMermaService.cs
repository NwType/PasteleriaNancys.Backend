using PasteleriaNancys.Application.Inventario.Dtos;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IMermaService
    {
        Task<List<MermaDto>> RegistrarAsync(Guid idUsuarioRegistro, RegistrarMermaRequest request);
        Task<List<MermaDto>> RegistrarProduccionFallidaAsync(Guid idUsuarioRegistro, RegistrarMermaProduccionRequest request);
        Task<List<MermaDto>> ObtenerTodasAsync();
    }
}
