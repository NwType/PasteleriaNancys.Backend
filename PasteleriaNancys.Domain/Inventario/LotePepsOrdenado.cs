using System;

namespace PasteleriaNancys.Domain.Inventario
{
    // Entidad sin clave (keyless), mapeada a la vista Inventario.vw_Lotes_PEPS_Ordenados.
    public class LotePepsOrdenado
    {
        public Guid IdLote { get; set; }
        public Guid IdItem { get; set; }
        public string CodigoReferencia { get; set; } = string.Empty;
        public string NombreItem { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public decimal CantidadInicial { get; set; }
        public decimal CantidadDisponible { get; set; }
        public DateTime FechaElaboracion { get; set; }
        public DateTime FechaCaducidad { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int? DiasParaCaducar { get; set; }
        public long? OrdenConsumo { get; set; }
    }
}
