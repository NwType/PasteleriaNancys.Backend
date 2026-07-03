using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasteleriaNancys.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        // Migración baseline: adopta EF Core Migrations sobre una base de datos ya creada manualmente
        // (esquemas Seguridad, Inventario, Caja, Web con sus 15 tablas reales). Seguridad.Rol y
        // Seguridad.Usuario ya coinciden con el modelo actual, por lo que no requieren DDL. Las demás
        // tablas (Inventario/Caja/Web) del modelo actual todavía no reflejan los nombres/columnas reales
        // de la base de datos (ver CLAUDE.md), así que esta migración se deja vacía a propósito para no
        // crear tablas duplicadas o incorrectas. Cuando esos módulos se alineen con el esquema real,
        // se deberán agregar migraciones nuevas normales sobre esta base.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
