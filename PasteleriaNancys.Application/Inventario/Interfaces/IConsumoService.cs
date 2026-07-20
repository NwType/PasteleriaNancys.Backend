using PasteleriaNancys.Application.Inventario.Dtos;

namespace PasteleriaNancys.Application.Inventario.Interfaces
{
    public interface IConsumoService
    {
        Task<HorneadaDto> RegistrarHorneadaAsync(Guid idUsuarioRegistro, RegistrarHorneadaRequest request);
        Task<List<HorneadaDto>> ObtenerHorneadasAsync();
        Task<ConsumoInsumoDto> RegistrarConsumoManualAsync(Guid idUsuarioRegistro, RegistrarConsumoRequest request);
        Task<List<ConsumoInsumoDto>> ObtenerConsumosManualesAsync();

        // Descuento automático por receta al producir un lote (no guarda cambios — el llamador
        // persiste todo junto). Ver ConsumoService.DescontarPorRecetaAsync.
        Task DescontarPorRecetaAsync(Guid idItemProducido, decimal cantidad, Guid idLoteProducido, Guid idUsuarioRegistro);

        // Descuento automático al producir una torta PERSONALIZABLE (2026-07-18): no hay
        // Receta_Item fija — los componentes vienen de lo que el cliente eligió en el pedido
        // (masa/porciones/crema/relleno/colorante). No guarda cambios; el llamador persiste todo
        // junto. Lanza ReglaNegocioException si algún componente no alcanza.
        Task DescontarPorPedidoPersonalizableAsync(
            Guid idPedido,
            string descripcionPedido,
            int numeroPorciones,
            string tipoMasa,
            Guid idInsumoCrema,
            Guid? idInsumoRelleno,
            Guid? idInsumoColorante,
            Guid idUsuarioRegistro);
    }
}
