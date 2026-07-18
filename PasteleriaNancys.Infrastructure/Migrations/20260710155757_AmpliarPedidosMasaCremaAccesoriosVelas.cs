using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PasteleriaNancys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AmpliarPedidosMasaCremaAccesoriosVelas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CantidadVelasNormales",
                schema: "Web",
                table: "Pedido_Configuracion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "IdInsumo_ColorDecoracion",
                schema: "Web",
                table: "Pedido_Configuracion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdInsumo_Crema",
                schema: "Web",
                table: "Pedido_Configuracion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumeroPorciones",
                schema: "Web",
                table: "Pedido_Configuracion",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoCrema",
                schema: "Web",
                table: "Pedido_Configuracion",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoMasa",
                schema: "Web",
                table: "Pedido_Configuracion",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VelaNumerica",
                schema: "Web",
                table: "Pedido_Configuracion",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Accesorio",
                schema: "Web",
                columns: table => new
                {
                    IdAccesorio = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoAccesorio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CapacidadTortas = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MontoAlquiler = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 20m),
                    MontoGarantia = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 100m),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accesorio", x => x.IdAccesorio);
                    table.CheckConstraint("CK_Web_Accesorio_CapacidadTortas", "[CapacidadTortas] IN (3,5,10,15)");
                    table.CheckConstraint("CK_Web_Accesorio_TipoAccesorio", "[TipoAccesorio] IN ('Vidrio','HierroBlanco','MaderaBlanca')");
                });

            migrationBuilder.CreateTable(
                name: "Tabla_Precio_Porciones",
                schema: "Web",
                columns: table => new
                {
                    IdTablaPrecio = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroPorciones = table.Column<int>(type: "int", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tabla_Precio_Porciones", x => x.IdTablaPrecio);
                });

            migrationBuilder.CreateTable(
                name: "Pedido_Accesorio",
                schema: "Web",
                columns: table => new
                {
                    IdPedidoAccesorio = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdPedido = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdAccesorio = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CantidadTortasEvento = table.Column<int>(type: "int", nullable: false),
                    MontoAlquiler = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MontoGarantia = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    EstadoGarantia = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, defaultValue: "Pendiente"),
                    FechaDevolucion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedido_Accesorio", x => x.IdPedidoAccesorio);
                    table.CheckConstraint("CK_Web_PedidoAccesorio_EstadoGarantia", "[EstadoGarantia] IN ('Pendiente','Devuelta','Retenida')");
                    table.ForeignKey(
                        name: "FK_Pedido_Accesorio_Accesorio_IdAccesorio",
                        column: x => x.IdAccesorio,
                        principalSchema: "Web",
                        principalTable: "Accesorio",
                        principalColumn: "IdAccesorio",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pedido_Accesorio_Pedido_Cliente_IdPedido",
                        column: x => x.IdPedido,
                        principalSchema: "Web",
                        principalTable: "Pedido_Cliente",
                        principalColumn: "IdPedido",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "Web",
                table: "Accesorio",
                columns: new[] { "IdAccesorio", "Activo", "CapacidadTortas", "ImagenUrl", "MontoAlquiler", "MontoGarantia", "Nombre", "TipoAccesorio" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), true, 3, null, 20m, 100m, "Estante de vidrio (3 tortas)", "Vidrio" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), true, 3, null, 20m, 100m, "Estante de hierro blanco (3 tortas)", "HierroBlanco" },
                    { new Guid("20000000-0000-0000-0000-000000000003"), true, 3, null, 20m, 100m, "Estructura de madera blanca (3 tortas)", "MaderaBlanca" },
                    { new Guid("20000000-0000-0000-0000-000000000004"), true, 5, null, 20m, 100m, "Estante de vidrio (5 tortas)", "Vidrio" },
                    { new Guid("20000000-0000-0000-0000-000000000005"), true, 5, null, 20m, 100m, "Estructura de madera blanca (5 tortas)", "MaderaBlanca" },
                    { new Guid("20000000-0000-0000-0000-000000000006"), true, 10, null, 20m, 100m, "Estante de vidrio (10 tortas)", "Vidrio" },
                    { new Guid("20000000-0000-0000-0000-000000000007"), true, 10, null, 20m, 100m, "Estructura de madera blanca (10 tortas)", "MaderaBlanca" },
                    { new Guid("20000000-0000-0000-0000-000000000008"), true, 15, null, 20m, 100m, "Estante de vidrio (15 tortas)", "Vidrio" },
                    { new Guid("20000000-0000-0000-0000-000000000009"), true, 15, null, 20m, 100m, "Estructura de madera blanca (15 tortas)", "MaderaBlanca" }
                });

            migrationBuilder.InsertData(
                schema: "Web",
                table: "Tabla_Precio_Porciones",
                columns: new[] { "IdTablaPrecio", "Activo", "Descripcion", "NumeroPorciones", "Precio" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), true, null, 10, 30m },
                    { new Guid("10000000-0000-0000-0000-000000000002"), true, null, 13, 40m },
                    { new Guid("10000000-0000-0000-0000-000000000003"), true, null, 15, 50m },
                    { new Guid("10000000-0000-0000-0000-000000000004"), true, null, 18, 60m },
                    { new Guid("10000000-0000-0000-0000-000000000005"), true, null, 22, 70m },
                    { new Guid("10000000-0000-0000-0000-000000000006"), true, null, 25, 80m },
                    { new Guid("10000000-0000-0000-0000-000000000007"), true, null, 30, 90m },
                    { new Guid("10000000-0000-0000-0000-000000000008"), true, null, 35, 100m },
                    { new Guid("10000000-0000-0000-0000-000000000009"), true, "Bizcocho cuadrado", 40, 150m }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Web_PedidoConfiguracion_TipoCrema",
                schema: "Web",
                table: "Pedido_Configuracion",
                sql: "[TipoCrema] IS NULL OR [TipoCrema] IN ('Mascrean','CremaPil','Fondant')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Web_PedidoConfiguracion_TipoMasa",
                schema: "Web",
                table: "Pedido_Configuracion",
                sql: "[TipoMasa] IS NULL OR [TipoMasa] IN ('Vainilla','Chocolate','Mixto')");

            migrationBuilder.CreateIndex(
                name: "IX_Accesorio_TipoAccesorio_CapacidadTortas",
                schema: "Web",
                table: "Accesorio",
                columns: new[] { "TipoAccesorio", "CapacidadTortas" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_Accesorio_IdAccesorio",
                schema: "Web",
                table: "Pedido_Accesorio",
                column: "IdAccesorio");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_Accesorio_IdPedido",
                schema: "Web",
                table: "Pedido_Accesorio",
                column: "IdPedido",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tabla_Precio_Porciones_NumeroPorciones",
                schema: "Web",
                table: "Tabla_Precio_Porciones",
                column: "NumeroPorciones",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pedido_Accesorio",
                schema: "Web");

            migrationBuilder.DropTable(
                name: "Tabla_Precio_Porciones",
                schema: "Web");

            migrationBuilder.DropTable(
                name: "Accesorio",
                schema: "Web");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Web_PedidoConfiguracion_TipoCrema",
                schema: "Web",
                table: "Pedido_Configuracion");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Web_PedidoConfiguracion_TipoMasa",
                schema: "Web",
                table: "Pedido_Configuracion");

            migrationBuilder.DropColumn(
                name: "CantidadVelasNormales",
                schema: "Web",
                table: "Pedido_Configuracion");

            migrationBuilder.DropColumn(
                name: "IdInsumo_ColorDecoracion",
                schema: "Web",
                table: "Pedido_Configuracion");

            migrationBuilder.DropColumn(
                name: "IdInsumo_Crema",
                schema: "Web",
                table: "Pedido_Configuracion");

            migrationBuilder.DropColumn(
                name: "NumeroPorciones",
                schema: "Web",
                table: "Pedido_Configuracion");

            migrationBuilder.DropColumn(
                name: "TipoCrema",
                schema: "Web",
                table: "Pedido_Configuracion");

            migrationBuilder.DropColumn(
                name: "TipoMasa",
                schema: "Web",
                table: "Pedido_Configuracion");

            migrationBuilder.DropColumn(
                name: "VelaNumerica",
                schema: "Web",
                table: "Pedido_Configuracion");
        }
    }
}
