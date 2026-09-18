using System.ComponentModel.DataAnnotations;

namespace FISIOSPORT.Models
{
    public class ProgramaRehabilitacion
    {
        public int Id { get; set; }

        [Required]
        public int PacienteId { get; set; }

        public Paciente? Paciente { get; set; }

        [Required]
        public int FisioterapeutaId { get; set; }

        public Fisioterapeuta? Fisioterapeuta { get; set; }

        [Required, MaxLength(200)]
        public string Diagnostico { get; set; } = string.Empty;

        [Required, MaxLength(3000)]
        public string ObjetivoGeneral { get; set; } = string.Empty;

        [Required]
        [Range(1, 100)]
        public int TotalSesiones { get; set; }

        public int SesionesCompletadas { get; set; } = 0;

        [Required]
        public DateTime FechaInicio { get; set; } = DateTime.Today;

        public DateTime? FechaFin { get; set; }

        [Required, MaxLength(30)]
        public string Estado { get; set; } = "Activo";

        public ICollection<EvolucionDiaria> Evoluciones { get; set; }
            = new List<EvolucionDiaria>();

        public ICollection<EjercicioAsignado> EjerciciosAsignados { get; set; }
            = new List<EjercicioAsignado>();

        public decimal PorcentajeProgreso
        {
            get
            {
                if (TotalSesiones <= 0)
                    return 0;

                return Math.Round(
                    (decimal)SesionesCompletadas / TotalSesiones * 100,
                    2
                );
            }
        }
    }
}