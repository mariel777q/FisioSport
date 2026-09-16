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
}