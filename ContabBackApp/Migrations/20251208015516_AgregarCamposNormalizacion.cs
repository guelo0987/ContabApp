using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ContabBackApp.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposNormalizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "aplica_itbis",
                table: "tipos_documento",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tasa_itbis",
                table: "tipos_documento",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValueSql: "18.00");

            migrationBuilder.AddColumn<string>(
                name: "tipo_movimiento_esperado",
                table: "tipos_documento",
                type: "character(2)",
                fixedLength: true,
                maxLength: 2,
                nullable: true,
                defaultValueSql: "'DB'::bpchar");

            migrationBuilder.AddColumn<string>(
                name: "codigo",
                table: "cuentas_contables",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "direccion",
                table: "clientes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "clientes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rnc",
                table: "clientes",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "telefono",
                table: "clientes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_cliente",
                table: "clientes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "'Persona'::character varying");

            migrationBuilder.CreateTable(
                name: "configuracion_sistema",
                columns: table => new
                {
                    id_configuracion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    clave = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    valor = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("configuracion_sistema_pkey", x => x.id_configuracion);
                });

            migrationBuilder.CreateIndex(
                name: "idx_cuentas_codigo",
                table: "cuentas_contables",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_clientes_rnc",
                table: "clientes",
                column: "rnc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "configuracion_sistema_clave_key",
                table: "configuracion_sistema",
                column: "clave",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuracion_sistema");

            migrationBuilder.DropIndex(
                name: "idx_cuentas_codigo",
                table: "cuentas_contables");

            migrationBuilder.DropIndex(
                name: "idx_clientes_rnc",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "aplica_itbis",
                table: "tipos_documento");

            migrationBuilder.DropColumn(
                name: "tasa_itbis",
                table: "tipos_documento");

            migrationBuilder.DropColumn(
                name: "tipo_movimiento_esperado",
                table: "tipos_documento");

            migrationBuilder.DropColumn(
                name: "codigo",
                table: "cuentas_contables");

            migrationBuilder.DropColumn(
                name: "direccion",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "email",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "rnc",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "telefono",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "tipo_cliente",
                table: "clientes");
        }
    }
}
