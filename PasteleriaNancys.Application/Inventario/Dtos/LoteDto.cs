namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class LoteDto
    {
        public Guid Id { get; set; }
        public Guid IdItem { get; set; }
        public Guid? IdProveedor { get; set; }
        public string Ubicacion { get; set; } = string.Empty;
        public decimal CantidadInicial { get; set; }
        public decimal CantidadDisponible { get; set; }
        public DateTime FechaElaboracion { get; set; }
        public DateTime FechaCaducidad { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public string? Alerta { get; set; }
    }
}
