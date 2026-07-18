namespace PasteleriaNancys.Application.Administracion.Dtos
{
    public class DashboardDto
    {
        public decimal TotalVentasHoy { get; set; }
        public int NumeroVentasHoy { get; set; }
        public int PedidosPendientes { get; set; }
        public List<TurnoActivoDto> TurnosActivos { get; set; } = new();
        public int InsumosCriticosActivos { get; set; }
        public bool SinActividadHoy { get; set; }
    }
}
