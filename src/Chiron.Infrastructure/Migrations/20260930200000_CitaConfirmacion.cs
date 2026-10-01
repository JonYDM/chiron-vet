using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Portal: el dueño responde si asistirá a la cita. Las citas existentes quedan en
    /// Pendiente (1).
    /// </remarks>
    public partial class CitaConfirmacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Confirmacion",
                table: "Citas",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Confirmacion", table: "Citas");
        }
    }
}