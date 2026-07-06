namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class AgregarLoteAlViajeRequest
    {
        public Guid IdLote { get; set; }
        public decimal CantidadEnviada { get; set; }
    }
}
