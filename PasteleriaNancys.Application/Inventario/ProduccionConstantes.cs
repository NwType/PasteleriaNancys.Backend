namespace PasteleriaNancys.Application.Inventario
{
    /// <summary>
    /// Reglas fijas de producción del bizcocho, dadas por el usuario (2026-07-13, ajustadas
    /// 2026-07-17 con el modelo de inventario automático) y compartidas entre el registro de
    /// Horneada (<see cref="Services.ConsumoService"/>), el descuento por receta al producir
    /// (<see cref="Services.DespachoService"/>) y la Proyección de Horneado
    /// (Reportes.Services.ReporteService) — un solo lugar para no duplicar estos números.
    /// </summary>
    public static class ProduccionConstantes
    {
        /// <summary>Rinde una batidora de 20L.</summary>
        public const int BizcochosPorBatida = 20;

        /// <summary>
        /// De todas las batidas del día, siempre la ÚLTIMA sale mitad y mitad — el resto son
        /// 100% vainilla. Por eso la producción "estándar" de chocolate de un día normal es fija,
        /// sin importar cuántas batidas se hagan.
        /// </summary>
        public const int BizcochosChocolatePorHorneadaEstandar = BizcochosPorBatida / 2;

        /// <summary>Cada torta se arma con dos bizcochos, cortados capa por capa.</summary>
        public const int BizcochosPorTorta = 2;

        /// <summary>
        /// El bizcocho como ítem Intermedio se mide en PORCIONES, no en unidades: la torta
        /// estándar de 20 porciones lleva 2 bizcochos → un bizcocho estándar rinde 10 porciones,
        /// y un bizcocho para torta de N porciones "vale" N/2 porciones. Así el mismo stock
        /// sirve para tortas de cualquier tamaño sin una receta por tamaño (decisión 2026-07-17).
        /// </summary>
        public const int PorcionesPorBizcocho = 10;

        /// <summary>Una batida rinde 20 bizcochos × 10 porciones = 200 porciones de bizcocho.</summary>
        public const int PorcionesPorBatida = BizcochosPorBatida * PorcionesPorBizcocho;

        /// <summary>
        /// Ítems Intermedios sembrados por la migración AgregarIntermediosMermasYRecetaAnidada —
        /// la receta de la batida (por porción) vive en Receta_Item de estos dos ítems, ya no
        /// hardcodeada en el código.
        /// </summary>
        public const string CodigoBizcochoVainilla = "PI-BIZC-001";
        public const string CodigoBizcochoChocolate = "PI-BIZC-002";

        /// <summary>
        /// Vida útil asumida del bizcocho horneado (se hornea a las 16:00 para armar al día
        /// siguiente; a los 3 días ya no se usa). Asunción marcada — ajustar si la encargada
        /// da el dato real.
        /// </summary>
        public const int DiasVidaUtilBizcocho = 3;

        // Armado de tortas personalizables (2026-07-18): como la combinación la elige el cliente
        // por pedido, no hay Receta_Item fija — las cantidades del armado se derivan de las
        // recetas fijas de la casa divididas entre sus 20 porciones (crema 0.40 kg, relleno
        // 0.30 kg, jalea 0.10 kg por torta estándar). ASUNCIÓN marcada — ajustar con la dueña.

        /// <summary>Kg de crema por porción (0.40 kg / 20 porciones de la torta estándar).</summary>
        public const decimal KgCremaPorPorcion = 0.02m;

        /// <summary>Kg de relleno por porción (0.30 kg / 20 porciones de la torta estándar).</summary>
        public const decimal KgRellenoPorPorcion = 0.015m;

        /// <summary>Kg de jalea/colorante por porción (0.10 kg / 20 porciones de la torta estándar).</summary>
        public const decimal KgColorantePorPorcion = 0.005m;
    }
}
