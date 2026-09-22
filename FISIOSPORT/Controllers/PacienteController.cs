using FISIOSPORT.Data;
using FISIOSPORT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FISIOSPORT.Controllers
{
    [Authorize(Roles = "Paciente")]
    public class PacienteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PacienteController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        private int IdActual =>
            int.Parse(
                User.FindFirst(
                    ClaimTypes.NameIdentifier)!.Value);

        // =========================================================
        // DASHBOARD
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var paciente =
                await _context.Pacientes
                    .FindAsync(IdActual);

            if (paciente == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            // =====================================================
            // CITAS
            // =====================================================

            var citas =
                await _context.Citas
                    .AsNoTracking()
                    .Include(c => c.Fisioterapeuta)
                    .Where(c =>
                        c.PacienteId ==
                        paciente.Id)
                    .OrderBy(c => c.Fecha)
                    .ThenBy(c => c.Hora)
                    .ToListAsync();

            // =====================================================
            // EJERCICIOS PENDIENTES
            // =====================================================

            var asignaciones =
                await _context.EjerciciosAsignados
                    .AsNoTracking()
                    .Include(a => a.Ejercicio)
                    .Include(a => a.Cita)
                    .Where(a =>
                        a.Activo &&

                        a.Cita != null &&

                        a.Cita.PacienteId ==
                        paciente.Id &&

                        a.Cita.Estado !=
                        "Cancelada" &&

                        !_context
                            .RegistrosEjerciciosPaciente
                            .Any(r =>
                                r.EjercicioAsignadoId ==
                                a.Id &&

                                r.PacienteId ==
                                paciente.Id &&

                                r.Completado))
                    .OrderBy(a => a.Cita!.Fecha)
                    .ThenBy(a => a.Cita!.Hora)
                    .ThenBy(a => a.Id)
                    .ToListAsync();

            var ejercicios =
                asignaciones
                    .Select(a =>
                        new EjercicioPacienteViewModel
                        {
                            AsignacionId =
                                a.Id,

                            Nombre =
                                a.Ejercicio?.Nombre ??
                                "Ejercicio",

                            Descripcion =
                                a.Ejercicio?.Descripcion ??
                                string.Empty,

                            ImagenUrl =
                                a.Ejercicio?.ImagenUrl ??
                                string.Empty,

                            VideoUrl =
                                a.Ejercicio?.VideoUrl ??
                                string.Empty,

                            TipoContraccion =
                                a.Ejercicio?.TipoContraccion ??
                                string.Empty,

                            RegionAnatomica =
                                a.Ejercicio?.RegionAnatomica ??
                                string.Empty,

                            ObjetivoTerapeutico =
                                a.Ejercicio?.ObjetivoTerapeutico ??
                                string.Empty,

                            FasePaciente =
                                a.Ejercicio?.FasePaciente ??
                                string.Empty,

                            Series =
                                a.Series,

                            Repeticiones =
                                a.Repeticiones,

                            Instrucciones =
                                string.IsNullOrWhiteSpace(
                                    a.InstruccionesPersonalizadas)
                                    ? a.Ejercicio?.Instrucciones ??
                                      string.Empty
                                    : a.InstruccionesPersonalizadas,

                            FechaCita =
                                a.Cita?.Fecha ??
                                DateTime.Today,

                            HoraCita =
                                a.Cita?.Hora ??
                                string.Empty
                        })
                    .ToList();

            var modelo =
                new DashboardPacienteViewModel
                {
                    Nombre =
                        paciente.Nombre,

                    Citas =
                        citas
                            .Select(c =>
                                new CitaPacienteCardViewModel
                                {
                                    Id =
                                        c.Id,

                                    NombreFisioterapeuta =
                                        c.Fisioterapeuta?.Nombre ??
                                        "Fisioterapeuta",

                                    Especialidad =
                                        c.Fisioterapeuta?.Especialidad ??
                                        string.Empty,

                                    Fecha =
                                        c.Fecha,

                                    Hora =
                                        c.Hora,

                                    Estado =
                                        c.Estado
                                })
                            .ToList(),

                    EjerciciosPendientes =
                        ejercicios
                };

            return View(modelo);
        }
    }
}