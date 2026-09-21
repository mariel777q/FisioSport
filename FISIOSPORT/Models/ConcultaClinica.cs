using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public DateTime FechaConsulta { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(3000)]
        public string Antecedentes { get; set; } = string.Empty;

        [Required]
        [MaxLength(3000)]
        public string Alergias { get; set; } = string.Empty;

        [Required]
        [MaxLength(3000)]
        public string MotivoConsulta { get; set; } = string.Empty;

        [Required]
        [MaxLength(3000)]
        public string Observaciones { get; set; } = string.Empty;

        // Nuevos campos de historia clínica

        [Required]
        [MaxLength(3000)]
        public string TratamientosPrevios { get; set; } = string.Empty;

        [MaxLength(500)]
        public string DiagnosticoMedico { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Reflejos { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Sensibilidad { get; set; } = string.Empty;

        [MaxLength(500)]
        public string LenguajeOrientacion { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string OtrosHallazgos { get; set; } = string.Empty;

        [MaxLength(500)]
        public string SignosVitales { get; set; } = string.Empty;

        [MaxLength(500)]
        public string EspasmosContractura { get; set; } = string.Empty;

        [MaxLength(500)]
        public string DiagnosticoRehabilitacion { get; set; } = string.Empty;

        [MaxLength(500)]
        public string CicatrizQuirurgica { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Movilidad { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Marcha { get; set; } = string.Empty;

        [Range(0, 10)]
        public int EscalaDolor { get; set; }
    }
}
