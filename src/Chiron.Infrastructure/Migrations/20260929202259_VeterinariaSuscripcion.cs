using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VeterinariaSuscripcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Veterinarias",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaRenovacion",
                table: "Veterinarias",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "Plan",
                table: "Veterinarias",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // Veterinarias existentes: renovación = 1 mes después de su alta (plan Mensual).
            migrationBuilder.Sql(
                "UPDATE \"Veterinarias\" SET \"FechaRenovacion\" = (\"FechaAlta\" + INTERVAL '1 month')::date " +
                "WHERE \"FechaRenovacion\" = DATE '0001-01-01';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Veterinarias");

            migrationBuilder.DropColumn(
                name: "FechaRenovacion",
                table: "Veterinarias");

            migrationBuilder.DropColumn(
                name: "Plan",
                table: "Veterinarias");
        }
    }
}
