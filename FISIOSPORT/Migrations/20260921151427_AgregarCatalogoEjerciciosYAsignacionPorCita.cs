using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FISIOSPORT.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCatalogoEjerciciosYAsignacionPorCita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ProgramaRehabilitacionId",
                table: "EjerciciosAsignados",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "CitaId",
                table: "EjerciciosAsignados",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAsignacion",
                table: "EjerciciosAsignados",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "Ejercicios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FasePaciente",
                table: "Ejercicios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "Ejercicios",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ObjetivoTerapeutico",
                table: "Ejercicios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegionAnatomica",
                table: "Ejercicios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoContraccion",
                table: "Ejercicios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_EjerciciosAsignados_CitaId",
                table: "EjerciciosAsignados",
                column: "CitaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_FasePaciente",
                table: "Ejercicios",
                column: "FasePaciente");

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_Nombre",
                table: "Ejercicios",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_ObjetivoTerapeutico",
                table: "Ejercicios",
                column: "ObjetivoTerapeutico");

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_RegionAnatomica",
                table: "Ejercicios",
                column: "RegionAnatomica");

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_TipoContraccion",
                table: "Ejercicios",
                column: "TipoContraccion");

            migrationBuilder.AddForeignKey(
                name: "FK_EjerciciosAsignados_Citas_CitaId",
                table: "EjerciciosAsignados",
                column: "CitaId",
                principalTable: "Citas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EjerciciosAsignados_Citas_CitaId",
                table: "EjerciciosAsignados");

            migrationBuilder.DropIndex(
                name: "IX_EjerciciosAsignados_CitaId",
                table: "EjerciciosAsignados");

            migrationBuilder.DropIndex(
                name: "IX_Ejercicios_FasePaciente",
                table: "Ejercicios");

            migrationBuilder.DropIndex(
                name: "IX_Ejercicios_Nombre",
                table: "Ejercicios");

            migrationBuilder.DropIndex(
                name: "IX_Ejercicios_ObjetivoTerapeutico",
                table: "Ejercicios");

            migrationBuilder.DropIndex(
                name: "IX_Ejercicios_RegionAnatomica",
                table: "Ejercicios");

            migrationBuilder.DropIndex(
                name: "IX_Ejercicios_TipoContraccion",
                table: "Ejercicios");

            migrationBuilder.DropColumn(
                name: "CitaId",
                table: "EjerciciosAsignados");

            migrationBuilder.DropColumn(
                name: "FechaAsignacion",
                table: "EjerciciosAsignados");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Ejercicios");

            migrationBuilder.DropColumn(
                name: "FasePaciente",
                table: "Ejercicios");

            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "Ejercicios");

            migrationBuilder.DropColumn(
                name: "ObjetivoTerapeutico",
                table: "Ejercicios");

            migrationBuilder.DropColumn(
                name: "RegionAnatomica",
                table: "Ejercicios");

            migrationBuilder.DropColumn(
                name: "TipoContraccion",
                table: "Ejercicios");

            migrationBuilder.AlterColumn<int>(
                name: "ProgramaRehabilitacionId",
                table: "EjerciciosAsignados",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
