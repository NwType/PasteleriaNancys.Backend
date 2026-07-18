namespace PasteleriaNancys.Application.Administracion.Dtos
{
    public class VentaPorVendedoraDto
    {
        public Guid IdUsuario { get; set; }
        public string NombreVendedora { get; set; } = string.Empty;
        public int NumeroVentas { get; set; }
        public decimal TotalVendido { get; set; }
    }
}
