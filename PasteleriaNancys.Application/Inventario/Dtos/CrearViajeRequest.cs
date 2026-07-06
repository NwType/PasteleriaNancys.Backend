namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class CrearViajeRequest
    {
        public Guid? IdUsuarioConductor { get; set; }
        public string Conductor { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
    }
}
