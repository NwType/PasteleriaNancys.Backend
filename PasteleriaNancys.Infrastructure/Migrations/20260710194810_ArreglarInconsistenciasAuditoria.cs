using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasteleriaNancys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ArreglarInconsistenciasAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "Caja",
                table: "Turno",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "TipoCremaAsociado",
                schema: "Inventario",
                table: "Item_Catalogo",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Venta_Detalle_Lote",
                schema: "Caja",
                columns: table => new
                {
                    IdVentaDetalleLote = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdVentaDetalle = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdLote = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CantidadDescontada = table.Column<decimal>(type: "decimal(8,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venta_Detalle_Lote", x => x.IdVentaDetalleLote);
                    table.ForeignKey(
                        name: "FK_Venta_Detalle_Lote_Venta_Detalle_IdVentaDetalle",
                        column: x => x.IdVentaDetalle,
                        principalSchema: "Caja",
                        principalTable: "Venta_Detalle",
                        principalColumn: "IdDetalle",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Caja_Turno_UsuarioAbierto",
                schema: "Caja",
                table: "Turno",
                column: "IdUsuario_Responsable",
                unique: true,
                filter: "[Estado] = 'Abierto'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Inventario_Item_TipoCremaAsociado",
                schema: "Inventario",
                table: "Item_Catalogo",
                sql: "[TipoCremaAsociado] IS NULL OR [TipoCremaAsociado] IN ('Mascrean','CremaPil','Fondant')");

            migrationBuilder.CreateIndex(
                name: "IX_Venta_Detalle_Lote_IdVentaDetalle",
                schema: "Caja",
                table: "Venta_Detalle_Lote",
                column: "IdVentaDetalle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Venta_Detalle_Lote",
                schema: "Caja");

            migrationBuilder.DropIndex(
                name: "UX_Caja_Turno_UsuarioAbierto",
                schema: "Caja",
                table: "Turno");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Inventario_Item_TipoCremaAsociado",
                schema: "Inventario",
                table: "Item_Catalogo");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "Caja",
                table: "Turno");

            migrationBuilder.DropColumn(
                name: "TipoCremaAsociado",
                schema: "Inventario",
                table: "Item_Catalogo");
        }
    }
}
