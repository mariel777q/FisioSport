using FISIOSPORT.Data;
using FISIOSPORT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;

namespace FISIOSPORT.Controllers
{
    [Authorize(Roles = "Fisioterapeuta")]
    public class FisioterapeutaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FisioterapeutaController> _logger;

        public FisioterapeutaController(
            ApplicationDbContext context,
            ILogger<FisioterapeutaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int IdActual =>
            int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

        // =========================================================
        // EXCEPCIÓN DE VALIDACIÓN
        // =========================================================

        private class PreSaveValidationException : Exception
        {
            public List<(string Key, string Message)> Errors { get; }

            public PreSaveValidationException(
                List<(string Key, string Message)> errors)
            {
                Errors = errors;
            }
        }

        // =========================================================
        // NORMALIZAR DATETIME PARA POSTGRESQL
        // =========================================================

        private void NormalizarDateTimesPendientes()
        {
            var entries = _context.ChangeTracker
                .Entries()
                .Where(e =>
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified)
                .ToList();

            foreach (var entry in entries)
            {
                foreach (var property in entry.Properties)
                {
                    var clrType = property.Metadata.ClrType;

                    if (clrType != typeof(DateTime) &&
                        clrType != typeof(DateTime?))
                    {
                        continue;
                    }

                    if (property.CurrentValue is not DateTime fecha)
                        continue;

                    var columnType =
                        property.Metadata.GetColumnType();

                    if (string.IsNullOrWhiteSpace(columnType))
                        continue;

                    // timestamp with time zone
                    if (columnType.Contains(
                            "timestamp with time zone",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        if (fecha.Kind == DateTimeKind.Utc)
                        {
                            continue;
                        }

                        if (fecha.Kind == DateTimeKind.Local)
                        {
                            property.CurrentValue =
                                fecha.ToUniversalTime();
                        }
                        else
                        {
                            property.CurrentValue =
                                DateTime.SpecifyKind(
                                    fecha,
                                    DateTimeKind.Local
                                ).ToUniversalTime();
                        }
                    }

                    // timestamp without time zone
                    else if (columnType.Contains(
                                 "timestamp without time zone",
                                 StringComparison.OrdinalIgnoreCase))
                    {
                        property.CurrentValue =
                            DateTime.SpecifyKind(
                                fecha,
                                DateTimeKind.Unspecified
                            );
                    }
                }
            }
        }

        // =========================================================
        // VALIDACIÓN ANTES DE GUARDAR
        // =========================================================

        private List<(string Key, string Message)>
            ValidatePendingEntities()
        {
            var errors =
                new List<(string Key, string Message)>();

            NormalizarDateTimesPendientes();

            var entries =
                _context.ChangeTracker
                    .Entries()
                    .Where(e =>
                        e.State == EntityState.Added ||
                        e.State == EntityState.Modified)
                    .ToList();

            foreach (var entry in entries)
            {
                var entity = entry.Entity;

                if (entity == null)
                    continue;

                var type = entity.GetType();

                var efType =
                    _context.Model.FindEntityType(type);

                foreach (
                    var prop in type.GetProperties(
                        BindingFlags.Public |
                        BindingFlags.Instance))
                {
                    var propName = prop.Name;
                    var propType = prop.PropertyType;
                    var val = prop.GetValue(entity);

                    // Nunca mostrar contraseñas
                    // dentro de mensajes de error.
                    if (propName.Contains(
                            "password",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var requiredAttr =
                        prop.GetCustomAttribute<RequiredAttribute>();

                    var efProp =
                        efType?.FindProperty(propName);

                    var isEfRequired =
                        efProp != null &&
                        efProp.IsNullable == false;

                    // =================================================
                    // REQUIRED / NOT NULL
                    // =================================================

                    if (
                        (requiredAttr != null ||
                         isEfRequired) &&
                        (
                            val == null ||
                            (
                                propType == typeof(string) &&
                                string.IsNullOrWhiteSpace(
                                    val as string
                                )
                            )
                        )
                    )
                    {
                        errors.Add(
                            (
                                propName,
                                $"{propName}: valor nulo o vacío " +
                                "pero es requerido en la base de datos."
                            )
                        );

                        continue;
                    }

                    // =================================================
                    // LONGITUD DE STRINGS
                    // =================================================

                    if (
                        propType == typeof(string) &&
                        val is string stringValue &&
                        !string.IsNullOrEmpty(stringValue)
                    )
                    {
                        int? maxLength = null;

                        var maxAttr =
                            prop.GetCustomAttribute<
                                MaxLengthAttribute>();

                        if (
                            maxAttr != null &&
                            maxAttr.Length > 0)
                        {
                            maxLength = maxAttr.Length;
                        }

                        var stringLengthAttr =
                            prop.GetCustomAttribute<
                                StringLengthAttribute>();

                        if (
                            stringLengthAttr != null &&
                            stringLengthAttr.MaximumLength > 0)
                        {
                            maxLength =
                                maxLength ??
                                stringLengthAttr.MaximumLength;
                        }

                        if (
                            maxLength == null &&
                            efProp != null)
                        {
                            maxLength =
                                efProp.GetMaxLength();
                        }

                        if (
                            maxLength.HasValue &&
                            stringValue.Length >
                            maxLength.Value)
                        {
                            var sample =
                                stringValue.Length > 200
                                    ? stringValue.Substring(
                                        0,
                                        200) + "..."
                                    : stringValue;

                            errors.Add(
                                (
                                    propName,
                                    $"{propName}: el contenido tiene " +
                                    $"{stringValue.Length} caracteres y " +
                                    $"el máximo permitido es " +
                                    $"{maxLength.Value}. " +
                                    $"Inicio: {sample}"
                                )
                            );
                        }
                    }

                    // =================================================
                    // VALIDACIÓN DE DATETIME
                    // =================================================

                    if (
                        (
                            propType == typeof(DateTime) ||
                            propType == typeof(DateTime?)
                        ) &&
                        val is DateTime fecha
                    )
                    {
                        var columnType =
                            efProp?.GetColumnType();

                        if (
                            columnType != null &&
                            columnType.Contains(
                                "timestamp with time zone",
                                StringComparison.OrdinalIgnoreCase) &&
                            fecha.Kind != DateTimeKind.Utc
                        )
                        {
                            errors.Add(
                                (
                                    propName,
                                    $"{propName}: la columna usa " +
                                    "timestamp with time zone y " +
                                    "el DateTime debe ser UTC."
                                )
                            );
                        }

                        if (
                            columnType != null &&
                            columnType.Contains(
                                "timestamp without time zone",
                                StringComparison.OrdinalIgnoreCase) &&
                            fecha.Kind != DateTimeKind.Unspecified
                        )
                        {
                            errors.Add(
                                (
                                    propName,
                                    $"{propName}: la columna usa " +
                                    "timestamp without time zone y " +
                                    "el DateTime debe tener Kind " +
                                    "Unspecified."
                                )
                            );
                        }
                    }
                }
            }

            return errors;
        }

        // =========================================================
        // DASHBOARD
        // =========================================================

        public async Task<IActionResult> Dashboard()
        {
            var fisio =
                await _context.Fisioterapeutas
                    .FirstOrDefaultAsync(
                        f => f.Id == IdActual);

            if (fisio == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var hoy = DateTime.Today;

            var citas =
                await _context.Citas
                    .Include(c => c.Paciente)
                    .Where(c =>
                        c.FisioterapeutaId == IdActual &&
                        c.Fecha >= hoy &&
                        c.Estado != "Completada" &&
                        c.Estado != "Cancelada")
                    .OrderBy(c => c.Fecha)
                    .ThenBy(c => c.Hora)
                    .ToListAsync();

            var modelo =
                new DashboardFisioViewModel
                {
                    Nombre = fisio.Nombre,

                    Especialidad =
                        fisio.Especialidad,

                    NumeroColegiado =
                        fisio.NumeroColegiado,

                    Citas =
                        citas.Select(c =>
                            new CitaCardViewModel
                            {
                                Id = c.Id,

                                NombrePaciente =
                                    c.Paciente?.Nombre ??
                                    "Paciente",

                                Fecha =
                                    c.Fecha,

                                Hora =
                                    c.Hora,

                                Estado =
                                    c.Estado
                            })
                        .ToList()
                };

            return View(modelo);
        }

        // =========================================================
        // NUEVO PACIENTE - GET
        // =========================================================

        [HttpGet]
        public IActionResult CrearPaciente()
        {
            return View(
                "RegistrarPaciente",
                new CrearPacienteViewModel()
            );
        }

        // =========================================================
        // NUEVO PACIENTE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearPaciente(
            CrearPacienteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(
                    "RegistrarPaciente",
                    model);
            }

            var correoExiste =
                await _context.Pacientes
                    .AnyAsync(p =>
                        p.Correo.ToLower() ==
                        model.Correo.ToLower());

            if (correoExiste)
            {
                ModelState.AddModelError(
                    "Correo",
                    "Ya existe un paciente registrado con ese correo."
                );

                return View(
                    "RegistrarPaciente",
                    model);
            }

            var strategy =
                _context.Database
                    .CreateExecutionStrategy();

            string pacienteNombre = string.Empty;
            string expediente = string.Empty;

            int pacienteId = 0;
            int sesionesCount = 0;

            try
            {
                await strategy.ExecuteAsync(
                    async () =>
                    {
                        await using var transaction =
                            await _context.Database
                                .BeginTransactionAsync();

                        try
                        {
                            // =================================================
                            // PACIENTE
                            // =================================================

                            var paciente =
                                new Paciente
                                {
                                    Nombre =
                                        model.Nombre.Trim(),

                                    Correo =
                                        model.Correo.Trim(),

                                    PasswordHash =
                                        BCrypt.Net.BCrypt
                                            .HashPassword(
                                                model.Password),

                                    Telefono =
                                        model.Telefono.Trim(),

                                    ContactoEmergencia =
                                        model.ContactoEmergencia.Trim(),

                                    Domicilio =
                                        model.Domicilio.Trim(),

                                    Ocupacion =
                                        model.Ocupacion.Trim(),

                                    Sexo =
                                        model.Sexo,

                                    Edad =
                                        model.Edad!.Value,

                                    EstadoCivil =
                                        model.EstadoCivil,

                                    Escolaridad =
                                        model.Escolaridad,

                                    Peso =
                                        model.Peso!.Value,

                                    Talla =
                                        model.Talla!.Value,

                                    Estatura =
                                        model.Estatura!.Value,

                                    Etnia =
                                        model.Etnia.Trim()
                                };

                            _context.Pacientes.Add(
                                paciente);

                            var errors =
                                ValidatePendingEntities();

                            if (errors.Any())
                            {
                                throw new PreSaveValidationException(
                                    errors);
                            }

                            await _context.SaveChangesAsync();

                            pacienteId =
                                paciente.Id;

                            pacienteNombre =
                                paciente.Nombre;

                            // =================================================
                            // EXPEDIENTE
                            // =================================================

                            expediente =
                                $"FS-{paciente.Id:000000}";

                            // =================================================
                            // ANTECEDENTES
                            // =================================================

                            var antecedentesJson =
                                JsonSerializer.Serialize(
                                    new Dictionary<string, string>
                                    {
                                        ["Expediente"] =
                                            expediente,

                                        ["Diabetes"] =
                                            SiNo(
                                                model.Diabetes),

                                        ["EspecificacionDiabetes"] =
                                            model.EspecificacionDiabetes,

                                        ["Alergia"] =
                                            SiNo(
                                                model.Alergia),

                                        ["EspecificacionAlergia"] =
                                            model.EspecificacionAlergia,

                                        ["HTA"] =
                                            SiNo(
                                                model.HTA),

                                        ["EspecificacionHTA"] =
                                            model.EspecificacionHTA,

                                        ["Cancer"] =
                                            SiNo(
                                                model.Cancer),

                                        ["EspecificacionCancer"] =
                                            model.EspecificacionCancer,

                                        ["Transfusiones"] =
                                            SiNo(
                                                model.Transfusiones),

                                        ["EspecificacionTransfusiones"] =
                                            model.EspecificacionTransfusiones,

                                        ["EnfermedadesReumaticas"] =
                                            SiNo(
                                                model.EnfermedadesReumaticas),

                                        ["EspecificacionEnfermedadesReumaticas"] =
                                            model.EspecificacionEnfermedadesReumaticas,

                                        ["Encames"] =
                                            SiNo(
                                                model.Encames),

                                        ["EspecificacionEncames"] =
                                            model.EspecificacionEncames,

                                        ["Accidentes"] =
                                            SiNo(
                                                model.Accidentes),

                                        ["EspecificacionAccidentes"] =
                                            model.EspecificacionAccidentes,

                                        ["Cardiopatias"] =
                                            SiNo(
                                                model.Cardiopatias),

                                        ["EspecificacionCardiopatias"] =
                                            model.EspecificacionCardiopatias,

                                        ["Cirugias"] =
                                            SiNo(
                                                model.Cirugias),

                                        ["EspecificacionCirugias"] =
                                            model.EspecificacionCirugias,

                                        ["Fracturas"] =
                                            SiNo(
                                                model.Fracturas),

                                        ["TipoFractura"] =
                                            model.TipoFractura,

                                        ["Tabaquismo"] =
                                            SiNo(
                                                model.Tabaquismo),

                                        ["Alcoholismo"] =
                                            SiNo(
                                                model.Alcoholismo),

                                        ["Drogas"] =
                                            SiNo(
                                                model.Drogas),

                                        ["ActividadFisica"] =
                                            SiNo(
                                                model.ActividadFisica),

                                        ["SeAutomedica"] =
                                            SiNo(
                                                model.SeAutomedica),

                                        ["Pasatiempo"] =
                                            model.Pasatiempo,

                                        ["EstaEmbarazada"] =
                                            SiNo(
                                                model.EstaEmbarazada),

                                        ["NumeroHijos"] =
                                            model.NumeroHijos!
                                                .Value
                                                .ToString(),

                                        ["AntecedentesGenerales"] =
                                            model.Antecedentes
                                    }
                                );

                            // =================================================
                            // DATOS FUNCIONALES
                            // =================================================

                            var funcionalJson =
                                JsonSerializer.Serialize(
                                    new Dictionary<string, string>
                                    {
                                        ["Peso"] =
                                            model.Peso!
                                                .Value
                                                .ToString("0.##"),

                                        ["Talla"] =
                                            model.Talla!
                                                .Value
                                                .ToString("0.##"),

                                        ["Estatura"] =
                                            model.Estatura!
                                                .Value
                                                .ToString("0.##"),

                                        ["IMC"] =
                                            model.IMC!
                                                .Value
                                                .ToString("0.##"),

                                        ["Etnia"] =
                                            model.Etnia,

                                        ["SitioCicatriz"] =
                                            model.SitioCicatriz,

                                        ["CicatrizQueloide"] =
                                            SiNo(
                                                model.CicatrizQueloide),

                                        ["CicatrizRetractil"] =
                                            SiNo(
                                                model.CicatrizRetractil),

                                        ["CicatrizAbierta"] =
                                            SiNo(
                                                model.CicatrizAbierta),

                                        ["CicatrizConAdherencia"] =
                                            SiNo(
                                                model.CicatrizConAdherencia),

                                        ["CicatrizHipertrofica"] =
                                            SiNo(
                                                model.CicatrizHipertrofica),

                                        ["TrasladosValorInicial"] =
                                            model.TrasladosValorInicial,

                                        ["TrasladosValorFinal"] =
                                            model.TrasladosValorFinal,

                                        ["TrasladoIndependiente"] =
                                            SiNo(
                                                model.TrasladoIndependiente),

                                        ["TrasladoSillaRuedas"] =
                                            SiNo(
                                                model.TrasladoSillaRuedas),

                                        ["TrasladoConAyudas"] =
                                            SiNo(
                                                model.TrasladoConAyudas),

                                        ["TrasladoCamilla"] =
                                            SiNo(
                                                model.TrasladoCamilla),

                                        ["MarchaLibre"] =
                                            SiNo(
                                                model.MarchaLibre),

                                        ["MarchaClaudicante"] =
                                            SiNo(
                                                model.MarchaClaudicante),

                                        ["MarchaConAyuda"] =
                                            SiNo(
                                                model.MarchaConAyuda),

                                        ["MarchaEspastica"] =
                                            SiNo(
                                                model.MarchaEspastica),

                                        ["MarchaAtaxica"] =
                                            SiNo(
                                                model.MarchaAtaxica),

                                        ["MarchaOtros"] =
                                            SiNo(
                                                model.MarchaOtros),

                                        ["ObservacionesMarcha"] =
                                            model.ObservacionesMarcha
                                    }
                                );

                            // =================================================
                            // HISTORIA CLÍNICA
                            // =================================================

                            var consulta =
                                new ConsultaClinica
                                {
                                    PacienteId =
                                        paciente.Id,

                                    FisioterapeutaId =
                                        IdActual,

                                    FechaConsulta =
                                        DateTime.UtcNow,

                                    Antecedentes =
                                        antecedentesJson,

                                    Alergias =
                                        model.Alergias,

                                    MotivoConsulta =
                                        model.MotivoConsulta,

                                    Observaciones =
                                        model.Observaciones,

                                    TratamientosPrevios =
                                        model.TratamientosPrevios,

                                    DiagnosticoMedico =
                                        model.DiagnosticoMedicoRehabilitacion,

                                    Reflejos =
                                        model.Reflejos,

                                    Sensibilidad =
                                        model.Sensibilidad,

                                    LenguajeOrientacion =
                                        model.LenguajeOrientacion,

                                    OtrosHallazgos =
                                        funcionalJson,

                                    SignosVitales =
                                        $"T/A: {model.TA} | " +
                                        $"TEMP: {model.Temperatura:0.##} °C | " +
                                        $"FC: {model.FC} lpm | " +
                                        $"FR: {model.FR} rpm",

                                    EspasmosContractura =
                                        model.EspasmosContracturaMuscular,

                                    DiagnosticoRehabilitacion =
                                        model.DiagnosticoMedicoRehabilitacion,

                                    CicatrizQuirurgica =
                                        $"Sitio: {model.SitioCicatriz}",

                                    Movilidad =
                                        $"Inicial: {model.TrasladosValorInicial} | " +
                                        $"Final: {model.TrasladosValorFinal}",

                                    Marcha =
                                        model.ObservacionesMarcha,

                                    EscalaDolor =
                                        model.EscalaDolor!.Value
                                };

                            _context.ConsultasClinicas.Add(
                                consulta);

                            // =================================================
                            // TRATAMIENTO
                            // =================================================

                            var tratamiento =
                                new Tratamiento
                                {
                                    PacienteId =
                                        paciente.Id,

                                    FisioterapeutaId =
                                        IdActual,

                                    MotivoConsulta =
                                        model.MotivoConsulta.Length > 250
                                            ? model.MotivoConsulta[..250]
                                            : model.MotivoConsulta,

                                    Diagnostico =
                                        model.DiagnosticoMedicoRehabilitacion
                                            .Length > 250
                                            ? model.DiagnosticoMedicoRehabilitacion
                                                [..250]
                                            : model.DiagnosticoMedicoRehabilitacion,

                                    Descripcion =
                                        "Plan de rehabilitación fisioterapéutica inicial.",

                                    TotalSesiones =
                                        model.TotalSesiones!.Value,

                                    FechaInicio =
                                        DateTime.UtcNow.Date,

                                    Estado =
                                        "Activo"
                                };

                            _context.Tratamientos.Add(
                                tratamiento);

                            var errors2 =
                                ValidatePendingEntities();

                            if (errors2.Any())
                            {
                                throw new PreSaveValidationException(
                                    errors2);
                            }

                            await _context.SaveChangesAsync();

                            // =================================================
                            // SESIONES
                            // =================================================

                            for (
                                int numeroSesion = 1;
                                numeroSesion <=
                                tratamiento.TotalSesiones;
                                numeroSesion++)
                            {
                                _context.SesionesTratamiento.Add(
                                    new SesionTratamiento
                                    {
                                        TratamientoId =
                                            tratamiento.Id,

                                        NumeroSesion =
                                            numeroSesion,

                                        TrabajoPlanificado =
                                            $"Sesión {numeroSesion} del tratamiento.",

                                        Observaciones =
                                            "Sin observaciones",

                                        Estado =
                                            "Pendiente",

                                        FechaCompletada =
                                            null
                                    }
                                );
                            }

                            var errors3 =
                                ValidatePendingEntities();

                            if (errors3.Any())
                            {
                                throw new PreSaveValidationException(
                                    errors3);
                            }

                            await _context.SaveChangesAsync();

                            sesionesCount =
                                tratamiento.TotalSesiones;

                            await transaction.CommitAsync();
                        }
                        catch (PreSaveValidationException pvex)
                        {
                            foreach (
                                var (key, message)
                                in pvex.Errors)
                            {
                                ModelState.AddModelError(
                                    key,
                                    message);
                            }

                            await transaction.RollbackAsync();

                            throw;
                        }
                        catch
                        {
                            await transaction.RollbackAsync();

                            throw;
                        }
                    });

                TempData["MensajeExito"] =
                    $"Paciente {pacienteNombre} registrado correctamente. " +
                    $"Expediente {expediente} creado con " +
                    $"{sesionesCount} sesiones.";

                return RedirectToAction(
                    nameof(CrearCita),
                    new
                    {
                        pacienteId = pacienteId
                    }
                );
            }
            catch (Exception ex)
            {
                try
                {
                    _logger.LogError(
                        ex,
                        "Error al guardar paciente");
                }
                catch
                {
                }

                var message =
                    ex.ToString();

                if (
                    ex is DbUpdateException &&
                    ex.InnerException != null)
                {
                    message =
                        ex.InnerException.ToString();
                }

                ModelState.AddModelError(
                    "",
                    message);

                return View(
                    "RegistrarPaciente",
                    model);
            }
        }

        // =========================================================
        // RUTA ANTIGUA
        // =========================================================

        [HttpGet]
        public IActionResult RegistrarPaciente()
        {
            return RedirectToAction(
                nameof(Dashboard));
        }

        // =========================================================
        // NUEVA CITA - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> CrearCita(
            int? pacienteId)
        {
            var pacientes =
                await _context.Pacientes
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

            var modelo =
                new CrearCitaViewModel
                {
                    PacientesDisponibles =
                        pacientes,

                    PacienteId =
                        pacienteId ?? 0,

                    Fecha =
                        DateTime.Today
                };

            if (pacienteId.HasValue)
            {
                await CargarHistoriaPaciente(
                    modelo,
                    pacienteId.Value);
            }

            return View(modelo);
        }

        // =========================================================
        // NUEVA CITA - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCita(
            CrearCitaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.PacientesDisponibles =
                    await _context.Pacientes
                        .OrderBy(p => p.Nombre)
                        .ToListAsync();

                await CargarHistoriaPaciente(
                    model,
                    model.PacienteId);

                return View(model);
            }

            var paciente =
                await _context.Pacientes
                    .FirstOrDefaultAsync(
                        p => p.Id == model.PacienteId);

            if (paciente == null)
            {
                ModelState.AddModelError(
                    "PacienteId",
                    "Paciente no encontrado.");

                model.PacientesDisponibles =
                    await _context.Pacientes
                        .OrderBy(p => p.Nombre)
                        .ToListAsync();

                return View(model);
            }

            // =================================================
            // SIGUIENTE SESIÓN PENDIENTE
            // =================================================

            var siguienteSesion =
                await _context.SesionesTratamiento
                    .Include(s => s.Tratamiento)
                    .Where(s =>
                        s.Tratamiento!.PacienteId ==
                        paciente.Id &&

                        s.Tratamiento.FisioterapeutaId ==
                        IdActual &&

                        s.Estado != "Completada" &&

                        !_context.Citas.Any(c =>
                            c.SesionTratamientoId ==
                            s.Id &&

                            c.Estado != "Completada" &&

                            c.Estado != "Cancelada")
                    )
                    .OrderBy(s => s.TratamientoId)
                    .ThenBy(s => s.NumeroSesion)
                    .FirstOrDefaultAsync();

            var cita =
                new Cita
                {
                    FisioterapeutaId =
                        IdActual,

                    PacienteId =
                        paciente.Id,

                    SesionTratamientoId =
                        siguienteSesion?.Id,

                    Fecha =
                        model.Fecha,

                    Hora =
                        model.Hora,

                    Estado =
                        "Pendiente",

                    Notas =
                        model.Notas?.Trim() ??
                        string.Empty
                };

            _context.Citas.Add(cita);

            NormalizarDateTimesPendientes();

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                siguienteSesion != null
                    ? $"Cita creada correctamente para {paciente.Nombre}. " +
                      $"Corresponde a la sesión " +
                      $"{siguienteSesion.NumeroSesion}."
                    : $"Cita creada correctamente para {paciente.Nombre}.";

            return RedirectToAction(
                nameof(Dashboard));
        }

