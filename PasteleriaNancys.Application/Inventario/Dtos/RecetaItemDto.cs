namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class RecetaItemDto
    {
        public Guid Id { get; set; }
        public Guid IdItemTerminado { get; set; }
        public Guid IdItemInsumo { get; set; }
        public decimal CantidadRequerida { get; set; }
    }
}
