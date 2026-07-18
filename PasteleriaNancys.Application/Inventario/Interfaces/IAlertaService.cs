using PasteleriaNancys.Application.Inventario.Dtos;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IAlertaService
    {
        Task<List<InsumoCriticoDto>> ConsultarInsumosCriticosAsync();
        Task<List<ProductoAfectadoDto>> ConsultarProductosAfectadosAsync(Guid idInsumo);

        // DS-07: pedidos web pendientes que quedan en riesgo por un insumo crítico
        // (ya sea porque su producto base lo requiere via Receta_Item, o porque el
        // cliente lo eligió directamente como sabor/relleno personalizado).
        Task<List<PedidoAfectadoDto>> ConsultarPedidosAfectadosAsync(Guid idInsumo);

        // Panel consolidado de DS-07: insumos críticos con sus productos y pedidos
        // afectados ya anidados, en una sola llamada.
        Task<List<InsumoCriticoDto>> ConsultarPanelAsync();
    }
}
