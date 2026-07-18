namespace PasteleriaNancys.Application.Administracion.Dtos
{
    public class VentaPorProductoDto
    {
        public Guid IdItem { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal CantidadVendida { get; set; }
        public decimal TotalVendido { get; set; }
    }
}
