using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasteleriaNancys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHorneadaYConsumoInsumo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Horneada",
                schema: "Inventario",
                columns: table => new
                {
                    IdHorneada = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateTime>(type: "date", nullable: false),
                    CantidadBatidas = table.Column<int>(type: "int", nullable: false),
                    CantidadBatidasChocolate = table.Column<int>(type: "int", nullable: false),
                    IdUsuarioRegistro = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Horneada", x => x.IdHorneada);
                    table.CheckConstraint("CK_Inventario_Horneada_CantidadBatidas", "[CantidadBatidas] > 0");
                    table.CheckConstraint("CK_Inventario_Horneada_CantidadBatidasChocolate", "[CantidadBatidasChocolate] >= 0 AND [CantidadBatidasChocolate] <= [CantidadBatidas]");
                });

            migrationBuilder.CreateTable(
                name: "Consumo_Insumo",
                schema: "Inventario",
                columns: table => new
                {
                    IdConsumo = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdHorneada = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IdItem = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdLote = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CantidadDescontada = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    IdUsuarioRegistro = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consumo_Insumo", x => x.IdConsumo);
                    table.ForeignKey(
                        name: "FK_Consumo_Insumo_Horneada_IdHorneada",
                        column: x => x.IdHorneada,
                        principalSchema: "Inventario",
                        principalTable: "Horneada",
                        principalColumn: "IdHorneada",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Consumo_Insumo_Item_Catalogo_IdItem",
                        column: x => x.IdItem,
                        principalSchema: "Inventario",
                        principalTable: "Item_Catalogo",
                        principalColumn: "IdItem",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Consumo_Insumo_Lote_PEPS_IdLote",
                        column: x => x.IdLote,
                        principalSchema: "Inventario",
                        principalTable: "Lote_PEPS",
                        principalColumn: "IdLote",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Consumo_Insumo_IdHorneada",
                schema: "Inventario",
                table: "Consumo_Insumo",
                column: "IdHorneada");

            migrationBuilder.CreateIndex(
                name: "IX_Consumo_Insumo_IdItem",
                schema: "Inventario",
                table: "Consumo_Insumo",
                column: "IdItem");

            migrationBuilder.CreateIndex(
                name: "IX_Consumo_Insumo_IdLote",
                schema: "Inventario",
                table: "Consumo_Insumo",
                column: "IdLote");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Consumo_Insumo",
                schema: "Inventario");

            migrationBuilder.DropTable(
                name: "Horneada",
                schema: "Inventario");
        }
    }
}
