using System;

namespace PasteleriaNancys.Domain.Inventario
{
    /// <summary>
    /// Ledger de descuentos de materia prima que no vienen de una venta: consumo por horneada
    /// (según la receta del bizcocho), consumo automático al producir un lote de torta (según
    /// su Receta_Item) o consumo manual parametrizado por el Encargado de Almacén.
    /// </summary>
    public class ConsumoInsumo
    {
        public Guid Id { get; set; }
        public Guid? IdHorneada { get; set; }
        public Guid IdItem { get; set; }
        public Guid IdLote { get; set; }

        // Trazabilidad de producción: si este consumo se descontó automáticamente por la receta
        // de un producto, apunta al lote PEPS del producto/intermedio que se estaba produciendo.
        public Guid? IdLoteProducido { get; set; }

        // Trazabilidad de pedidos (2026-07-18): si este consumo salió de producir una torta
        // personalizable, apunta al pedido que la originó. FK lógica cross-schema hacia
        // Web.Pedido_Cliente — sin FK física ni navegación, igual que IdUsuarioRegistro.
        public Guid? IdPedido { get; set; }

        public decimal CantidadDescontada { get; set; }
        public string? Motivo { get; set; }
        public DateTime Fecha { get; set; }
        public Guid IdUsuarioRegistro { get; set; }

        public Horneada? Horneada { get; set; }
        public ItemCatalogo Item { get; set; } = null!;
        public LotePeps Lote { get; set; } = null!;
        public LotePeps? LoteProducido { get; set; }
    }
}
