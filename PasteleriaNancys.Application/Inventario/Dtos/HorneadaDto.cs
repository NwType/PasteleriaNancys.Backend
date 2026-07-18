namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class HorneadaDto
    {
        public Guid Id { get; set; }
        public DateTime Fecha { get; set; }
        public int CantidadBatidas { get; set; }
        public int CantidadBatidasChocolateExtra { get; set; }
        public int TotalBizcochos { get; set; }
        public int BizcochosVainilla { get; set; }
        public int BizcochosChocolate { get; set; }
        public DateTime FechaRegistro { get; set; }
        public List<ConsumoInsumoDto> Consumos { get; set; } = new();
    }
}
