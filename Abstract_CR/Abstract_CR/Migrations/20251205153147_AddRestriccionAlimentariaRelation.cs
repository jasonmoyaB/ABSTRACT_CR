using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abstract_CR.Migrations
{
    /// <inheritdoc />
    public partial class AddRestriccionAlimentariaRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RestriccionesAlimentarias",
                columns: table => new
                {
                    RestriccionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", maxLength: 2147483647, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestriccionesAlimentarias", x => x.RestriccionID);
                    table.ForeignKey(
                        name: "FK_RestriccionesAlimentarias_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestriccionesAlimentarias_UsuarioID",
                table: "RestriccionesAlimentarias",
                column: "UsuarioID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestriccionesAlimentarias");
        }
    }
}
