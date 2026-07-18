using PasteleriaNancys.Domain.Inventario;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IConsumoInsumoRepository
    {
        Task<List<ConsumoInsumo>> ObtenerTodosAsync();
        Task AgregarAsync(ConsumoInsumo consumo);
        Task GuardarCambiosAsync();
    }
}
