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

        public PacienteController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int IdActual => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public async Task<IActionResult> Dashboard()
        {
            var paciente = await _context.Pacientes.FindAsync(IdActual);
            if (paciente == null) return RedirectToAction("Login", "Account");

            var citas = await _context.Citas
                .Include(c => c.Fisioterapeuta)
                .Where(c => c.PacienteId == paciente.Id)
                .OrderBy(c => c.Fecha).ThenBy(c => c.Hora)
                .ToListAsync();

            var modelo = new DashboardPacienteViewModel
            {
                Nombre = paciente.Nombre,
                Citas = citas.Select(c => new CitaPacienteCardViewModel
                {
                    Id = c.Id,
                    NombreFisioterapeuta = c.Fisioterapeuta!.Nombre,
                    Especialidad = c.Fisioterapeuta.Especialidad,
                    Fecha = c.Fecha,
                    Hora = c.Hora,
                    Estado = c.Estado
                }).ToList()
            };

            return View(modelo);
        }
    }
}