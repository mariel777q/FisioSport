using System.ComponentModel.DataAnnotations;

namespace FISIOSPORT.Models
{
    public class Cita
    {
        public int Id { get; set; }

        // =========================================================
        // FISIOTERAPEUTA
        // =========================================================

        public int FisioterapeutaId { get; set; }

        public Fisioterapeuta? Fisioterapeuta { get; set; }

        // =========================================================
        // PACIENTE
        // =========================================================

        public int PacienteId { get; set; }

        public Paciente? Paciente { get; set; }

        // =========================================================
        // SESIÓN DE TRATAMIENTO
        // =========================================================

        public int? SesionTratamientoId { get; set; }

        public SesionTratamiento? SesionTratamiento { get; set; }

        // =========================================================
        // DATOS DE LA CITA
        // =========================================================

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [MaxLength(10)]
        public string Hora { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Estado { get; set; } = "Pendiente";

        public string Notas { get; set; } = string.Empty;

        // =========================================================
        // EJERCICIOS ASIGNADOS EN ESTA CITA
        // =========================================================

        public ICollection<EjercicioAsignado> EjerciciosAsignados
        {
            get;
            set;
        } = new List<EjercicioAsignado>();
    }
}