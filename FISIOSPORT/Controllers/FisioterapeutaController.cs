using FISIOSPORT.Data;
using FISIOSPORT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FISIOSPORT.Controllers
{
    [Authorize(Roles = "Fisioterapeuta")]
    public class FisioterapeutaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FisioterapeutaController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int IdActual => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public async Task<IActionResult> Dashboard()
        {
            var fisio = await _context.Fisioterapeutas.FindAsync(IdActual);

            if (fisio == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var citas = await _context.Citas
                .Include(c => c.Paciente)
                .Where(c =>
                    c.FisioterapeutaId == fisio.Id &&
                    c.Estado != "Completada" &&
                    c.Estado != "Cancelada"
                )
                .OrderBy(c => c.Fecha)
                .ThenBy(c => c.Hora)
                .Take(9)
                .ToListAsync();

            var modelo = new DashboardFisioViewModel
            {
                Nombre = fisio.Nombre,
                Especialidad = fisio.Especialidad,
                NumeroColegiado = fisio.NumeroColegiado,

                Citas = citas.Select(c => new CitaCardViewModel
                {
                    Id = c.Id,
                    NombrePaciente = c.Paciente!.Nombre,
                    Fecha = c.Fecha,
                    Hora = c.Hora,
                    Estado = c.Estado
                }).ToList()
            };

            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> CrearCita()
        {
            var modelo = new CrearCitaViewModel
            {
                PacientesDisponibles = await _context.Pacientes.OrderBy(p => p.Nombre).ToListAsync()
            };
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCita(CrearCitaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.PacientesDisponibles = await _context.Pacientes.OrderBy(p => p.Nombre).ToListAsync();
                return View(model);
            }

            var nuevaCita = new Cita
            {
                FisioterapeutaId = IdActual,
                PacienteId = model.PacienteId,
                Fecha = model.Fecha,
                Hora = model.Hora,
                Estado = "Pendiente",
                Notas = model.Notas
            };

            _context.Citas.Add(nuevaCita);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }
        [HttpGet]
        public async Task<IActionResult> DetalleCita(int id)
        {
            var cita = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Fisioterapeuta)
                .Include(c => c.SesionTratamiento)
                    .ThenInclude(s => s!.Tratamiento)
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.FisioterapeutaId == IdActual
                );

            if (cita == null)
            {
                return NotFound();
            }

            var tratamiento = cita.SesionTratamiento?.Tratamiento;

            if (tratamiento == null)
            {
                var modeloSinTratamiento = new DetalleCitaViewModel
                {
                    CitaId = cita.Id,
                    NombrePaciente = cita.Paciente?.Nombre ?? string.Empty,
                    CorreoPaciente = cita.Paciente?.Correo ?? string.Empty,
                    TelefonoPaciente = cita.Paciente?.Telefono ?? string.Empty,
                    Fecha = cita.Fecha,
                    Hora = cita.Hora,
                    EstadoCita = cita.Estado,
                    MotivoConsulta = "Todavía no hay tratamiento asignado.",
                    Diagnostico = "Todavía no hay diagnóstico registrado.",
                    DescripcionTratamiento = "Debes asignar un tratamiento a este paciente.",
                    TotalSesiones = 0,
                    SesionesCompletadas = 0,
                    SesionesRestantes = 0,
                    PorcentajeProgreso = 0,
                    NumeroSesionActual = 0
                };

                return View(modeloSinTratamiento);
            }

            var sesiones = await _context.SesionesTratamiento
                .Where(s => s.TratamientoId == tratamiento.Id)
                .OrderBy(s => s.NumeroSesion)
                .ToListAsync();

            var sesionesCompletadas = sesiones.Count(s => s.Estado == "Completada");

            var sesionActual = sesiones
                .Where(s => s.Estado != "Completada")
                .OrderBy(s => s.NumeroSesion)
                .FirstOrDefault();

            var porcentaje = tratamiento.TotalSesiones > 0
                ? (int)Math.Round(
                    sesionesCompletadas * 100.0 / tratamiento.TotalSesiones
                )
                : 0;

            var modelo = new DetalleCitaViewModel
            {
                CitaId = cita.Id,

                NombrePaciente = cita.Paciente?.Nombre ?? string.Empty,

                CorreoPaciente = cita.Paciente?.Correo ?? string.Empty,

                TelefonoPaciente = cita.Paciente?.Telefono ?? string.Empty,

                Fecha = cita.Fecha,

                Hora = cita.Hora,

                EstadoCita = cita.Estado,

                MotivoConsulta = tratamiento.MotivoConsulta,

                Diagnostico = tratamiento.Diagnostico,

                DescripcionTratamiento = tratamiento.Descripcion,

                TotalSesiones = tratamiento.TotalSesiones,

                SesionesCompletadas = sesionesCompletadas,

                SesionesRestantes = Math.Max(
                    tratamiento.TotalSesiones - sesionesCompletadas,
                    0
                ),

                PorcentajeProgreso = porcentaje,

                SesionActualId = sesionActual?.Id,

                NumeroSesionActual = sesionActual?.NumeroSesion ?? 0,

                TrabajoSesionActual =
                    sesionActual?.TrabajoPlanificado
                    ?? "No hay más sesiones pendientes.",

                Sesiones = sesiones.Select(s => new SesionDetalleViewModel
                {
                    Id = s.Id,
                    NumeroSesion = s.NumeroSesion,
                    TrabajoPlanificado = s.TrabajoPlanificado,
                    Estado = s.Estado,
                    Observaciones = s.Observaciones,
                    FechaCompletada = s.FechaCompletada,
                    EsSesionActual =
                        sesionActual != null &&
                        s.Id == sesionActual.Id,
                    EstaCompletada = s.Estado == "Completada"
                }).ToList()
            };

            return View(modelo);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletarSesion(
    int sesionId,
    int citaId)
        {
            var sesion = await _context.SesionesTratamiento
                .Include(s => s.Tratamiento)
                .FirstOrDefaultAsync(s =>
                    s.Id == sesionId &&
                    s.Tratamiento!.FisioterapeutaId == IdActual
                );

            if (sesion == null)
            {
                return NotFound();
            }

            var cita = await _context.Citas
                .FirstOrDefaultAsync(c =>
                    c.Id == citaId &&
                    c.FisioterapeutaId == IdActual
                );

            if (cita == null)
            {
                return NotFound();
            }

            sesion.Estado = "Completada";
            sesion.FechaCompletada = DateTime.Now;

            cita.Estado = "Completada";

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(DetalleCita),
                new { id = citaId }
            );
        }
    }
}