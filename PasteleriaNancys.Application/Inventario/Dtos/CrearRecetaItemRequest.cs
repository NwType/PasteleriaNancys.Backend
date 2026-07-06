namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class CrearRecetaItemRequest
    {
        public Guid IdItemTerminado { get; set; }
        public Guid IdItemInsumo { get; set; }
        public decimal CantidadRequerida { get; set; }
    }
}
