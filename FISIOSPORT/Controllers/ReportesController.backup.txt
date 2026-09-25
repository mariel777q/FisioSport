using FISIOSPORT.Data;
using FISIOSPORT.DTOs;
using FISIOSPORT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FISIOSPORT.Controllers
{
    [Authorize(Roles = "Fisioterapeuta")]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CsvReporteService _csvReporteService;

        public ReportesController(
            ApplicationDbContext context,
            CsvReporteService csvReporteService)
        {
            _context = context;
            _csvReporteService = csvReporteService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Citas));
        }

        [HttpGet]
        public async Task<IActionResult> Citas(
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            var idFisioterapeuta = ObtenerIdFisioterapeuta();

            if (idFisioterapeuta == null)
            {
                return Forbid();
            }

            var consulta = _context.Citas
                .AsNoTracking()
                .Include(c => c.Paciente)
                .Where(c =>
                    c.FisioterapeutaId == idFisioterapeuta.Value);

            if (fechaInicio.HasValue)
            {
                var inicio = fechaInicio.Value.Date;

                consulta = consulta.Where(c =>
                    c.Fecha >= inicio);
            }

            if (fechaFin.HasValue)
            {
                var fin = fechaFin.Value.Date;

                consulta = consulta.Where(c =>
                    c.Fecha < fin.AddDays(1));
            }

            var citas = await consulta
                .OrderBy(c => c.Fecha)
                .ThenBy(c => c.Id)
                .Select(c => new CitaReporteDto
                {
                    Id = c.Id,

                    NombrePaciente =
                        c.Paciente != null
                            ? c.Paciente.Nombre
                            : "Paciente",

                    FechaCita = c.Fecha,

                    EstadoCita = c.Estado
                })
                .ToListAsync();

            ViewBag.FechaInicio =
                fechaInicio?.ToString("yyyy-MM-dd");

            ViewBag.FechaFin =
                fechaFin?.ToString("yyyy-MM-dd");

            return View(citas);
        }

        [HttpGet]
        public async Task<IActionResult> CitasCsv(
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            var idFisioterapeuta = ObtenerIdFisioterapeuta();

            if (idFisioterapeuta == null)
            {
                return Forbid();
            }

            var consulta = _context.Citas
                .AsNoTracking()
                .Include(c => c.Paciente)
                .Where(c =>
                    c.FisioterapeutaId == idFisioterapeuta.Value);

            if (fechaInicio.HasValue)
            {
                var inicio = fechaInicio.Value.Date;

                consulta = consulta.Where(c =>
                    c.Fecha >= inicio);
            }

            if (fechaFin.HasValue)
            {
                var fin = fechaFin.Value.Date;

                consulta = consulta.Where(c =>
                    c.Fecha < fin.AddDays(1));
            }

            var citas = await consulta
                .OrderBy(c => c.Fecha)
                .ThenBy(c => c.Id)
                .Select(c => new CitaReporteDto
                {
                    Id = c.Id,

                    NombrePaciente =
                        c.Paciente != null
                            ? c.Paciente.Nombre
                            : "Paciente",

                    FechaCita = c.Fecha,

                    EstadoCita = c.Estado
                })
                .ToListAsync();

            var archivo =
                _csvReporteService.GenerarReporteCitas(citas);

            var nombreArchivo =
                $"Reporte_Citas_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(
                archivo,
                "text/csv; charset=utf-8",
                nombreArchivo);
        }

        private int? ObtenerIdFisioterapeuta()
        {
            var claim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier);

            if (claim == null)
            {
                return null;
            }

            if (!int.TryParse(
                    claim.Value,
                    out var id))
            {
                return null;
            }

            return id;
        }
    }
}