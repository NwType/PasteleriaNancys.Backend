namespace PasteleriaNancys.Application.Inventario.Dtos
{
    /// <summary>
    /// Merma de producción: una preparación salió mal y se descarta ANTES de convertirse en
    /// stock (la torta/bizcocho arruinado nunca entró a un lote). Se explota la receta del
    /// producto y se descuentan sus componentes como merma. La reposición se registra aparte,
    /// como producción normal (Horneada o Viaje) — así quedan descontados "lo que se arruinó"
    /// y "lo repuesto", tal como pidió el tutor.
    /// </summary>
    public class RegistrarMermaProduccionRequest
    {
        public Guid IdItemProducto { get; set; }
        public decimal Cantidad { get; set; }
        public string? Motivo { get; set; }
    }
}
