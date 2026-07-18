using System;
using System.Collections.Generic;

namespace PasteleriaNancys.Domain.Inventario
{
    public class Horneada
    {
        public Guid Id { get; set; }
        public DateTime Fecha { get; set; }
        public int CantidadBatidas { get; set; }

        // Cuántas de las batidas de arriba (además de la mixta estándar, que siempre existe)
        // salieron 100% chocolate — la recomendación de la Proyección de Horneado por sabor
        // puede pedir esto cuando la demanda de chocolate supera lo que cubre la mixta sola.
        public int CantidadBatidasChocolateExtra { get; set; }

        public Guid IdUsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }

        public List<ConsumoInsumo> Consumos { get; set; } = new();
    }
}
