using System.ComponentModel.DataAnnotations;

namespace FISIOSPORT.Models
{
    public class CitaCardViewModel
    {
        public int Id { get; set; }
        public string NombrePaciente { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Hora { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class DashboardFisioViewModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public string NumeroColegiado { get; set; } = string.Empty;
        public List<CitaCardViewModel> Citas { get; set; } = new();
    }

    public class CrearCitaViewModel
    {
        [Required(ErrorMessage = "Selecciona un paciente")]
        public int PacienteId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Indica la hora")]
        public string Hora { get; set; } = string.Empty;

        public string Notas { get; set; } = string.Empty;

        public List<Paciente> PacientesDisponibles { get; set; } = new();
    }
    public class DetalleCitaViewModel
    {
        public int CitaId { get; set; }

        public string NombrePaciente { get; set; } = string.Empty;

        public string CorreoPaciente { get; set; } = string.Empty;

        public string TelefonoPaciente { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public string Hora { get; set; } = string.Empty;

        public string EstadoCita { get; set; } = string.Empty;

        public string MotivoConsulta { get; set; } = "Sin información";

        public string Diagnostico { get; set; } = "Sin información";

        public string DescripcionTratamiento { get; set; } = "Sin información";

        public int TotalSesiones { get; set; }

        public int SesionesCompletadas { get; set; }

        public int SesionesRestantes { get; set; }

        public int PorcentajeProgreso { get; set; }

        public int? SesionActualId { get; set; }

        public int NumeroSesionActual { get; set; }

        public string TrabajoSesionActual { get; set; } = string.Empty;

        public List<SesionDetalleViewModel> Sesiones { get; set; } = new();
    }

    public class SesionDetalleViewModel
    {
        public int Id { get; set; }

        public int NumeroSesion { get; set; }

        public string TrabajoPlanificado { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string Observaciones { get; set; } = string.Empty;

        public DateTime? FechaCompletada { get; set; }

        public bool EsSesionActual { get; set; }

        public bool EstaCompletada { get; set; }
    }
}