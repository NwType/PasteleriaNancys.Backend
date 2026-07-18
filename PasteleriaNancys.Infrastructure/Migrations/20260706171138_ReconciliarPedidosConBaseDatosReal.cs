using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasteleriaNancys.Infrastructure.Migrations
{
    // Nota: la migración auto-generada intentaba eliminar "PedidoWeb" (el scaffold original,
    // que nunca llegó a existir físicamente porque InitialCreate es un baseline vacío) y crear
    // "Pedido_Cliente"/"Pedido_Configuracion" (que ya existen en la BD real, vacías, creadas
    // directamente por SQL DDL). Se recorta a mano dejando únicamente los cambios reales:
    // la tabla Pago_QR (no existía, requerida por CU-04), la columna IdUsuario_Vendedora en
    // Pedido_Cliente (requerida por CU-13 para diferenciar pedidos presenciales de anónimos),
    // el índice único IdPedido en Pedido_Configuracion (cardinalidad 1:1 del diagrama de clases,
    // no forzada físicamente en la BD real hasta ahora) y PrecioUnitario en Item_Catalogo
    // (requerida para que el catálogo web calcule el total automáticamente, CU-01/CU-02).
    /// <inheritdoc />
    public partial class ReconciliarPedidosConBaseDatosReal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecioUnitario",
                schema: "Inventario",
                table: "Item_Catalogo",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "IdUsuario_Vendedora",
                schema: "Web",
                table: "Pedido_Cliente",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Pago_QR",
                schema: "Web",
                columns: table => new
                {
                    IdPago = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdPedido = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaGeneracion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MontoSolicitado = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MontoConfirmado = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Diferencia = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CanalPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CodigoRespuestaBanco = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, defaultValue: "Pendiente")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pago_QR", x => x.IdPago);
                    table.ForeignKey(
                        name: "FK_Pago_QR_Pedido_Cliente_IdPedido",
                        column: x => x.IdPedido,
                        principalSchema: "Web",
                        principalTable: "Pedido_Cliente",
                        principalColumn: "IdPedido",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pago_QR_IdPedido",
                schema: "Web",
                table: "Pago_QR",
                column: "IdPedido");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_Configuracion_IdPedido",
                schema: "Web",
                table: "Pedido_Configuracion",
                column: "IdPedido",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pago_QR",
                schema: "Web");

            migrationBuilder.DropIndex(
                name: "IX_Pedido_Configuracion_IdPedido",
                schema: "Web",
                table: "Pedido_Configuracion");

            migrationBuilder.DropColumn(
                name: "IdUsuario_Vendedora",
                schema: "Web",
                table: "Pedido_Cliente");

            migrationBuilder.DropColumn(
                name: "PrecioUnitario",
                schema: "Inventario",
                table: "Item_Catalogo");
        }
    }
}
