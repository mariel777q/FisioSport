using System.ComponentModel.DataAnnotations;

namespace FISIOSPORT.Models
{
    public class EjercicioAsignado
    {
        public int Id { get; set; }

        [Required]
        public int ProgramaRehabilitacionId { get; set; }

        public ProgramaRehabilitacion? ProgramaRehabilitacion { get; set; }

        [Required]
        public int EjercicioId { get; set; }

        public Ejercicio? Ejercicio { get; set; }

        [Required]
        [Range(1, 20)]
        public int Series { get; set; }

        [Required]
        [Range(1, 100)]
        public int Repeticiones { get; set; }

        [MaxLength(1000)]
        public string InstruccionesPersonalizadas { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Frecuencia { get; set; } = "Diaria";

        public bool Activo { get; set; } = true;
    }
}