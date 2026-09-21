using System.ComponentModel.DataAnnotations;

namespace FISIOSPORT.Models
{
    public class Tratamiento
    {
        public int Id { get; set; }

        [Required]
        public int PacienteId { get; set; }

        public Paciente? Paciente { get; set; }

        [Required]
        public int FisioterapeutaId { get; set; }

        public Fisioterapeuta? Fisioterapeuta { get; set; }

        [Required, MaxLength(250)]
        public string MotivoConsulta { get; set; } = string.Empty;

        [Required, MaxLength(250)]
        public string Diagnostico { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public int TotalSesiones { get; set; }

        public DateTime FechaInicio { get; set; } = DateTime.UtcNow.Date;

        [MaxLength(30)]
        public string Estado { get; set; } = "Activo";

        public ICollection<SesionTratamiento> Sesiones { get; set; }
            = new List<SesionTratamiento>();
    }
}
