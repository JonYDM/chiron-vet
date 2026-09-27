using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chiron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CrudCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Cambio",
                table: "Ventas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MetodoPago",
                table: "Ventas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoRecibido",
                table: "Ventas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AtendidoPorId",
                table: "RegistrosMedicos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Mascotas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Clientes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "VeterinarioId",
                table: "Citas",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cambio",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "MetodoPago",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "MontoRecibido",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "AtendidoPorId",
                table: "RegistrosMedicos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "VeterinarioId",
                table: "Citas");
        }
    }
}
