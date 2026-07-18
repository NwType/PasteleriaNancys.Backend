using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IHorneadaRepository
    {
        Task<List<Horneada>> ObtenerTodosAsync();
        Task AgregarAsync(Horneada horneada);
        Task GuardarCambiosAsync();
    }
}
