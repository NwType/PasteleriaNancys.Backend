using System;

namespace PasteleriaNancys.Domain.Inventario
{
    /// <summary>
    /// Registro de pérdidas de inventario (requisito del tutor, 2026-07-17): insumos que llegan
    /// dañados (ej. huevos podridos de un maple), preparaciones que salen mal y se descartan,
    /// productos accidentados, etc. Cada fila descuenta stock de UN lote PEPS concreto — una
    /// merma que toca varios lotes genera varias filas (mismo patrón que Consumo_Insumo).
    /// La reposición de lo arruinado NO se registra aquí: es una producción normal
    /// (Horneada o Viaje), que descuenta sus propios insumos por receta.
    /// </summary>
    public class Merma
    {
        public Guid Id { get; set; }
        public Guid IdItem { get; set; }
        public Guid IdLote { get; set; }
        public decimal Cantidad { get; set; }

        // 'Insumo dañado' | 'Producción fallida' | 'Caducidad' | 'Accidente' | 'Otro'
        public string TipoMerma { get; set; } = string.Empty;
        public string? Motivo { get; set; }
        public DateTime Fecha { get; set; }
        public Guid IdUsuarioRegistro { get; set; }

        public ItemCatalogo Item { get; set; } = null!;
        public LotePeps Lote { get; set; } = null!;
    }
}