        // =========================================================
        // DETALLE DE CITA
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> DetalleCita(
            int id)
        {
            var cita =
                await _context.Citas
                    .Include(c => c.Paciente)
                    .Include(c => c.SesionTratamiento)
                    .FirstOrDefaultAsync(c =>
                        c.Id == id &&
                        c.FisioterapeutaId == IdActual);

            if (cita == null)
                return NotFound();

            var consulta =
                await _context.ConsultasClinicas
                    .Include(c => c.Fisioterapeuta)
                    .Where(c =>
                        c.PacienteId ==
                        cita.PacienteId &&

                        c.FisioterapeutaId ==
                        IdActual)
                    .OrderByDescending(
                        c => c.FechaConsulta)
                    .FirstOrDefaultAsync();

            Tratamiento? tratamiento = null;

            if (cita.SesionTratamiento != null)
            {
                tratamiento =
                    await _context.Tratamientos
                        .Include(t => t.Sesiones)
                        .FirstOrDefaultAsync(t =>
                            t.Id ==
                            cita.SesionTratamiento
                                .TratamientoId);
            }

            if (tratamiento == null)
            {
                tratamiento =
                    await _context.Tratamientos
                        .Include(t => t.Sesiones)
                        .Where(t =>
                            t.PacienteId ==
                            cita.PacienteId &&

                            t.FisioterapeutaId ==
                            IdActual &&

                            t.Estado ==
                            "Activo")
                        .OrderByDescending(
                            t => t.FechaInicio)
                        .FirstOrDefaultAsync();
            }

            var modelo =
                new DetalleCitaViewModel
                {
                    CitaId =
                        cita.Id,

                    NombrePaciente =
                        cita.Paciente?.Nombre ??
                        "Paciente",

                    CorreoPaciente =
                        cita.Paciente?.Correo ??
                        string.Empty,

                    TelefonoPaciente =
                        cita.Paciente?.Telefono ??
                        string.Empty,

                    Fecha =
                        cita.Fecha,

                    Hora =
                        cita.Hora,

                    EstadoCita =
                        cita.Estado,

                    Historia =
                        consulta != null &&
                        cita.Paciente != null
                            ? ConstruirHistoria(
                                cita.Paciente,
                                consulta)
                            : new(),

                    FechaHistoria =
                        consulta?.FechaConsulta,

                    Expediente =
                        cita.Paciente != null
                            ? $"FS-{cita.Paciente.Id:000000}"
                            : string.Empty
                };

            if (consulta != null)
            {
                modelo.MotivoConsulta =
                    consulta.MotivoConsulta;

                modelo.Diagnostico =
                    consulta.DiagnosticoRehabilitacion;

                modelo.FechaHistoria =
                    consulta.FechaConsulta;
            }

            if (tratamiento != null)
            {
                modelo.DescripcionTratamiento =
                    tratamiento.Descripcion;

                modelo.TotalSesiones =
                    tratamiento.TotalSesiones;

                var sesiones =
                    tratamiento.Sesiones
                        .OrderBy(s =>
                            s.NumeroSesion)
                        .ToList();

                modelo.Sesiones =
                    sesiones
                        .Select(s =>
                            new SesionDetalleViewModel
                            {
                                Id =
                                    s.Id,

                                NumeroSesion =
                                    s.NumeroSesion,

                                TrabajoPlanificado =
                                    s.TrabajoPlanificado,

                                Observaciones =
                                    s.Observaciones,

                                FechaCompletada =
                                    s.FechaCompletada,

                                Estado =
                                    s.Estado,

                                EstaCompletada =
                                    s.Estado ==
                                    "Completada",

                                EsSesionActual =
                                    cita.SesionTratamientoId ==
                                    s.Id
                            })
                        .ToList();

                SesionTratamiento? siguiente = null;

                // Primero: sesión vinculada a la cita.
                if (cita.SesionTratamientoId.HasValue)
                {
                    siguiente =
                        sesiones.FirstOrDefault(
                            s =>
                                s.Id ==
                                cita.SesionTratamientoId.Value);
                }

                // Si no existe, primera pendiente.
                if (siguiente == null)
                {
                    siguiente =
                        sesiones.FirstOrDefault(
                            s =>
                                s.Estado !=
                                "Completada");
                }

                modelo.SesionesCompletadas =
                    sesiones.Count(
                        s =>
                            s.Estado ==
                            "Completada");

                modelo.SesionesRestantes =
                    Math.Max(
                        0,
                        tratamiento.TotalSesiones -
                        modelo.SesionesCompletadas);

                modelo.PorcentajeProgreso =
                    tratamiento.TotalSesiones > 0
                        ? (int)Math.Round(
                            modelo.SesionesCompletadas *
                            100.0 /
                            tratamiento.TotalSesiones)
                        : 0;

                if (siguiente != null)
                {
                    modelo.SesionActualId =
                        siguiente.Id;

                    modelo.NumeroSesionActual =
                        siguiente.NumeroSesion;

                    modelo.TrabajoSesionActual =
                        siguiente.TrabajoPlanificado;

                    modelo.ObservacionesSesion =
                        siguiente.Observaciones;
                }
            }

            return View(modelo);
        }

