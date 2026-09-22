using System.ComponentModel.DataAnnotations;

namespace FISIOSPORT.Models
{
    public class RegistroEjercicioPaciente
    {
        public int Id { get; set; }

        // =========================================================
        // EJERCICIO ASIGNADO
        // =========================================================

        [Required]
        public int EjercicioAsignadoId { get; set; }

        public EjercicioAsignado? EjercicioAsignado { get; set; }

        // =========================================================
        // PACIENTE
        // =========================================================

        [Required]
        public int PacienteId { get; set; }

        public Paciente? Paciente { get; set; }

        // =========================================================
        // FECHA DEL REGISTRO
        // =========================================================

        [Required]
        public DateTime Fecha { get; set; }
            = DateTime.UtcNow.Date;

        // =========================================================
        // COMPLETADO
        // =========================================================

        public DateTime? FechaCompletado { get; set; }

        public bool Completado { get; set; } = false;
    }
}