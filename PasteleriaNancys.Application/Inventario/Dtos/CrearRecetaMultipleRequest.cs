namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class LineaRecetaRequest
    {
        public Guid IdItemInsumo { get; set; }
        public decimal CantidadRequerida { get; set; }
    }

    public class CrearRecetaMultipleRequest
    {
        public Guid IdItemTerminado { get; set; }
        public List<LineaRecetaRequest> Lineas { get; set; } = new();
    }
}
