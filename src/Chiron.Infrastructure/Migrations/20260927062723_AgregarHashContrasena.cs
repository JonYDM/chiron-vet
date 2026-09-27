using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHashContrasena : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HashContrasena",
                table: "Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HashContrasena",
                table: "Usuarios");
        }
    }
}
