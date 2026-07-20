using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IMermaRepository
    {
        Task<List<Merma>> ObtenerTodasAsync();
        Task AgregarAsync(Merma merma);
        Task GuardarCambiosAsync();
    }
}
