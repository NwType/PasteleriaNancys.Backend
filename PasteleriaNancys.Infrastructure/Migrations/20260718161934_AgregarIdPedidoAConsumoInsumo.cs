using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasteleriaNancys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarIdPedidoAConsumoInsumo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IdPedido",
                schema: "Inventario",
                table: "Consumo_Insumo",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consumo_Insumo_IdPedido",
                schema: "Inventario",
                table: "Consumo_Insumo",
                column: "IdPedido");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consumo_Insumo_IdPedido",
                schema: "Inventario",
                table: "Consumo_Insumo");

            migrationBuilder.DropColumn(
                name: "IdPedido",
                schema: "Inventario",
                table: "Consumo_Insumo");
        }
    }
}
