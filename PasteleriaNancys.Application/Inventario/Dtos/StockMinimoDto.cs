namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class StockMinimoDto
    {
        public Guid Id { get; set; }
        public Guid IdItem { get; set; }
        public decimal CantidadMinima { get; set; }
        public bool Activo { get; set; }
    }
}
