using System.ComponentModel.DataAnnotations;

namespace FISIOSPORT.Models
{
    public class RegistroEjercicioPaciente
    {
        public int Id { get; set; }

        [Required]
        public int EjercicioAsignadoId { get; set; }

        public EjercicioAsignado? EjercicioAsignado { get; set; }

        [Required]
        public int PacienteId { get; set; }

        public Paciente? Paciente { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.UtcNow.Date;

        public DateTime? FechaCompletado { get; set; }

        public bool Completado { get; set; } = false;
    }
}
