namespace PasteleriaNancys.Application.Pedidos.Dtos
{
    public class CrearPedidoWebRequest
    {
        public string NombreCliente { get; set; } = string.Empty;
        public string WhatsApp { get; set; } = string.Empty;
        public DateTime FechaEntregaSolicitada { get; set; }
        public Guid IdItemProductoBase { get; set; }
        public Guid? IdInsumoSaborMasa { get; set; }
        public Guid? IdInsumoRelleno { get; set; }
        public string? TamanoRacion { get; set; }
        public string? ColorDecoracion { get; set; }
        public string? DedicatoriaDetalle { get; set; }
        public string? ImagenReferenciaUrl { get; set; }
        public decimal? PorcentajeAnticipo { get; set; }
        public string? Observaciones { get; set; }

        // Personalización de torta (solo aplica si el producto base tiene NumeroPorciones en el catálogo)
        public int? NumeroPorciones { get; set; }
        public string? TipoMasa { get; set; }
        public string? TipoCrema { get; set; }
        public Guid? IdInsumoCrema { get; set; }
        public Guid? IdInsumoColorDecoracion { get; set; }

        // Velas extra
        public int CantidadVelasNormales { get; set; }
        public string? VelaNumerica { get; set; }
    }
}
