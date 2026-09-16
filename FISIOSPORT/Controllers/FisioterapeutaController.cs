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
            if (fisio == null) return RedirectToAction("Login", "Account");

            var citas = await _context.Citas
                .Include(c => c.Paciente)
                .Where(c => c.FisioterapeutaId == fisio.Id)
                .OrderBy(c => c.Fecha).ThenBy(c => c.Hora)
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
    }
}