namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class AgregarProductoAlViajeRequest
    {
        public Guid IdItem { get; set; }
        public decimal Cantidad { get; set; }
        public DateTime FechaCaducidad { get; set; }
    }
}
