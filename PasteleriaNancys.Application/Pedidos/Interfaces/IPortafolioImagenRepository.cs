using PasteleriaNancys.Domain.Pedidos;

namespace PasteleriaNancys.Application.Pedidos.Interfaces
{
    public interface IPortafolioImagenRepository
    {
        Task<PortafolioImagen?> ObtenerPorIdAsync(Guid id);
        Task<List<PortafolioImagen>> ObtenerPorCategoriaAsync(string categoria);
        Task<List<PortafolioImagen>> ObtenerTodasAsync();
        Task AgregarAsync(PortafolioImagen imagen);
        Task GuardarCambiosAsync();
    }
}
