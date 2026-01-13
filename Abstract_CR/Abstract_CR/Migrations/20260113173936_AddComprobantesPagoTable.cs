using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abstract_CR.Migrations
{
    /// <inheritdoc />
    public partial class AddComprobantesPagoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComprobantesPago",
                columns: table => new
                {
                    ComprobanteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    RutaArchivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NombreArchivoOriginal = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TipoArchivo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "sysutcdatetime()"),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pendiente")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprobantesPago", x => x.ComprobanteID);
                    table.ForeignKey(
                        name: "FK_ComprobantesPago_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesPago_UsuarioID",
                table: "ComprobantesPago",
                column: "UsuarioID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComprobantesPago");
        }
    }
}
