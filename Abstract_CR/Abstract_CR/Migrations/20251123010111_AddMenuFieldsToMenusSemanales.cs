using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abstract_CR.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuFieldsToMenusSemanales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar campos nuevos a la tabla existente MenusSemanales
            migrationBuilder.AddColumn<string>(
                name: "NombrePlatillo",
                table: "MenusSemanales",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiaSemana",
                table: "MenusSemanales",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Caracteristicas",
                table: "MenusSemanales",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IngredientesPrincipales",
                table: "MenusSemanales",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipChef",
                table: "MenusSemanales",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaImagen",
                table: "MenusSemanales",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NombrePlatillo",
                table: "MenusSemanales");

            migrationBuilder.DropColumn(
                name: "DiaSemana",
                table: "MenusSemanales");

            migrationBuilder.DropColumn(
                name: "Caracteristicas",
                table: "MenusSemanales");

            migrationBuilder.DropColumn(
                name: "IngredientesPrincipales",
                table: "MenusSemanales");

            migrationBuilder.DropColumn(
                name: "TipChef",
                table: "MenusSemanales");

            migrationBuilder.DropColumn(
                name: "RutaImagen",
                table: "MenusSemanales");
        }
    }
}
