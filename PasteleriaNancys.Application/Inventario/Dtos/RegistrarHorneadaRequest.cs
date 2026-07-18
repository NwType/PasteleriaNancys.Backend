namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class RegistrarHorneadaRequest
    {
        public int CantidadBatidas { get; set; }

        // No existe una fórmula fija de huevos por batida (confirmado por el usuario,
        // 2026-07-13: "no encuentro la fórmula, lo vi en vivo" — varía 20-23 por batida,
        // un poco más en la última por ser mitad y mitad). Por eso se ingresa a mano en
        // cada horneada en vez de calcularse, a diferencia de harina/azúcar/maicena/etc.
        public decimal CantidadHuevos { get; set; }

        // Cuántas de las CantidadBatidas de arriba (sin contar la mixta estándar, que siempre
        // se hace) deben salir 100% chocolate — lo llena el encargado siguiendo la recomendación
        // de la Proyección de Horneado por sabor cuando la demanda de chocolate supera los 10
        // fijos de la mixta. 0 = día normal, ninguna batida extra de chocolate.
        public int CantidadBatidasChocolateExtra { get; set; }
    }
}