        // =========================================================
        // COMPLETAR SESIÓN
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletarSesion(
            int sesionId,
            int citaId,
            string? observaciones)
        {
            var cita =
                await _context.Citas
                    .FirstOrDefaultAsync(c =>
                        c.Id == citaId &&
                        c.FisioterapeutaId == IdActual);

            if (cita == null)
                return NotFound();

            var sesion =
                await _context.SesionesTratamiento
                    .Include(s => s.Tratamiento)
                    .FirstOrDefaultAsync(s =>
                        s.Id == sesionId &&
                        s.Tratamiento!.PacienteId ==
                            cita.PacienteId &&
                        s.Tratamiento.FisioterapeutaId ==
                            IdActual);

            if (sesion == null)
                return NotFound();

            if (sesion.Estado == "Completada")
            {
                TempData["MensajeExito"] =
                    "Esta sesión ya estaba completada.";

                return RedirectToAction(
                    nameof(Dashboard));
            }

            // =================================================
            // OBSERVACIONES
            // =================================================

            if (string.IsNullOrWhiteSpace(observaciones))
            {
                sesion.Observaciones =
                    "Sin observaciones";
            }
            else
            {
                sesion.Observaciones =
                    observaciones.Trim();
            }

            // =================================================
            // ESTADO DE LA SESIÓN
            // =================================================

            sesion.Estado =
                "Completada";

            // =================================================
            // FECHA DE COMPLETADO
            // =================================================
            //
            // Esta columna es timestamp without time zone.
            // Por eso usamos Unspecified.
            // =================================================

            sesion.FechaCompletada =
                DateTime.SpecifyKind(
                    DateTime.Now,
                    DateTimeKind.Unspecified
                );

            // =================================================
            // ESTADO DE LA CITA
            // =================================================

            cita.Estado =
                "Completada";

            // =================================================
            // COMPROBAR SI TERMINÓ EL TRATAMIENTO
            // =================================================

            if (sesion.Tratamiento != null)
            {
                var pendientes =
                    await _context.SesionesTratamiento
                        .CountAsync(s =>
                            s.TratamientoId ==
                            sesion.Tratamiento.Id &&

                            s.Estado !=
                            "Completada");

                if (pendientes == 0)
                {
                    sesion.Tratamiento.Estado =
                        "Completado";
                }
            }

            // =================================================
            // NORMALIZAR DATETIME
            // =================================================

            NormalizarDateTimesPendientes();

            // =================================================
            // VALIDAR
            // =================================================

            var errors =
                ValidatePendingEntities();

            if (errors.Any())
            {
                foreach (
                    var (key, message)
                    in errors)
                {
                    ModelState.AddModelError(
                        key,
                        message);
                }

                TempData["MensajeError"] =
                    "No se pudo completar la sesión porque " +
                    "hay datos que no cumplen las reglas de la base de datos.";

                return RedirectToAction(
                    nameof(DetalleCita),
                    new
                    {
                        id = citaId
                    });
            }

            // =================================================
            // GUARDAR
            // =================================================

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] =
                "Sesión completada con éxito.";

