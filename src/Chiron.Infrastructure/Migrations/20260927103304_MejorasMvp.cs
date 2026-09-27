using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MejorasMvp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Diagnostico",
                table: "RegistrosMedicos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notas",
                table: "RegistrosMedicos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PesoKg",
                table: "RegistrosMedicos",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TemperaturaC",
                table: "RegistrosMedicos",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tratamiento",
                table: "RegistrosMedicos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Productos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Esterilizado",
                table: "Mascotas",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Padecimientos",
                table: "Mascotas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PesoKg",
                table: "Mascotas",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Diagnostico",
                table: "RegistrosMedicos");

            migrationBuilder.DropColumn(
                name: "Notas",
                table: "RegistrosMedicos");

            migrationBuilder.DropColumn(
                name: "PesoKg",
                table: "RegistrosMedicos");

            migrationBuilder.DropColumn(
                name: "TemperaturaC",
                table: "RegistrosMedicos");

            migrationBuilder.DropColumn(
                name: "Tratamiento",
                table: "RegistrosMedicos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Esterilizado",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "Padecimientos",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "PesoKg",
                table: "Mascotas");
        }
    }
}
