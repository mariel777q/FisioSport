using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FISIOSPORT.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHistorialClinicoYEjercicios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsultasClinicas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PacienteId = table.Column<int>(type: "integer", nullable: false),
                    FisioterapeutaId = table.Column<int>(type: "integer", nullable: false),
                    FechaConsulta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Antecedentes = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    Alergias = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    MotivoConsulta = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultasClinicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultasClinicas_Fisioterapeutas_FisioterapeutaId",
                        column: x => x.FisioterapeutaId,
                        principalTable: "Fisioterapeutas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultasClinicas_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ejercicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    Instrucciones = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    VideoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ejercicios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProgramasRehabilitacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PacienteId = table.Column<int>(type: "integer", nullable: false),
                    FisioterapeutaId = table.Column<int>(type: "integer", nullable: false),
                    Diagnostico = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ObjetivoGeneral = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    TotalSesiones = table.Column<int>(type: "integer", nullable: false),
                    SesionesCompletadas = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramasRehabilitacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramasRehabilitacion_Fisioterapeutas_FisioterapeutaId",
                        column: x => x.FisioterapeutaId,
                        principalTable: "Fisioterapeutas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramasRehabilitacion_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EjerciciosAsignados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProgramaRehabilitacionId = table.Column<int>(type: "integer", nullable: false),
                    EjercicioId = table.Column<int>(type: "integer", nullable: false),
                    Series = table.Column<int>(type: "integer", nullable: false),
                    Repeticiones = table.Column<int>(type: "integer", nullable: false),
                    InstruccionesPersonalizadas = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Frecuencia = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EjerciciosAsignados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EjerciciosAsignados_Ejercicios_EjercicioId",
                        column: x => x.EjercicioId,
                        principalTable: "Ejercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EjerciciosAsignados_ProgramasRehabilitacion_ProgramaRehabil~",
                        column: x => x.ProgramaRehabilitacionId,
                        principalTable: "ProgramasRehabilitacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvolucionesDiarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProgramaRehabilitacionId = table.Column<int>(type: "integer", nullable: false),
                    FisioterapeutaId = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Informe = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    Estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvolucionesDiarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvolucionesDiarias_Fisioterapeutas_FisioterapeutaId",
                        column: x => x.FisioterapeutaId,
                        principalTable: "Fisioterapeutas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvolucionesDiarias_ProgramasRehabilitacion_ProgramaRehabili~",
                        column: x => x.ProgramaRehabilitacionId,
                        principalTable: "ProgramasRehabilitacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosEjerciciosPaciente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EjercicioAsignadoId = table.Column<int>(type: "integer", nullable: false),
                    PacienteId = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCompletado = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Completado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosEjerciciosPaciente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosEjerciciosPaciente_EjerciciosAsignados_EjercicioAs~",
                        column: x => x.EjercicioAsignadoId,
                        principalTable: "EjerciciosAsignados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrosEjerciciosPaciente_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultasClinicas_FisioterapeutaId",
                table: "ConsultasClinicas",
                column: "FisioterapeutaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultasClinicas_PacienteId",
                table: "ConsultasClinicas",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_EjerciciosAsignados_EjercicioId",
                table: "EjerciciosAsignados",
                column: "EjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_EjerciciosAsignados_ProgramaRehabilitacionId",
                table: "EjerciciosAsignados",
                column: "ProgramaRehabilitacionId");

            migrationBuilder.CreateIndex(
                name: "IX_EvolucionesDiarias_FisioterapeutaId",
                table: "EvolucionesDiarias",
                column: "FisioterapeutaId");

            migrationBuilder.CreateIndex(
                name: "IX_EvolucionesDiarias_ProgramaRehabilitacionId",
                table: "EvolucionesDiarias",
                column: "ProgramaRehabilitacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramasRehabilitacion_FisioterapeutaId",
                table: "ProgramasRehabilitacion",
                column: "FisioterapeutaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramasRehabilitacion_PacienteId",
                table: "ProgramasRehabilitacion",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosEjerciciosPaciente_EjercicioAsignadoId_PacienteId_~",
                table: "RegistrosEjerciciosPaciente",
                columns: new[] { "EjercicioAsignadoId", "PacienteId", "Fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosEjerciciosPaciente_PacienteId",
                table: "RegistrosEjerciciosPaciente",
                column: "PacienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsultasClinicas");

            migrationBuilder.DropTable(
                name: "EvolucionesDiarias");

            migrationBuilder.DropTable(
                name: "RegistrosEjerciciosPaciente");

            migrationBuilder.DropTable(
                name: "EjerciciosAsignados");

            migrationBuilder.DropTable(
                name: "Ejercicios");

            migrationBuilder.DropTable(
                name: "ProgramasRehabilitacion");
        }
    }
}
