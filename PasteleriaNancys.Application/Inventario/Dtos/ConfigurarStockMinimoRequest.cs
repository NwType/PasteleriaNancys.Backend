namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class ConfigurarStockMinimoRequest
    {
        public Guid IdItem { get; set; }
        public decimal CantidadMinima { get; set; }
    }
}
