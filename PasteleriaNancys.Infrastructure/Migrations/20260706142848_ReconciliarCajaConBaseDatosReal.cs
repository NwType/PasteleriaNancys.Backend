using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasteleriaNancys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReconciliarCajaConBaseDatosReal : Migration
    {
        // Nota: la migración auto-generada intentaba eliminar/recrear "VentaPos"/"TurnoCaja"
        // (el scaffold original, que nunca llegó a existir físicamente porque InitialCreate es
        // un baseline vacío) y crear "Turno"/"Venta_Detalle"/"Gasto_Extra" (que ya existen en la
        // BD real, vacías, creadas directamente por SQL DDL). También arrastraba un rename de
        // Viaje_Despacho.IdUsuarioConductor->IdUsuario_Conductor que ya está aplicado en la BD
        // real desde la reconciliación de Inventario (solo faltaba en el snapshot de EF). Se
        // recorta a mano dejando únicamente el cambio real: la columna MotivoAnulacion, que no
        // existía en Caja.Venta_POS y es necesaria para CU-10 (anular venta, restringido a
        // Administrador).
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MotivoAnulacion",
                schema: "Caja",
                table: "Venta_POS",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MotivoAnulacion",
                schema: "Caja",
                table: "Venta_POS");
        }
    }
}
