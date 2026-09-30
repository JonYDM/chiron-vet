using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Fase 2 (HU-SU1..SU3): la sucursal pasa a ser la unidad de cobro. Crea la tabla y la
    /// MATRIZ de cada veterinaria existente, heredando su dirección, teléfono, plan,
    /// fecha de renovación y estado. Precio base: $250 mensual / $2,500 anual.
    /// </remarks>
    public partial class SucursalesCobro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sucursales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VeterinariaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Direccion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EsMatriz = table.Column<bool>(type: "boolean", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Plan = table.Column<int>(type: "integer", nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    FechaRenovacion = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sucursales", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_VeterinariaId",
                table: "Sucursales",
                column: "VeterinariaId");

            // Matriz heredada para cada veterinaria existente (gen_random_uuid: PostgreSQL 13+).
            migrationBuilder.Sql(@"
                INSERT INTO ""Sucursales""
                    (""Id"", ""VeterinariaId"", ""Nombre"", ""Direccion"", ""Telefono"", ""EsMatriz"",
                     ""Activa"", ""FechaAlta"", ""Plan"", ""Precio"", ""FechaRenovacion"")
                SELECT gen_random_uuid(), v.""Id"", 'Matriz', v.""Direccion"", LEFT(v.""Telefono"", 20), TRUE,
                       v.""Activa"", v.""FechaAlta"", v.""Plan"",
                       CASE WHEN v.""Plan"" = 2 THEN 2500 ELSE 250 END,
                       v.""FechaRenovacion""
                FROM ""Veterinarias"" v;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Sucursales");
        }
    }
}
