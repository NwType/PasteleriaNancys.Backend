using PasteleriaNancys.Application.Inventario.Dtos;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    // Solo lectura: DS-07. El bloqueo efectivo de pedidos queda diferido hasta que
    // el módulo Pedidos/Web esté reconciliado con la base de datos real.
    public interface IAlertaService
    {
        Task<List<InsumoCriticoDto>> ConsultarInsumosCriticosAsync();
        Task<List<ProductoAfectadoDto>> ConsultarProductosAfectadosAsync(Guid idInsumo);
    }
}
