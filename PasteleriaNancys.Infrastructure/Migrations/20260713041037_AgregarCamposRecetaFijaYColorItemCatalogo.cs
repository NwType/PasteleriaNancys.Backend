using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasteleriaNancys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposRecetaFijaYColorItemCatalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColorDecoracion",
                schema: "Inventario",
                table: "Item_Catalogo",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdInsumoCrema",
                schema: "Inventario",
                table: "Item_Catalogo",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdInsumoRelleno",
                schema: "Inventario",
                table: "Item_Catalogo",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoMasa",
                schema: "Inventario",
                table: "Item_Catalogo",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Item_Catalogo_IdInsumoCrema",
                schema: "Inventario",
                table: "Item_Catalogo",
                column: "IdInsumoCrema");

            migrationBuilder.CreateIndex(
                name: "IX_Item_Catalogo_IdInsumoRelleno",
                schema: "Inventario",
                table: "Item_Catalogo",
                column: "IdInsumoRelleno");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Inventario_Item_TipoMasa",
                schema: "Inventario",
                table: "Item_Catalogo",
                sql: "[TipoMasa] IS NULL OR [TipoMasa] IN ('Vainilla','Chocolate','Mixto')");

            migrationBuilder.AddForeignKey(
                name: "FK_Item_Catalogo_Item_Catalogo_IdInsumoCrema",
                schema: "Inventario",
                table: "Item_Catalogo",
                column: "IdInsumoCrema",
                principalSchema: "Inventario",
                principalTable: "Item_Catalogo",
                principalColumn: "IdItem",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Item_Catalogo_Item_Catalogo_IdInsumoRelleno",
                schema: "Inventario",
                table: "Item_Catalogo",
                column: "IdInsumoRelleno",
                principalSchema: "Inventario",
                principalTable: "Item_Catalogo",
                principalColumn: "IdItem",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Item_Catalogo_Item_Catalogo_IdInsumoCrema",
                schema: "Inventario",
                table: "Item_Catalogo");

            migrationBuilder.DropForeignKey(
                name: "FK_Item_Catalogo_Item_Catalogo_IdInsumoRelleno",
                schema: "Inventario",
                table: "Item_Catalogo");

            migrationBuilder.DropIndex(
                name: "IX_Item_Catalogo_IdInsumoCrema",
                schema: "Inventario",
                table: "Item_Catalogo");

            migrationBuilder.DropIndex(
                name: "IX_Item_Catalogo_IdInsumoRelleno",
                schema: "Inventario",
                table: "Item_Catalogo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Inventario_Item_TipoMasa",
                schema: "Inventario",
                table: "Item_Catalogo");

            migrationBuilder.DropColumn(
                name: "ColorDecoracion",
                schema: "Inventario",
                table: "Item_Catalogo");

            migrationBuilder.DropColumn(
                name: "IdInsumoCrema",
                schema: "Inventario",
                table: "Item_Catalogo");

            migrationBuilder.DropColumn(
                name: "IdInsumoRelleno",
                schema: "Inventario",
                table: "Item_Catalogo");

            migrationBuilder.DropColumn(
                name: "TipoMasa",
                schema: "Inventario",
                table: "Item_Catalogo");
        }
    }
}
