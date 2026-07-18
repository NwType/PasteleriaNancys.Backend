namespace PasteleriaNancys.Application.Inventario
{
    /// <summary>
    /// Reglas fijas de producción del bizcocho, dadas por el usuario (2026-07-13) y compartidas
    /// entre el registro de Horneada (<see cref="Services.ConsumoService"/>) y la Proyección de
    /// Horneado por sabor (<see cref="Reportes.Services.ReporteService"/>) — un solo lugar para
    /// no duplicar estos números y arriesgar que diverjan.
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
        /// kg de caramelina por batida 100% chocolate adicional (más allá de la mixta estándar).
        /// Derivado, no dato aparte del usuario: la mixta gasta 0.050kg para sus 10 bizcochos de
        /// chocolate (mitad de la batida) — una batida completa de chocolate tiene el doble de
        /// bizcochos de chocolate (20), así que gasta el doble (0.100kg). Confirmado por el
        /// usuario 2026-07-13 que SÍ escala (a diferencia de la mixta, que es monto fijo).
        /// </summary>
        public const decimal CaramelinaKgPorBatidaChocolateExtra = 0.100m;
    }
}
