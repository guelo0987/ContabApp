using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ContabBackApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuariosTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Solo crear la tabla usuarios, las demás tablas ya existen
            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    id_auxiliar = table.Column<int>(type: "integer", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    activo = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("usuarios_pkey", x => x.id_usuario);
                    table.ForeignKey(
                        name: "usuarios_id_auxiliar_fkey",
                        column: x => x.id_auxiliar,
                        principalTable: "auxiliares",
                        principalColumn: "id_auxiliar",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_id_auxiliar",
                table: "usuarios",
                column: "id_auxiliar");

            migrationBuilder.CreateIndex(
                name: "usuarios_username_key",
                table: "usuarios",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Solo eliminar la tabla usuarios
            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
