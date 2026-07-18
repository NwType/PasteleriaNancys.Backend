using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasteleriaNancys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarInsumosPersonalizacionPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IdInsumo_Relleno",
                schema: "Web",
                table: "Pedido_Configuracion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdInsumo_SaborMasa",
                schema: "Web",
                table: "Pedido_Configuracion",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdInsumo_Relleno",
                schema: "Web",
                table: "Pedido_Configuracion");

            migrationBuilder.DropColumn(
                name: "IdInsumo_SaborMasa",
                schema: "Web",
                table: "Pedido_Configuracion");
        }
    }
}
