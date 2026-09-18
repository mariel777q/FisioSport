using System.ComponentModel.DataAnnotations;

namespace FISIOSPORT.Models
{
    public class Paciente
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Telefono { get; set; } = string.Empty;

        [MaxLength(150)]
        public string ContactoEmergencia { get; set; } = string.Empty;

        public ICollection<Cita> Citas { get; set; } = new List<Cita>();

        public ICollection<Tratamiento> Tratamientos { get; set; }
            = new List<Tratamiento>();
        public ICollection<ConsultaClinica> ConsultasClinicas { get; set; }
    = new List<ConsultaClinica>();

        public ICollection<ProgramaRehabilitacion> ProgramasRehabilitacion { get; set; }
            = new List<ProgramaRehabilitacion>();

        public ICollection<RegistroEjercicioPaciente> RegistrosEjercicios { get; set; }
            = new List<RegistroEjercicioPaciente>();
    }
}