            return RedirectToAction(
                nameof(Dashboard));
        }

        // =========================================================
        // DESCARGAR HISTORIA CLÍNICA PDF
        // =========================================================

        [HttpGet]
        public async Task<IActionResult>
            DescargarHistoriaClinica(
                int pacienteId)
        {
            var paciente =
                await _context.Pacientes
                    .FirstOrDefaultAsync(
                        p => p.Id == pacienteId);

            if (paciente == null)
                return NotFound();

            var consulta =
                await _context.ConsultasClinicas
                    .Include(c => c.Fisioterapeuta)
                    .Where(c =>
                        c.PacienteId ==
                        pacienteId &&

                        c.FisioterapeutaId ==
                        IdActual)
                    .OrderByDescending(
                        c => c.FechaConsulta)
                    .FirstOrDefaultAsync();

            if (consulta == null)
                return NotFound();

            var historia =
                ConstruirHistoria(
                    paciente,
                    consulta);

            var etiquetas =
                new Dictionary<string, string>
                {
                    ["Nombre"] = "Nombre",
                    ["Domicilio"] = "Domicilio",
                    ["Telefono"] = "Teléfono",
                    ["Ocupacion"] = "Ocupación",
                    ["Sexo"] = "Sexo",
                    ["Edad"] = "Edad",
                    ["EstadoCivil"] = "Estado civil",
                    ["Escolaridad"] = "Escolaridad",
                    ["Terapeuta"] = "Terapeuta",
                    ["Expediente"] = "Expediente #",
                    ["Fecha"] = "Fecha",

                    ["Peso"] = "Peso",
                    ["Talla"] = "Talla",
                    ["Estatura"] = "Estatura",
                    ["IMC"] = "IMC",
                    ["Etnia"] = "Etnia",

                    ["MotivoConsulta"] =
                        "Motivo de consulta",

                    ["TratamientosPrevios"] =
                        "Tratamientos previos",

                    ["Diabetes"] = "Diabetes",
                    ["Alergia"] = "Alergia",
                    ["HTA"] = "HTA",
                    ["Cancer"] = "Cáncer",

                    ["Transfusiones"] =
                        "Transfusiones",

                    ["EnfermedadesReumaticas"] =
                        "Enfermedades reumáticas",

                    ["Encames"] = "Encames",
                    ["Accidentes"] = "Accidentes",

                    ["Cardiopatias"] =
                        "Cardiopatías",

                    ["Cirugias"] = "Cirugías",
                    ["Fracturas"] = "Fracturas",

                    ["TA"] = "T/A",
                    ["Temperatura"] = "Temperatura",
                    ["FC"] = "FC",
                    ["FR"] = "FR",

                    ["EspasmosContractura"] =
                        "Espasmos / contractura muscular",

                    ["Tabaquismo"] = "Tabaquismo",
                    ["Alcoholismo"] = "Alcoholismo",
                    ["Drogas"] = "Drogas",

                    ["ActividadFisica"] =
                        "Actividad física",

                    ["SeAutomedica"] =
                        "Se automedica",

                    ["Pasatiempo"] =
                        "Pasatiempo",

                    ["EstaEmbarazada"] =
                        "Embarazo",

                    ["NumeroHijos"] =
                        "Número de hijos",

                    ["Diagnostico"] =
                        "Diagnóstico médico en rehabilitación",

                    ["Reflejos"] =
                        "Reflejos",

                    ["Sensibilidad"] =
                        "Sensibilidad",

                    ["LenguajeOrientacion"] =
                        "Lenguaje / orientación",

                    ["OtrosDiagnostico"] =
                        "Otros",

                    ["SitioCicatriz"] =
                        "Sitio de cicatriz",

                    ["CicatrizQueloide"] =
                        "Queloide",

                    ["CicatrizRetractil"] =
                        "Retráctil",

                    ["CicatrizAbierta"] =
                        "Abierta",

                    ["CicatrizConAdherencia"] =
                        "Con adherencia",

                    ["CicatrizHipertrofica"] =
                        "Hipertrófica",

                    ["TrasladosValorInicial"] =
                        "Traslados - valoración inicial",

                    ["TrasladosValorFinal"] =
                        "Traslados - valoración final",

                    ["TrasladoIndependiente"] =
                        "Independiente",

                    ["TrasladoSillaRuedas"] =
                        "Silla de ruedas",

                    ["TrasladoConAyudas"] =
                        "Con ayudas",

                    ["TrasladoCamilla"] =
                        "Camilla",

                    ["MarchaLibre"] =
                        "Marcha libre",

                    ["MarchaClaudicante"] =
                        "Marcha claudicante",

                    ["MarchaConAyuda"] =
                        "Marcha con ayuda",

                    ["MarchaEspastica"] =
                        "Marcha espástica",

                    ["MarchaAtaxica"] =
                        "Marcha atáxica",

                    ["MarchaOtros"] =
                        "Otros tipos de marcha",

                    ["ObservacionesMarcha"] =
                        "Observaciones de marcha",

                    ["EscalaDolor"] =
                        "Escala del dolor",

                    ["AntecedentesGenerales"] =
                        "Antecedentes generales",

                    ["Alergias"] =
                        "Alergias",

                    ["ObservacionesGenerales"] =
                        "Observaciones clínicas"
                };

            var secciones =
                new List<(string Titulo, string[] Claves)>
                {
                    (
                        "1. DATOS DEL PACIENTE",
                        new[]
                        {
                            "Nombre",
                            "Domicilio",
                            "Telefono",
                            "Ocupacion",
                            "Sexo",
                            "Edad",
                            "EstadoCivil",
                            "Escolaridad",
                            "Terapeuta",
                            "Expediente",
                            "Fecha"
                        }
                    ),

                    (
                        "2. EXPLORACIÓN FÍSICA",
                        new[]
                        {
                            "Peso",
                            "Talla",
                            "Estatura",
                            "IMC",
                            "Etnia",
                            "MotivoConsulta",
                            "TratamientosPrevios"
                        }
                    ),

                    (
                        "3. ANTECEDENTES PATOLÓGICOS Y HEREDOFAMILIARES",
                        new[]
                        {
                            "Diabetes",
                            "Alergia",
                            "HTA",
                            "Cancer",
                            "Transfusiones",
                            "EnfermedadesReumaticas",
                            "Encames",
                            "Accidentes",
                            "Cardiopatias",
                            "Cirugias",
                            "Fracturas",
                            "AntecedentesGenerales"
                        }
                    ),

                    (
                        "4. SIGNOS VITALES",
                        new[]
                        {
                            "TA",
                            "Temperatura",
                            "FC",
                            "FR",
                            "EspasmosContractura"
                        }
                    ),

                    (
                        "5. HÁBITOS Y MUJERES",
                        new[]
                        {
                            "Tabaquismo",
                            "Alcoholismo",
                            "Drogas",
                            "ActividadFisica",
                            "SeAutomedica",
                            "Pasatiempo",
                            "EstaEmbarazada",
                            "NumeroHijos"
                        }
                    ),

                    (
                        "6. DIAGNÓSTICO",
                        new[]
                        {
                            "Diagnostico",
                            "Reflejos",
                            "Sensibilidad",
                            "LenguajeOrientacion",
                            "OtrosDiagnostico"
                        }
                    ),

                    (
                        "7. CICATRIZ QUIRÚRGICA",
                        new[]
                        {
                            "SitioCicatriz",
                            "CicatrizQueloide",
                            "CicatrizRetractil",
                            "CicatrizAbierta",
                            "CicatrizConAdherencia",
                            "CicatrizHipertrofica"
                        }
                    ),

                    (
                        "8. TRASLADOS",
                        new[]
                        {
                            "TrasladosValorInicial",
                            "TrasladosValorFinal",
                            "TrasladoIndependiente",
                            "TrasladoSillaRuedas",
                            "TrasladoConAyudas",
                            "TrasladoCamilla"
                        }
                    ),

                    (
                        "9. MARCHA",
                        new[]
                        {
                            "MarchaLibre",
                            "MarchaClaudicante",
                            "MarchaConAyuda",
                            "MarchaEspastica",
                            "MarchaAtaxica",
                            "MarchaOtros",
                            "ObservacionesMarcha"
                        }
                    ),

                    (
                        "10. OBSERVACIONES",
                        new[]
                        {
                            "Alergias",
                            "ObservacionesGenerales"
                        }
                    )
                };

            var dolor =
                historia.TryGetValue(
                    "EscalaDolor",
                    out var dolorTexto) &&
                int.TryParse(
                    dolorTexto,
                    out var dolorNumero)
                    ? dolorNumero
                    : 0;

            var document =
                QuestPDF.Fluent.Document.Create(
                    document =>
                    {
                        document.Page(page =>
                        {
                            page.Size(
                                PageSizes.A4);

                            page.Margin(
                                1.4f,
                                Unit.Centimetre);

                            page.PageColor(
                                Colors.White);

                            page.DefaultTextStyle(
                                x => x.FontSize(8.5f));

                            page.Header()
                                .Column(column =>
                                {
                                    column.Item()
                                        .AlignCenter()
                                        .Text(
                                            "HISTORIA CLÍNICA FISIOTERAPIA")
                                        .Bold()
                                        .FontSize(18)
                                        .FontColor(
                                            Colors.Blue.Medium);

                                    column.Item()
                                        .PaddingTop(3)
                                        .AlignCenter()
                                        .Text(
                                            "FISIOSPORT")
                                        .SemiBold()
                                        .FontSize(9)
                                        .FontColor(
                                            Colors.Grey.Darken1);

                                    column.Item()
                                        .PaddingTop(8)
                                        .LineHorizontal(1)
                                        .LineColor(
                                            Colors.Blue.Medium);
                                });

                            page.Content()
                                .PaddingTop(12)
                                .Column(column =>
                                {
                                    column.Spacing(8);

                                    foreach (
                                        var seccion
                                        in secciones)
                                    {
                                        column.Item()
                                            .Background(
                                                Colors.Blue.Lighten5)
                                            .Border(1)
                                            .BorderColor(
                                                Colors.Blue.Lighten2)
                                            .Padding(6)
                                            .Text(
                                                seccion.Titulo)
                                            .Bold()
                                            .FontSize(9)
                                            .FontColor(
                                                Colors.Blue.Darken2);

                                        foreach (
                                            var clave
                                            in seccion.Claves)
                                        {
                                            if (!historia.TryGetValue(
                                                    clave,
                                                    out var valor))
                                            {
                                                continue;
                                            }

                                            if (string.IsNullOrWhiteSpace(
                                                    valor))
                                            {
                                                valor = "—";
                                            }

                                            column.Item()
                                                .PaddingVertical(2)
                                                .Row(row =>
                                                {
                                                    row.ConstantItem(145)
                                                        .Text(
                                                            etiquetas[clave])
                                                        .SemiBold();

                                                    row.RelativeItem()
                                                        .Text(valor);
                                                });
                                        }
                                    }

                                    column.Item()
                                        .PaddingTop(8)
                                        .Text(
                                            "ESCALA DEL DOLOR")
                                        .Bold()
                                        .FontSize(9);

                                    column.Item()
                                        .PaddingTop(5)
                                        .Row(row =>
                                        {
                                            for (
                                                int i = 0;
                                                i <= 10;
                                                i++)
                                            {
                                                var color =
                                                    i <= 2
                                                        ? "#16A34A"
                                                        : i <= 5
                                                            ? "#84CC16"
                                                            : i <= 7
                                                                ? "#F59E0B"
                                                                : i <= 8
                                                                    ? "#F97316"
                                                                    : "#DC2626";

                                                row.RelativeItem()
                                                    .Padding(1)
                                                    .Background(color)
                                                    .Padding(4)
                                                    .AlignCenter()
                                                    .Text(
                                                        i.ToString())
                                                    .Bold()
                                                    .FontColor(
                                                        Colors.White);
                                            }
                                        });

                                    column.Item()
                                        .PaddingTop(3)
                                        .AlignCenter()
                                        .Text(
                                            $"Dolor registrado: {dolor}/10")
                                        .Bold();
                                });

                            page.Footer()
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.Span(
                                        "FisioSport · Historia clínica · Página ");

                                    text.CurrentPageNumber();
                                });
                        });
                    });

            var pdf =
                document.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"HistoriaClinica_{paciente.Id}_{DateTime.Now:yyyyMMdd}.pdf");
        }

        // =========================================================
        // CARGAR HISTORIA DEL PACIENTE
        // =========================================================

        private async Task CargarHistoriaPaciente(
            CrearCitaViewModel modelo,
            int pacienteId)
        {
            if (pacienteId <= 0)
                return;

            var paciente =
                await _context.Pacientes
                    .FirstOrDefaultAsync(
                        p => p.Id == pacienteId);

            if (paciente == null)
                return;

            var consulta =
                await _context.ConsultasClinicas
                    .Include(c => c.Fisioterapeuta)
                    .Where(c =>
                        c.PacienteId == pacienteId &&
                        c.FisioterapeutaId == IdActual)
                    .OrderByDescending(
                        c => c.FechaConsulta)
                    .FirstOrDefaultAsync();

            modelo.PacienteSeleccionado =
                paciente;

            if (consulta != null)
            {
                modelo.HistoriaPaciente =
                    ConstruirHistoria(
                        paciente,
                        consulta);
            }
        }

        // =========================================================
        // CONSTRUIR HISTORIA
        // =========================================================

        private static Dictionary<string, string>
            ConstruirHistoria(
                Paciente paciente,
                ConsultaClinica consulta)
        {
            var historia =
                new Dictionary<string, string>();

            historia["IdPaciente"] =
                paciente.Id.ToString();

            using var antecedentes =
                ParseJson(
                    consulta.Antecedentes);

            using var funcional =
                ParseJson(
                    consulta.OtrosHallazgos);

            string Get(
                JsonElement root,
                string key)
            {
                if (
                    root.TryGetProperty(
                        key,
                        out var property))
                {
                    return property.GetString() ??
                           string.Empty;
                }

                return string.Empty;
            }

            historia["Nombre"] =
                paciente.Nombre;

            historia["Domicilio"] =
                paciente.Domicilio;

            historia["Telefono"] =
                paciente.Telefono;

            historia["Ocupacion"] =
                paciente.Ocupacion;

            historia["Sexo"] =
                paciente.Sexo;

            historia["Edad"] =
                paciente.Edad.ToString();

            historia["EstadoCivil"] =
                paciente.EstadoCivil;

            historia["Escolaridad"] =
                paciente.Escolaridad;

            historia["Terapeuta"] =
                consulta.Fisioterapeuta?.Nombre ??
                string.Empty;

            historia["Expediente"] =
                Get(
                    antecedentes.RootElement,
                    "Expediente");

            if (
                string.IsNullOrWhiteSpace(
                    historia["Expediente"]))
            {
                historia["Expediente"] =
                    $"FS-{paciente.Id:000000}";
            }

            historia["Fecha"] =
                consulta.FechaConsulta
                    .ToString("dd/MM/yyyy");

            historia["Peso"] =
                Get(
                    funcional.RootElement,
                    "Peso");

            historia["Talla"] =
                Get(
                    funcional.RootElement,
                    "Talla");

            historia["Estatura"] =
                Get(
                    funcional.RootElement,
                    "Estatura");

            historia["IMC"] =
                Get(
                    funcional.RootElement,
                    "IMC");

            historia["Etnia"] =
                Get(
                    funcional.RootElement,
                    "Etnia");

            historia["MotivoConsulta"] =
                consulta.MotivoConsulta;

            historia["TratamientosPrevios"] =
                consulta.TratamientosPrevios;

            historia["Diabetes"] =
                $"{Get(antecedentes.RootElement, "Diabetes")} · " +
                $"{Get(antecedentes.RootElement, "EspecificacionDiabetes")}";

            historia["Alergia"] =
                $"{Get(antecedentes.RootElement, "Alergia")} · " +
                $"{Get(antecedentes.RootElement, "EspecificacionAlergia")}";

            historia["HTA"] =
                $"{Get(antecedentes.RootElement, "HTA")} · " +
                $"{Get(antecedentes.RootElement, "EspecificacionHTA")}";

            historia["Cancer"] =
                $"{Get(antecedentes.RootElement, "Cancer")} · " +
                $"{Get(antecedentes.RootElement, "EspecificacionCancer")}";

            historia["Transfusiones"] =
                $"{Get(antecedentes.RootElement, "Transfusiones")} · " +
                $"{Get(antecedentes.RootElement, "EspecificacionTransfusiones")}";

            historia["EnfermedadesReumaticas"] =
                $"{Get(antecedentes.RootElement, "EnfermedadesReumaticas")} · " +
                $"{Get(antecedentes.RootElement, "EspecificacionEnfermedadesReumaticas")}";

            historia["Encames"] =
                $"{Get(antecedentes.RootElement, "Encames")} · " +
                $"{Get(antecedentes.RootElement, "EspecificacionEncames")}";

            historia["Accidentes"] =
                $"{Get(antecedentes.RootElement, "Accidentes")} · " +
                $"{Get(antecedentes.RootElement, "EspecificacionAccidentes")}";

            historia["Cardiopatias"] =
                $"{Get(antecedentes.RootElement, "Cardiopatias")} · " +
                $"{Get(antecedentes.RootElement, "EspecificacionCardiopatias")}";

            historia["Cirugias"] =
                $"{Get(antecedentes.RootElement, "Cirugias")} · " +
                $"{Get(antecedentes.RootElement, "EspecificacionCirugias")}";

            historia["Fracturas"] =
                $"{Get(antecedentes.RootElement, "Fracturas")} · " +
                $"Tipo: {Get(antecedentes.RootElement, "TipoFractura")}";

            historia["TA"] =
                ObtenerParteSignos(
                    consulta.SignosVitales,
                    "T/A");

            historia["Temperatura"] =
                ObtenerParteSignos(
                    consulta.SignosVitales,
                    "TEMP");

            historia["FC"] =
                ObtenerParteSignos(
                    consulta.SignosVitales,
                    "FC");

            historia["FR"] =
                ObtenerParteSignos(
                    consulta.SignosVitales,
                    "FR");

            historia["EspasmosContractura"] =
                consulta.EspasmosContractura;

            historia["Tabaquismo"] =
                Get(
                    antecedentes.RootElement,
                    "Tabaquismo");

            historia["Alcoholismo"] =
                Get(
                    antecedentes.RootElement,
                    "Alcoholismo");

            historia["Drogas"] =
                Get(
                    antecedentes.RootElement,
                    "Drogas");

            historia["ActividadFisica"] =
                Get(
                    antecedentes.RootElement,
                    "ActividadFisica");

            historia["SeAutomedica"] =
                Get(
                    antecedentes.RootElement,
                    "SeAutomedica");

            historia["Pasatiempo"] =
                Get(
                    antecedentes.RootElement,
                    "Pasatiempo");

            historia["EstaEmbarazada"] =
                Get(
                    antecedentes.RootElement,
                    "EstaEmbarazada");

            historia["NumeroHijos"] =
                Get(
                    antecedentes.RootElement,
                    "NumeroHijos");

            historia["Diagnostico"] =
                consulta.DiagnosticoRehabilitacion;

            historia["Reflejos"] =
                consulta.Reflejos;

            historia["Sensibilidad"] =
                consulta.Sensibilidad;

            historia["LenguajeOrientacion"] =
                consulta.LenguajeOrientacion;

            historia["OtrosDiagnostico"] =
                Get(
                    funcional.RootElement,
                    "OtrosDiagnostico");

            historia["SitioCicatriz"] =
                Get(
                    funcional.RootElement,
                    "SitioCicatriz");

            historia["CicatrizQueloide"] =
                Get(
                    funcional.RootElement,
                    "CicatrizQueloide");

            historia["CicatrizRetractil"] =
                Get(
                    funcional.RootElement,
                    "CicatrizRetractil");

            historia["CicatrizAbierta"] =
                Get(
                    funcional.RootElement,
                    "CicatrizAbierta");

            historia["CicatrizConAdherencia"] =
                Get(
                    funcional.RootElement,
                    "CicatrizConAdherencia");

            historia["CicatrizHipertrofica"] =
                Get(
                    funcional.RootElement,
                    "CicatrizHipertrofica");

            historia["TrasladosValorInicial"] =
                Get(
                    funcional.RootElement,
                    "TrasladosValorInicial");

            historia["TrasladosValorFinal"] =
                Get(
                    funcional.RootElement,
                    "TrasladosValorFinal");

            historia["TrasladoIndependiente"] =
                Get(
                    funcional.RootElement,
                    "TrasladoIndependiente");

            historia["TrasladoSillaRuedas"] =
                Get(
                    funcional.RootElement,
                    "TrasladoSillaRuedas");

            historia["TrasladoConAyudas"] =
                Get(
                    funcional.RootElement,
                    "TrasladoConAyudas");

            historia["TrasladoCamilla"] =
                Get(
                    funcional.RootElement,
                    "TrasladoCamilla");

            historia["MarchaLibre"] =
                Get(
                    funcional.RootElement,
                    "MarchaLibre");

            historia["MarchaClaudicante"] =
                Get(
                    funcional.RootElement,
                    "MarchaClaudicante");

            historia["MarchaConAyuda"] =
                Get(
                    funcional.RootElement,
                    "MarchaConAyuda");

            historia["MarchaEspastica"] =
                Get(
                    funcional.RootElement,
                    "MarchaEspastica");

            historia["MarchaAtaxica"] =
                Get(
                    funcional.RootElement,
                    "MarchaAtaxica");

            historia["MarchaOtros"] =
                Get(
                    funcional.RootElement,
                    "MarchaOtros");

            historia["ObservacionesMarcha"] =
                Get(
                    funcional.RootElement,
                    "ObservacionesMarcha");

            historia["EscalaDolor"] =
                consulta.EscalaDolor.ToString();

            historia["AntecedentesGenerales"] =
                Get(
                    antecedentes.RootElement,
                    "AntecedentesGenerales");

            historia["Alergias"] =
                consulta.Alergias;

            historia["ObservacionesGenerales"] =
                consulta.Observaciones;

            return historia;
        }

        // =========================================================
        // JSON
        // =========================================================

        private static JsonDocument ParseJson(
            string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return JsonDocument.Parse("{}");
            }

            try
            {
                return JsonDocument.Parse(json);
            }
            catch
            {
                return JsonDocument.Parse("{}");
            }
        }

        // =========================================================
        // SIGNOS VITALES
        // =========================================================

        private static string ObtenerParteSignos(
            string signos,
            string clave)
        {
            if (string.IsNullOrWhiteSpace(signos))
                return string.Empty;

            var partes =
                signos.Split(
                    '|',
                    StringSplitOptions.TrimEntries |
                    StringSplitOptions.RemoveEmptyEntries);

            var parte =
                partes.FirstOrDefault(
                    p =>
                        p.StartsWith(
                            clave,
                            StringComparison.OrdinalIgnoreCase));

            if (parte == null)
                return string.Empty;

            var indice =
                parte.IndexOf(':');

            return indice >= 0
                ? parte[(indice + 1)..].Trim()
                : parte;
        }

        // =========================================================
        // SÍ / NO
        // =========================================================

        private static string SiNo(bool? valor)
        {
            return valor == true
                ? "Sí"
                : "No";
        }
    }
}