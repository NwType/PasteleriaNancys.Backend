namespace PasteleriaNancys.Application.Caja.Dtos
{
    public class RegistrarGastoRequest
    {
        public string Concepto { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }
}
