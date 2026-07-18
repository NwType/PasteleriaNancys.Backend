namespace PasteleriaNancys.Application.Reportes.Dtos
{
    public class RecomendacionSaboresDto
    {
        public int TortasChocolatePuroPendientes { get; set; }
        public int TortasVainillaPuraPendientes { get; set; }
        public int TortasMixtasPendientes { get; set; }
        public int BizcochosChocolateNecesarios { get; set; }
        public int BizcochosVainillaNecesarios { get; set; }
        public int BizcochosChocolateCubiertosPorBatidaEstandar { get; set; }
        public bool HuboHorneadaRegistradaHoy { get; set; }
        public int BizcochosChocolateProducidosHoy { get; set; }
        public int FaltanteBizcochosChocolate { get; set; }
        public int BatidasCompletasChocolateRecomendadas { get; set; }
        public bool RequiereBatidaCompletaChocolate { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
