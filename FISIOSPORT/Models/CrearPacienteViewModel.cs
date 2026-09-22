using System.ComponentModel.DataAnnotations;

namespace FISIOSPORT.Models
{
    public class CrearPacienteViewModel
    {
        // =========================================================
        // DATOS PERSONALES
        // =========================================================

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(150)]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Introduce un correo válido.")]
        [MaxLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [MaxLength(30)]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El contacto de emergencia es obligatorio.")]
        [MaxLength(150)]
        [Display(Name = "Contacto de emergencia")]
        public string ContactoEmergencia { get; set; } = string.Empty;

        [Required(ErrorMessage = "El domicilio es obligatorio.")]
        [MaxLength(250)]
        public string Domicilio { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ocupación es obligatoria.")]
        [MaxLength(150)]
        public string Ocupacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona el sexo.")]
        public string Sexo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La edad es obligatoria.")]
        [Range(1, 120, ErrorMessage = "La edad debe estar entre 1 y 120 años.")]
        public int? Edad { get; set; }

        [Required(ErrorMessage = "Selecciona el estado civil.")]
        public string EstadoCivil { get; set; } = string.Empty;

        [Required(ErrorMessage = "La escolaridad es obligatoria.")]
        public string Escolaridad { get; set; } = string.Empty;

        // =========================================================
        // EXPLORACIÓN FÍSICA
        // =========================================================

        [Required(ErrorMessage = "El peso es obligatorio.")]
        [Range(0.1, 500, ErrorMessage = "Introduce un peso válido.")]
        public decimal? Peso { get; set; }

        [Required(ErrorMessage = "La talla es obligatoria.")]
        [Range(0.1, 300, ErrorMessage = "Introduce una talla válida.")]
        public decimal? Talla { get; set; }

        [Required(ErrorMessage = "La estatura es obligatoria.")]
        [Range(1, 300, ErrorMessage = "Introduce una estatura válida.")]
        public decimal? Estatura { get; set; }

        [Required(ErrorMessage = "El IMC es obligatorio.")]
        [Range(0.1, 100, ErrorMessage = "Introduce un IMC válido.")]
        public decimal? IMC { get; set; }

        [Required(ErrorMessage = "La etnia es obligatoria.")]
        [MaxLength(100)]
        public string Etnia { get; set; } = string.Empty;

        // =========================================================
        // MOTIVO Y TRATAMIENTOS PREVIOS
        // =========================================================

        [Required(ErrorMessage = "El motivo de consulta es obligatorio.")]
        [MaxLength(3000)]
        public string MotivoConsulta { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los tratamientos previos son obligatorios.")]
        [MaxLength(3000)]
        public string TratamientosPrevios { get; set; } = string.Empty;

        // Campo virtual para mostrar errores relacionados con el JSON de otros hallazgos
        // (no se guarda directamente desde el ViewModel; sirve para enlazar mensajes de ModelState)
        [Display(Name = "Otros hallazgos")]
        public string? OtrosHallazgos { get; set; }

        // =========================================================
        // ANTECEDENTES
        // =========================================================

        [Required(ErrorMessage = "Selecciona Sí o No para Diabetes.")]
        public bool? Diabetes { get; set; }

        [Required(ErrorMessage = "Especifica el antecedente de Diabetes.")]
        [MaxLength(1000)]
        public string EspecificacionDiabetes { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona Sí o No para Alergia.")]
        public bool? Alergia { get; set; }

        [Required(ErrorMessage = "Especifica el antecedente de Alergia.")]
        [MaxLength(1000)]
        public string EspecificacionAlergia { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona Sí o No para HTA.")]
        public bool? HTA { get; set; }

        [Required(ErrorMessage = "Especifica el antecedente de HTA.")]
        [MaxLength(1000)]
        public string EspecificacionHTA { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona Sí o No para Cáncer.")]
        public bool? Cancer { get; set; }

        [Required(ErrorMessage = "Especifica el antecedente de Cáncer.")]
        [MaxLength(1000)]
        public string EspecificacionCancer { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona Sí o No para Transfusiones.")]
        public bool? Transfusiones { get; set; }

        [Required(ErrorMessage = "Especifica el antecedente de Transfusiones.")]
        [MaxLength(1000)]
        public string EspecificacionTransfusiones { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona Sí o No para Enfermedades reumáticas.")]
        public bool? EnfermedadesReumaticas { get; set; }

        [Required(ErrorMessage = "Especifica el antecedente de Enfermedades reumáticas.")]
        [MaxLength(1000)]
        public string EspecificacionEnfermedadesReumaticas { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona Sí o No para Encames.")]
        public bool? Encames { get; set; }

        [Required(ErrorMessage = "Especifica el antecedente de Encames.")]
        [MaxLength(1000)]
        public string EspecificacionEncames { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona Sí o No para Accidentes.")]
        public bool? Accidentes { get; set; }

        [Required(ErrorMessage = "Especifica el antecedente de Accidentes.")]
        [MaxLength(1000)]
        public string EspecificacionAccidentes { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona Sí o No para Cardiopatías.")]
        public bool? Cardiopatias { get; set; }

        [Required(ErrorMessage = "Especifica el antecedente de Cardiopatías.")]
        [MaxLength(1000)]
        public string EspecificacionCardiopatias { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona Sí o No para Cirugías.")]
        public bool? Cirugias { get; set; }

        [Required(ErrorMessage = "Especifica el antecedente de Cirugías.")]
        [MaxLength(1000)]
        public string EspecificacionCirugias { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona Sí o No para Fracturas.")]
        public bool? Fracturas { get; set; }

        [Required(ErrorMessage = "Especifica el tipo de fractura.")]
        [MaxLength(500)]
        public string TipoFractura { get; set; } = string.Empty;

        // =========================================================
        // SIGNOS VITALES
        // =========================================================

        [Required(ErrorMessage = "La tensión arterial es obligatoria.")]
        [MaxLength(50)]
        public string TA { get; set; } = string.Empty;

        [Required(ErrorMessage = "La temperatura es obligatoria.")]
        [Range(20, 50, ErrorMessage = "Introduce una temperatura válida.")]
        public decimal? Temperatura { get; set; }

        [Required(ErrorMessage = "La frecuencia cardiaca es obligatoria.")]
        [Range(20, 250, ErrorMessage = "Introduce una frecuencia cardiaca válida.")]
        public int? FC { get; set; }

        [Required(ErrorMessage = "La frecuencia respiratoria es obligatoria.")]
        [Range(5, 100, ErrorMessage = "Introduce una frecuencia respiratoria válida.")]
        public int? FR { get; set; }

        [Required(ErrorMessage = "Este campo es obligatorio.")]
        [MaxLength(2000)]
        public string EspasmosContracturaMuscular { get; set; } = string.Empty;

        // =========================================================
        // HÁBITOS
        // =========================================================

        [Required(ErrorMessage = "Selecciona Sí o No para Tabaquismo.")]
        public bool? Tabaquismo { get; set; }

        [Required(ErrorMessage = "Selecciona Sí o No para Alcoholismo.")]
        public bool? Alcoholismo { get; set; }

        [Required(ErrorMessage = "Selecciona Sí o No para Drogas.")]
        public bool? Drogas { get; set; }

        [Required(ErrorMessage = "Selecciona Sí o No para Actividad física.")]
        public bool? ActividadFisica { get; set; }

        [Required(ErrorMessage = "Selecciona Sí o No para Automedicación.")]
        public bool? SeAutomedica { get; set; }

        [Required(ErrorMessage = "El pasatiempo es obligatorio.")]
        [MaxLength(500)]
        public string Pasatiempo { get; set; } = string.Empty;

        // =========================================================
        // MUJERES
        // =========================================================

        [Required(ErrorMessage = "Selecciona Sí o No para embarazo.")]
        public bool? EstaEmbarazada { get; set; }

        [Required(ErrorMessage = "Indica el número de hijos.")]
        [Range(0, 30, ErrorMessage = "Introduce un número válido de hijos.")]
        public int? NumeroHijos { get; set; }

        // =========================================================
        // DIAGNÓSTICO
        // =========================================================

        [Required(ErrorMessage = "El diagnóstico médico en rehabilitación es obligatorio.")]
        [MaxLength(3000)]
        public string DiagnosticoMedicoRehabilitacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La valoración de reflejos es obligatoria.")]
        [MaxLength(2000)]
        public string Reflejos { get; set; } = string.Empty;

        [Required(ErrorMessage = "La valoración de sensibilidad es obligatoria.")]
        [MaxLength(2000)]
        public string Sensibilidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "La valoración de lenguaje/orientación es obligatoria.")]
        [MaxLength(2000)]
        public string LenguajeOrientacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo Otros es obligatorio.")]
        [MaxLength(2000)]
        public string OtrosDiagnostico { get; set; } = string.Empty;

        // =========================================================
        // CICATRIZ
        // =========================================================

        [Required(ErrorMessage = "El sitio de la cicatriz es obligatorio.")]
        [MaxLength(500)]
        public string SitioCicatriz { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona Sí o No para Queloide.")]
        public bool? CicatrizQueloide { get; set; }

        [Required(ErrorMessage = "Selecciona Sí o No para Retráctil.")]
        public bool? CicatrizRetractil { get; set; }

        [Required(ErrorMessage = "Selecciona Sí o No para Abierto.")]
        public bool? CicatrizAbierta { get; set; }

        [Required(ErrorMessage = "Selecciona Sí o No para Adherencia.")]
        public bool? CicatrizConAdherencia { get; set; }

        [Required(ErrorMessage = "Selecciona Sí o No para Hipertrófica.")]
        public bool? CicatrizHipertrofica { get; set; }

        // =========================================================
        // TRASLADOS
        // =========================================================

        [Required(ErrorMessage = "La valoración inicial de traslados es obligatoria.")]
        [MaxLength(2000)]
        public string TrasladosValorInicial { get; set; } = string.Empty;

        [Required(ErrorMessage = "La valoración final de traslados es obligatoria.")]
        [MaxLength(2000)]
        public string TrasladosValorFinal { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona Sí o No para Independiente.")]
        public bool? TrasladoIndependiente { get; set; }

        [Required(ErrorMessage = "Selecciona Sí o No para Silla de ruedas.")]
        public bool? TrasladoSillaRuedas { get; set; }

        [Required(ErrorMessage = "Selecciona Sí o No para Ayudas.")]
        public bool? TrasladoConAyudas { get; set; }

        [Required(ErrorMessage = "Selecciona Sí o No para Camilla.")]
        public bool? TrasladoCamilla { get; set; }

        // =========================================================
        // MARCHA
        // =========================================================

        [Required(ErrorMessage = "Selecciona Sí o No para Marcha libre.")]
        public bool? MarchaLibre { get; set; }

        [Required(ErrorMessage = "Selecciona Sí o No para Marcha claudicante.")]
        public bool? MarchaClaudicante { get; set; }

        [Required(ErrorMessage = "Selecciona Sí o No para Marcha con ayuda.")]
        public bool? MarchaConAyuda { get; set; }

        [Required(ErrorMessage = "Selecciona Sí o No para Marcha espástica.")]
        public bool? MarchaEspastica { get; set; }

        [Required(ErrorMessage = "Selecciona Sí o No para Marcha atáxica.")]
        public bool? MarchaAtaxica { get; set; }

        [Required(ErrorMessage = "Selecciona Sí o No para Otros tipos de marcha.")]
        public bool? MarchaOtros { get; set; }

        [Required(ErrorMessage = "Las observaciones de marcha son obligatorias.")]
        [MaxLength(2000)]
        public string ObservacionesMarcha { get; set; } = string.Empty;

        // =========================================================
        // DOLOR
        // =========================================================

        [Required(ErrorMessage = "Selecciona el nivel de dolor.")]
        [Range(0, 10, ErrorMessage = "El dolor debe estar entre 0 y 10.")]
        public int? EscalaDolor { get; set; }

        // =========================================================
        // OBSERVACIONES GENERALES
        // =========================================================

        [Required(ErrorMessage = "Los antecedentes generales son obligatorios.")]
        [MaxLength(3000)]
        public string Antecedentes { get; set; } = string.Empty;

        [Required(ErrorMessage = "Las alergias son obligatorias.")]
        [MaxLength(3000)]
        public string Alergias { get; set; } = string.Empty;

        [Required(ErrorMessage = "Las observaciones son obligatorias.")]
        [MaxLength(3000)]
        public string Observaciones { get; set; } = string.Empty;

        // =========================================================
        // TRATAMIENTO
        // =========================================================

        [Required(ErrorMessage = "Indica cuántas sesiones necesitará el paciente.")]
        [Range(1, 100, ErrorMessage = "El tratamiento debe tener entre 1 y 100 sesiones.")]
        [Display(Name = "Cantidad de sesiones")]
        public int? TotalSesiones { get; set; }
    }
}