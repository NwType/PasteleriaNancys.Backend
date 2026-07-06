namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class ViajeDto
    {
        public Guid Id { get; set; }
        public Guid? IdUsuarioConductor { get; set; }
        public string Conductor { get; set; } = string.Empty;
        public DateTime FechaDespacho { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public List<ViajeDetalleDto> Detalles { get; set; } = new();
    }
}
