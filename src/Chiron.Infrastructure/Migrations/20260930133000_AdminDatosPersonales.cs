using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// HU-SA4: datos personales del staff en Usuarios. Todas nullable: los usuarios
    /// existentes quedan en null y no se rompen.
    /// </remarks>
    public partial class AdminDatosPersonales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApellidoMaterno",
                table: "Usuarios",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApellidoPaterno",
                table: "Usuarios",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Curp",
                table: "Usuarios",
                type: "character varying(18)",
                maxLength: 18,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Usuarios",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ApellidoMaterno", table: "Usuarios");
            migrationBuilder.DropColumn(name: "ApellidoPaterno", table: "Usuarios");
            migrationBuilder.DropColumn(name: "Curp", table: "Usuarios");
            migrationBuilder.DropColumn(name: "Telefono", table: "Usuarios");
        }
    }
}
