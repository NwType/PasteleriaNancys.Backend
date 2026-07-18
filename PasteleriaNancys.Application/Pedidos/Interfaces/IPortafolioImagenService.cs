using PasteleriaNancys.Application.Pedidos.Dtos;

namespace PasteleriaNancys.Application.Pedidos.Interfaces
{
    public interface IPortafolioImagenService
    {
        Task<List<PortafolioImagenDto>> ObtenerPorCategoriaAsync(string categoria);
        Task<List<PortafolioImagenDto>> ObtenerTodasAsync();
        Task<PortafolioImagenDto> AgregarAsync(string categoria, string? descripcion, string imagenUrl);
        Task DesactivarAsync(Guid id);
    }
}
