using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abstract_CR.Migrations
{
    /// <inheritdoc />
    public partial class AddInteraccionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PuntosTotales",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MensajesInteraccion",
                columns: table => new
                {
                    MensajeInteraccionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    RemitenteId = table.Column<int>(type: "int", nullable: true),
                    EnviadoPorChef = table.Column<bool>(type: "bit", nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Leido = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensajesInteraccion", x => x.MensajeInteraccionId);
                    table.ForeignKey(
                        name: "FK_MensajesInteraccion_Usuarios_RemitenteId",
                        column: x => x.RemitenteId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MensajesInteraccion_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PuntosUsuarios",
                columns: table => new
                {
                    PuntosUsuarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Puntos = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AsignadoPorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuntosUsuarios", x => x.PuntosUsuarioId);
                    table.ForeignKey(
                        name: "FK_PuntosUsuarios_Usuarios_AsignadoPorId",
                        column: x => x.AsignadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PuntosUsuarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MensajesInteraccion_RemitenteId",
                table: "MensajesInteraccion",
                column: "RemitenteId");

            migrationBuilder.CreateIndex(
                name: "IX_MensajesInteraccion_UsuarioId",
                table: "MensajesInteraccion",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PuntosUsuarios_AsignadoPorId",
                table: "PuntosUsuarios",
                column: "AsignadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PuntosUsuarios_UsuarioId",
                table: "PuntosUsuarios",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MensajesInteraccion");

            migrationBuilder.DropTable(
                name: "PuntosUsuarios");

            migrationBuilder.DropColumn(
                name: "PuntosTotales",
                table: "Usuarios");
        }
    }
}
