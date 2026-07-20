namespace PasteleriaNancys.Application.Inventario.Dtos
{
    /// <summary>
    /// Merma directa de stock: insumo que llegó dañado (ej. 5 huevos podridos de un maple),
    /// producto accidentado, lote caducado que se descarta, etc.
    /// </summary>
    public class RegistrarMermaRequest
    {
        public Guid IdItem { get; set; }
        public decimal Cantidad { get; set; }

        // 'Insumo dañado' | 'Caducidad' | 'Accidente' | 'Otro'
        public string TipoMerma { get; set; } = string.Empty;
        public string? Motivo { get; set; }

        // Opcional: lote exacto del que se pierde (ej. el maple concreto de huevos). Si no se
        // indica, se descuenta por orden PEPS (el lote más antiguo primero).
        public Guid? IdLote { get; set; }
    }
}
