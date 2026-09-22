using FISIOSPORT.Data;
using FISIOSPORT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FISIOSPORT.Controllers
{
    [Authorize(Roles = "Fisioterapeuta")]
    public class EjerciciosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EjerciciosController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int IdActual =>
            int.Parse(
                User.FindFirst(
                    ClaimTypes.NameIdentifier)!.Value);

        // =========================================================
        // CATÁLOGO DE EJERCICIOS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            int citaId,
            string? buscar)
        {
            await AsegurarCatalogoAsync();

            var cita = await ObtenerCitaAsync(citaId);

            if (cita == null)
                return NotFound();

            var modelo = await ConstruirModeloAsync(
                cita,
                buscar,
                null);

            return View(modelo);
        }

        // =========================================================
        // ASIGNAR EJERCICIOS A UNA CITA
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Asignar(
            EjerciciosIndexViewModel model)
        {
            var cita = await ObtenerCitaAsync(model.CitaId);

            if (cita == null)
                return NotFound();

            if (model.Ejercicios == null ||
                !model.Ejercicios.Any(e => e.Seleccionado))
            {
                ModelState.AddModelError(
                    "",
                    "Selecciona al menos un ejercicio.");

                model = await ConstruirModeloAsync(
                    cita,
                    model.Buscar,
                    model.Ejercicios);

                return View("Index", model);
            }

            var seleccionados = model.Ejercicios
                .Where(e => e.Seleccionado)
                .ToList();

            // -----------------------------------------------------
            // VALIDAR SERIES Y REPETICIONES
            // -----------------------------------------------------

            foreach (var ejercicio in seleccionados)
            {
                if (ejercicio.Series < 1 ||
                    ejercicio.Series > 20)
                {
                    ModelState.AddModelError(
                        "",
                        $"Las series de {ejercicio.Nombre} deben estar entre 1 y 20.");
                }

                if (ejercicio.Repeticiones < 1 ||
                    ejercicio.Repeticiones > 100)
                {
                    ModelState.AddModelError(
                        "",
                        $"Las repeticiones de {ejercicio.Nombre} deben estar entre 1 y 100.");
                }
            }

            if (!ModelState.IsValid)
            {
                model = await ConstruirModeloAsync(
                    cita,
                    model.Buscar,
                    model.Ejercicios);

                return View("Index", model);
            }

            // -----------------------------------------------------
            // VERIFICAR QUE LOS EJERCICIOS EXISTAN Y ESTÉN ACTIVOS
            // -----------------------------------------------------

            var ids = seleccionados
                .Select(e => e.EjercicioId)
                .Distinct()
                .ToList();

            var ejerciciosDb = await _context.Ejercicios
                .Where(e =>
                    ids.Contains(e.Id) &&
                    e.Activo)
                .ToListAsync();

            if (ejerciciosDb.Count != ids.Count)
            {
                ModelState.AddModelError(
                    "",
                    "Uno de los ejercicios seleccionados ya no está disponible.");

                model = await ConstruirModeloAsync(
                    cita,
                    model.Buscar,
                    model.Ejercicios);

                return View("Index", model);
            }

            // -----------------------------------------------------
            // BUSCAR PROGRAMA DE REHABILITACIÓN ACTIVO
            // -----------------------------------------------------

            var programa =
                await _context.ProgramasRehabilitacion
                    .Where(p =>
                        p.PacienteId == cita.PacienteId &&
                        p.FisioterapeutaId == IdActual &&
                        p.Estado == "Activo")
                    .OrderByDescending(p => p.FechaInicio)
                    .FirstOrDefaultAsync();

            // -----------------------------------------------------
            // OBTENER ASIGNACIONES EXISTENTES
            // -----------------------------------------------------

            var asignacionesExistentes =
                await _context.EjerciciosAsignados
                    .Where(a =>
                        a.CitaId == cita.Id &&
                        ids.Contains(a.EjercicioId))
                    .ToListAsync();

            // -----------------------------------------------------
            // CREAR O ACTUALIZAR ASIGNACIONES
            // -----------------------------------------------------

            foreach (var seleccionado in seleccionados)
            {
                var asignacion =
                    asignacionesExistentes.FirstOrDefault(
                        a => a.EjercicioId ==
                             seleccionado.EjercicioId);

                if (asignacion == null)
                {
                    asignacion = new EjercicioAsignado
                    {
                        CitaId = cita.Id,

                        ProgramaRehabilitacionId =
                            programa?.Id,

                        EjercicioId =
                            seleccionado.EjercicioId,

                        Series =
                            seleccionado.Series,

                        Repeticiones =
                            seleccionado.Repeticiones,

                        InstruccionesPersonalizadas =
                            seleccionado
                                .InstruccionesPersonalizadas
                                ?.Trim()
                                ?? string.Empty,

                        Frecuencia = "Una vez",

                        Activo = true,

                        FechaAsignacion =
                            DateTime.UtcNow
                    };

                    _context.EjerciciosAsignados.Add(asignacion);
                }
                else
                {
                    asignacion.ProgramaRehabilitacionId =
                        programa?.Id;

                    asignacion.Series =
                        seleccionado.Series;

                    asignacion.Repeticiones =
                        seleccionado.Repeticiones;

                    asignacion.InstruccionesPersonalizadas =
                        seleccionado
                            .InstruccionesPersonalizadas
                            ?.Trim()
                            ?? string.Empty;

                    asignacion.Frecuencia =
                        string.IsNullOrWhiteSpace(
                            asignacion.Frecuencia)
                            ? "Una vez"
                            : asignacion.Frecuencia;

                    asignacion.Activo = true;

                    asignacion.FechaAsignacion =
                        DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                "Los ejercicios fueron asignados correctamente.";

            return RedirectToAction(
                "DetalleCita",
                "Fisioterapeuta",
                new
                {
                    id = cita.Id
                });
        }

        // =========================================================
        // CONSTRUIR MODELO DEL CATÁLOGO
        // =========================================================

        private async Task<EjerciciosIndexViewModel>
            ConstruirModeloAsync(
                Cita cita,
                string? buscar,
                List<EjercicioSeleccionViewModel>? enviados)
        {
            var query = _context.Ejercicios
                .AsNoTracking()
                .Where(e => e.Activo);

            // -----------------------------------------------------
            // ÚNICO FILTRO: BÚSQUEDA GENERAL
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim().ToLower();

                query = query.Where(e =>
                    e.Nombre.ToLower().Contains(texto) ||
                    e.Descripcion.ToLower().Contains(texto) ||
                    e.Instrucciones.ToLower().Contains(texto) ||
                    e.TipoContraccion.ToLower().Contains(texto) ||
                    e.ObjetivoTerapeutico.ToLower().Contains(texto) ||
                    e.RegionAnatomica.ToLower().Contains(texto) ||
                    e.FasePaciente.ToLower().Contains(texto));
            }

            // -----------------------------------------------------
            // OBTENER EJERCICIOS
            // -----------------------------------------------------

            var ejercicios =
                await query
                    .OrderBy(e => e.ObjetivoTerapeutico)
                    .ThenBy(e => e.TipoContraccion)
                    .ThenBy(e => e.Nombre)
                    .ToListAsync();

            // -----------------------------------------------------
            // PRESERVAR DATOS ENVIADOS SI HUBO ERROR
            // -----------------------------------------------------

            if (enviados != null &&
                enviados.Count > 0)
            {
                var enviadosIds = enviados
                    .Select(e => e.EjercicioId)
                    .Distinct()
                    .ToList();

                var adicionales =
                    await _context.Ejercicios
                        .AsNoTracking()
                        .Where(e =>
                            enviadosIds.Contains(e.Id) &&
                            e.Activo)
                        .ToListAsync();

                foreach (var adicional in adicionales)
                {
                    if (!ejercicios.Any(
                        x => x.Id == adicional.Id))
                    {
                        ejercicios.Add(adicional);
                    }
                }

                ejercicios =
                    ejercicios
                        .OrderBy(e => e.ObjetivoTerapeutico)
                        .ThenBy(e => e.TipoContraccion)
                        .ThenBy(e => e.Nombre)
                        .ToList();
            }

            // -----------------------------------------------------
            // ASIGNACIONES EXISTENTES DE LA CITA
            // -----------------------------------------------------

            var existentes =
                await _context.EjerciciosAsignados
                    .AsNoTracking()
                    .Where(a =>
                        a.CitaId == cita.Id &&
                        a.Activo)
                    .ToListAsync();

            // -----------------------------------------------------
            // CONSTRUIR VIEWMODEL
            // -----------------------------------------------------

            var modelo =
                new EjerciciosIndexViewModel
                {
                    CitaId =
                        cita.Id,

                    PacienteNombre =
                        cita.Paciente?.Nombre ??
                        "Paciente",

                    FechaCita =
                        cita.Fecha,

                    HoraCita =
                        cita.Hora,

                    Buscar =
                        buscar,

                    TodosLosNombres =
                        await _context.Ejercicios
                            .AsNoTracking()
                            .Where(e => e.Activo)
                            .Select(e => e.Nombre)
                            .Distinct()
                            .OrderBy(n => n)
                            .ToListAsync()
                };

            // -----------------------------------------------------
            // MAPEAR EJERCICIOS
            // -----------------------------------------------------

            foreach (var ejercicio in ejercicios)
            {
                var enviado =
                    enviados?.FirstOrDefault(
                        e =>
                            e.EjercicioId ==
                            ejercicio.Id);

                var existente =
                    existentes.FirstOrDefault(
                        e =>
                            e.EjercicioId ==
                            ejercicio.Id);

                modelo.Ejercicios.Add(
                    new EjercicioSeleccionViewModel
                    {
                        EjercicioId =
                            ejercicio.Id,

                        Nombre =
                            ejercicio.Nombre,

                        Descripcion =
                            ejercicio.Descripcion,

                        Instrucciones =
                            ejercicio.Instrucciones,

                        VideoUrl =
                            ejercicio.VideoUrl ??
                            string.Empty,

                        ImagenUrl =
                            ejercicio.ImagenUrl ??
                            string.Empty,

                        TipoContraccion =
                            ejercicio.TipoContraccion ??
                            string.Empty,

                        RegionAnatomica =
                            ejercicio.RegionAnatomica ??
                            string.Empty,

                        ObjetivoTerapeutico =
                            ejercicio.ObjetivoTerapeutico ??
                            string.Empty,

                        FasePaciente =
                            ejercicio.FasePaciente ??
                            string.Empty,

                        Seleccionado =
                            enviado?.Seleccionado ??
                            existente != null,

                        Series =
                            enviado?.Series ??
                            existente?.Series ??
                            3,

                        Repeticiones =
                            enviado?.Repeticiones ??
                            existente?.Repeticiones ??
                            10,

                        InstruccionesPersonalizadas =
                            enviado?.InstruccionesPersonalizadas ??
                            existente?.InstruccionesPersonalizadas ??
                            string.Empty
                    });
            }

            return modelo;
        }

        // =========================================================
        // PENDIENTES DE UNA CITA
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> PendientesCita(
            int citaId)
        {
            var cita =
                await ObtenerCitaAsync(citaId);

            if (cita == null)
                return NotFound();

            var asignaciones =
                await _context.EjerciciosAsignados
                    .Include(a => a.Ejercicio)
                    .Where(a =>
                        a.CitaId == citaId &&
                        a.Activo)
                    .OrderBy(a => a.Id)
                    .ToListAsync();

            var ids =
                asignaciones
                    .Select(a => a.Id)
                    .ToList();

            var completados =
                await _context
                    .RegistrosEjerciciosPaciente
                    .Where(r =>
                        ids.Contains(
                            r.EjercicioAsignadoId) &&
                        r.PacienteId ==
                            cita.PacienteId &&
                        r.Completado)
                    .ToListAsync();

            var modelo =
                new EjerciciosCitaViewModel
                {
                    CitaId =
                        cita.Id,

                    PacienteNombre =
                        cita.Paciente?.Nombre ??
                        "Paciente",

                    Ejercicios =
                        asignaciones
                            .Select(a =>
                            {
                                var registro =
                                    completados
                                        .FirstOrDefault(
                                            r =>
                                                r.EjercicioAsignadoId ==
                                                a.Id);

                                return new EjercicioPendienteItemViewModel
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

                                    Completado =
                                        registro?.Completado == true,

                                    FechaCompletado =
                                        registro?.FechaCompletado
                                };
                            })
                            .ToList()
                };

            return PartialView(
                "_PendientesCita",
                modelo);
        }

        // =========================================================
        // RESUMEN PARA DASHBOARD
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> ResumenCita(
            int citaId)
        {
            var cita =
                await _context.Citas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.Id == citaId &&
                        c.FisioterapeutaId ==
                            IdActual);

            if (cita == null)
                return NotFound();

            var asignaciones =
                await _context.EjerciciosAsignados
                    .Where(a =>
                        a.CitaId == citaId &&
                        a.Activo)
                    .Select(a => a.Id)
                    .ToListAsync();

            var completados =
                asignaciones.Count == 0
                    ? 0
                    : await _context
                        .RegistrosEjerciciosPaciente
                        .CountAsync(r =>
                            asignaciones.Contains(
                                r.EjercicioAsignadoId) &&
                            r.PacienteId ==
                                cita.PacienteId &&
                            r.Completado);

            return Json(
                new
                {
                    citaId,

                    total =
                        asignaciones.Count,

                    completados,

                    pendientes =
                        Math.Max(
                            0,
                            asignaciones.Count -
                            completados)
                });
        }

        // =========================================================
        // OBTENER CITA
        // =========================================================

        private async Task<Cita?> ObtenerCitaAsync(
            int citaId)
        {
            return await _context.Citas
                .Include(c => c.Paciente)
                .FirstOrDefaultAsync(c =>
                    c.Id == citaId &&
                    c.FisioterapeutaId ==
                        IdActual);
        }

        // =========================================================
        // CATÁLOGO INICIAL
        // =========================================================

        private async Task AsegurarCatalogoAsync()
        {
            var existentes =
                await _context.Ejercicios
                    .Select(e => e.Nombre)
                    .ToListAsync();

            var nuevos =
                new List<Ejercicio>();

            void Agregar(
                string nombre,
                string descripcion,
                string instrucciones,
                string tipo,
                string region,
                string objetivo,
                string fase)
            {
                if (existentes.Contains(
                    nombre,
                    StringComparer.OrdinalIgnoreCase))
                {
                    return;
                }

                nuevos.Add(
                    new Ejercicio
                    {
                        Nombre =
                            nombre,

                        Descripcion =
                            descripcion,

                        Instrucciones =
                            instrucciones,

                        VideoUrl =
                            "https://www.youtube.com/results?search_query=" +
                            Uri.EscapeDataString(
                                nombre +
                                " fisioterapia técnica"),

                        ImagenUrl =
                            "https://placehold.co/800x500/png?text=" +
                            Uri.EscapeDataString(nombre),

                        TipoContraccion =
                            tipo,

                        RegionAnatomica =
                            region,

                        ObjetivoTerapeutico =
                            objetivo,

                        FasePaciente =
                            fase,

                        Activo =
                            true
                    });
            }

            Agregar(
                "Sentadilla",
                "Ejercicio para fortalecer miembros inferiores.",
                "Mantener la espalda neutra y controlar el descenso.",
                "Concéntrico",
                "Rodilla",
                "Fuerza",
                "Intermedio");

            Agregar(
                "Puente de glúteos",
                "Fortalecimiento de glúteos y estabilización de cadera.",
                "Elevar la pelvis lentamente manteniendo el abdomen activo.",
                "Concéntrico",
                "Cadera",
                "Fuerza",
                "Inicial (agudo)");

            Agregar(
                "Elevación de talones",
                "Fortalecimiento de la musculatura de la pantorrilla.",
                "Elevar los talones y regresar lentamente.",
                "Concéntrico",
                "Tobillo",
                "Fuerza",
                "Inicial (agudo)");

            Agregar(
                "Movilidad de tobillo",
                "Movilización controlada de la articulación del tobillo.",
                "Realizar el movimiento lentamente y sin dolor.",
                "Isométrico",
                "Tobillo",
                "Movilidad",
                "Inicial (agudo)");

            Agregar(
                "Rotación externa de hombro",
                "Ejercicio de movilidad y fortalecimiento del hombro.",
                "Mantener el codo junto al cuerpo durante el movimiento.",
                "Excéntrico",
                "Hombro",
                "Estabilidad",
                "Intermedio");

            Agregar(
                "Retracción escapular",
                "Ejercicio para mejorar el control escapular.",
                "Llevar suavemente los omóplatos hacia atrás.",
                "Isométrico",
                "Espalda alta",
                "Estabilidad",
                "Inicial (agudo)");

            Agregar(
                "Estiramiento lumbar",
                "Movilidad suave de la región lumbar.",
                "Mantener una respiración relajada durante el estiramiento.",
                "Isométrico",
                "Lumbar",
                "Flexibilidad",
                "Inicial (agudo)");

            Agregar(
                "Estiramiento de isquiotibiales",
                "Estiramiento de la musculatura posterior del muslo.",
                "Mantener el estiramiento sin provocar dolor.",
                "Isométrico",
                "Cadera",
                "Flexibilidad",
                "Inicial (agudo)");

            Agregar(
                "Plancha isométrica",
                "Ejercicio de estabilización del tronco.",
                "Mantener el cuerpo alineado durante todo el ejercicio.",
                "Isométrico",
                "Lumbar",
                "Estabilidad",
                "Avanzado");

            Agregar(
                "Elevación de pierna recta",
                "Fortalecimiento de miembros inferiores.",
                "Mantener la rodilla extendida durante la elevación.",
                "Concéntrico",
                "Rodilla",
                "Fuerza",
                "Inicial (agudo)");

            if (nuevos.Count > 0)
            {
                _context.Ejercicios.AddRange(nuevos);

                await _context.SaveChangesAsync();
            }
        }
    }
}



