using FISIOSPORT.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FISIOSPORT.Controllers
{
    [Authorize(Roles = "Paciente")]
    public class EjerciciosPacienteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EjerciciosPacienteController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        private int IdActual =>
            int.Parse(
                User.FindFirst(
                    ClaimTypes.NameIdentifier)!.Value);

        // =========================================================
        // COMPLETAR EJERCICIO
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Completar(
            int id)
        {
            var asignacion =
                await _context.EjerciciosAsignados
                    .Include(a => a.Cita)
                    .FirstOrDefaultAsync(a =>
                        a.Id == id &&
                        a.Activo &&
                        a.Cita != null &&
                        a.Cita.PacienteId == IdActual);

            if (asignacion == null)
                return NotFound();

            var registro =
                await _context.RegistrosEjerciciosPaciente
                    .FirstOrDefaultAsync(r =>
                        r.EjercicioAsignadoId == id &&
                        r.PacienteId == IdActual);

            if (registro == null)
            {
                registro =
                    new Models.RegistroEjercicioPaciente
                    {
                        EjercicioAsignadoId =
                            id,

                        PacienteId =
                            IdActual,

                        Fecha =
                            DateTime.UtcNow.Date,

                        FechaCompletado =
                            DateTime.UtcNow,

                        Completado = true
                    };

                _context.RegistrosEjerciciosPaciente.Add(
                    registro);
            }
            else
            {
                registro.Completado = true;

                registro.FechaCompletado =
                    DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            TempData["MensajeEjercicio"] =
                "Ejercicio completado correctamente.";

            return RedirectToAction(
                "Dashboard",
                "Paciente");
        }
    }
}