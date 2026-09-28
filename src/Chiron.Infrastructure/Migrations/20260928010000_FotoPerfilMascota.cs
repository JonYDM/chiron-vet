using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FotoPerfilMascota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotoPerfilUrl",
                table: "Mascotas",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoPerfilUrl",
                table: "Mascotas");
        }
    }
}
