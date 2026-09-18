using System.ComponentModel.DataAnnotations;

namespace FISIOSPORT.Models
{
    public class Fisioterapeuta
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(150)]
        public string Especialidad { get; set; } = string.Empty;

        [MaxLength(50)]
        public string NumeroColegiado { get; set; } = string.Empty;

        // Citas asignadas a este fisioterapeuta
        public ICollection<Cita> Citas { get; set; }
            = new List<Cita>();

        // Tratamientos realizados por este fisioterapeuta
        public ICollection<Tratamiento> Tratamientos { get; set; }
            = new List<Tratamiento>();
        public ICollection<ConsultaClinica> ConsultasClinicas { get; set; }
    = new List<ConsultaClinica>();

        public ICollection<ProgramaRehabilitacion> ProgramasRehabilitacion { get; set; }
            = new List<ProgramaRehabilitacion>();

        public ICollection<EvolucionDiaria> Evoluciones { get; set; }
            = new List<EvolucionDiaria>();
    }
}