using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FISIOSPORT.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTratamientosYSesiones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Fisioterapeutas_FisioterapeutaId",
                table: "Citas");

            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Pacientes_PacienteId",
                table: "Citas");

            migrationBuilder.AddColumn<int>(
                name: "SesionTratamientoId",
                table: "Citas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tratamientos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PacienteId = table.Column<int>(type: "integer", nullable: false),
                    FisioterapeutaId = table.Column<int>(type: "integer", nullable: false),
                    MotivoConsulta = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Diagnostico = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TotalSesiones = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "date", nullable: false),
                    Estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tratamientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tratamientos_Fisioterapeutas_FisioterapeutaId",
                        column: x => x.FisioterapeutaId,
                        principalTable: "Fisioterapeutas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tratamientos_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SesionesTratamiento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TratamientoId = table.Column<int>(type: "integer", nullable: false),
                    NumeroSesion = table.Column<int>(type: "integer", nullable: false),
                    TrabajoPlanificado = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FechaCompletada = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CitaId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionesTratamiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SesionesTratamiento_Tratamientos_TratamientoId",
                        column: x => x.TratamientoId,
                        principalTable: "Tratamientos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Citas_SesionTratamientoId",
                table: "Citas",
                column: "SesionTratamientoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SesionesTratamiento_TratamientoId_NumeroSesion",
                table: "SesionesTratamiento",
                columns: new[] { "TratamientoId", "NumeroSesion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tratamientos_FisioterapeutaId",
                table: "Tratamientos",
                column: "FisioterapeutaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tratamientos_PacienteId",
                table: "Tratamientos",
                column: "PacienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Fisioterapeutas_FisioterapeutaId",
                table: "Citas",
                column: "FisioterapeutaId",
                principalTable: "Fisioterapeutas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Pacientes_PacienteId",
                table: "Citas",
                column: "PacienteId",
                principalTable: "Pacientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_SesionesTratamiento_SesionTratamientoId",
                table: "Citas",
                column: "SesionTratamientoId",
                principalTable: "SesionesTratamiento",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Fisioterapeutas_FisioterapeutaId",
                table: "Citas");

            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Pacientes_PacienteId",
                table: "Citas");

            migrationBuilder.DropForeignKey(
                name: "FK_Citas_SesionesTratamiento_SesionTratamientoId",
                table: "Citas");

            migrationBuilder.DropTable(
                name: "SesionesTratamiento");

            migrationBuilder.DropTable(
                name: "Tratamientos");

            migrationBuilder.DropIndex(
                name: "IX_Citas_SesionTratamientoId",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "SesionTratamientoId",
                table: "Citas");

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Fisioterapeutas_FisioterapeutaId",
                table: "Citas",
                column: "FisioterapeutaId",
                principalTable: "Fisioterapeutas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Pacientes_PacienteId",
                table: "Citas",
                column: "PacienteId",
                principalTable: "Pacientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
