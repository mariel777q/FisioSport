using System.ComponentModel.DataAnnotations;

namespace FISIOSPORT.Models
{
    public class EjercicioAsignado
    {
        public int Id { get; set; }

        // =========================================================
        // CITA A LA QUE PERTENECE
        // =========================================================

        public int? CitaId { get; set; }

        public Cita? Cita { get; set; }

        // =========================================================
        // PROGRAMA DE REHABILITACIÓN
        // Se mantiene para no romper la estructura existente.
        // =========================================================

        public int? ProgramaRehabilitacionId { get; set; }

        public ProgramaRehabilitacion? ProgramaRehabilitacion { get; set; }

        // =========================================================
        // EJERCICIO DEL CATÁLOGO
        // =========================================================

        [Required]
        public int EjercicioId { get; set; }

        public Ejercicio? Ejercicio { get; set; }

        // =========================================================
        // DOSIFICACIÓN
        // =========================================================

        [Required]
        [Range(1, 20)]
        public int Series { get; set; } = 3;

        [Required]
        [Range(1, 100)]
        public int Repeticiones { get; set; } = 10;

        // =========================================================
        // PERSONALIZACIÓN
        // =========================================================

        [MaxLength(1000)]
        public string InstruccionesPersonalizadas { get; set; }
            = string.Empty;

        [MaxLength(30)]
        public string Frecuencia { get; set; }
            = "Una vez";

        public bool Activo { get; set; } = true;

        // =========================================================
        // FECHA DE ASIGNACIÓN
        // PostgreSQL: timestamp with time zone
        // =========================================================

        public DateTime FechaAsignacion { get; set; }
            = DateTime.UtcNow;
    }
}