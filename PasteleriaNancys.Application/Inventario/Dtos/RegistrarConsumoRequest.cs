namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class RegistrarConsumoRequest
    {
        public Guid IdItem { get; set; }
        public decimal Cantidad { get; set; }
        public string? Motivo { get; set; }
    }
}
