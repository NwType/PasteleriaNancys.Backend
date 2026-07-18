namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class RegistrarLoteRequest
    {
        public Guid IdItem { get; set; }
        public Guid? IdProveedor { get; set; }
        public decimal CantidadInicial { get; set; }
        public DateTime FechaElaboracion { get; set; }
        public DateTime FechaCaducidad { get; set; }
    }
}
