namespace PasteleriaNancys.Application.Inventario.Dtos
{
    public class RegistrarHorneadaRequest
    {
        public int CantidadBatidas { get; set; }

        // OBSOLETO (2026-07-17): el usuario fijó 25 huevos por batida, así que el huevo ahora es
        // una línea más de la receta del bizcocho (0.125 huevo/porción × 200 porciones/batida
        // = 25) y ya no se ingresa a mano. Se conserva la propiedad solo para no romper el
        // contrato con el frontend actual; el backend la ignora.
        public decimal CantidadHuevos { get; set; }

        // Cuántas de las CantidadBatidas de arriba (sin contar la mixta estándar, que siempre
        // se hace) deben salir 100% chocolate — lo llena el encargado siguiendo la recomendación
        // de la Proyección de Horneado por sabor cuando la demanda de chocolate supera los 10
        // fijos de la mixta. 0 = día normal, ninguna batida extra de chocolate.
        public int CantidadBatidasChocolateExtra { get; set; }
    }
}
