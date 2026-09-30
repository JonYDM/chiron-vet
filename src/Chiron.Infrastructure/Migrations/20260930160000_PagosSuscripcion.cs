using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Fase 3 (HU-SU3..SU5): cada renovación registra un pago; de aquí salen las ganancias.
    /// Las renovaciones anteriores no se registraron, así que el histórico empieza en cero.
    /// </remarks>
    public partial class PagosSuscripcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PagosSuscripcion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VeterinariaId = table.Column<Guid>(type: "uuid", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    FechaPago = table.Column<DateOnly>(type: "date", nullable: false),
                    Plan = table.Column<int>(type: "integer", nullable: false),
                    PeriodoDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodoHasta = table.Column<DateOnly>(type: "date", nullable: false),
                    Nota = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Anulado = table.Column<bool>(type: "boolean", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosSuscripcion", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PagosSuscripcion_FechaPago",
                table: "PagosSuscripcion",
                column: "FechaPago");

            migrationBuilder.CreateIndex(
                name: "IX_PagosSuscripcion_SucursalId",
                table: "PagosSuscripcion",
                column: "SucursalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PagosSuscripcion");
        }
    }
}