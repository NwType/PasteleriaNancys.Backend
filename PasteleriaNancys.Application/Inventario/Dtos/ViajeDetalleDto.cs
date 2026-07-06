namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class ViajeDetalleDto
    {
        public Guid Id { get; set; }
        public Guid IdLote { get; set; }
        public decimal CantidadEnviada { get; set; }
    }
}
