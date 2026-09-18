using System.ComponentModel.DataAnnotations;

namespace FISIOSPORT.Models
{
    public class Cita
    {
        public int Id { get; set; }

        public int FisioterapeutaId { get; set; }

        public Fisioterapeuta? Fisioterapeuta { get; set; }

        public int PacienteId { get; set; }

        public Paciente? Paciente { get; set; }

        public int? SesionTratamientoId { get; set; }

        public SesionTratamiento? SesionTratamiento { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required, MaxLength(10)]
        public string Hora { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Estado { get; set; } = "Pendiente";

        public string Notas { get; set; } = string.Empty;
    }
}