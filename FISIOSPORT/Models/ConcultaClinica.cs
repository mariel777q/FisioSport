using System.ComponentModel.DataAnnotations;

namespace FISIOSPORT.Models
{
    public class ConsultaClinica
    {
        public int Id { get; set; }

        [Required]
        public int PacienteId { get; set; }

        public Paciente? Paciente { get; set; }

        [Required]
        public int FisioterapeutaId { get; set; }

        public Fisioterapeuta? Fisioterapeuta { get; set; }

        [Required]
        public DateTime FechaConsulta { get; set; } = DateTime.Now;

        [MaxLength(3000)]
        public string Antecedentes { get; set; } = string.Empty;

        [MaxLength(3000)]
        public string Alergias { get; set; } = string.Empty;

        [MaxLength(3000)]
        public string MotivoConsulta { get; set; } = string.Empty;

        [MaxLength(3000)]
        public string Observaciones { get; set; } = string.Empty;
    }
}