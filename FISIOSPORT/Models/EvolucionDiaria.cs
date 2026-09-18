using System.ComponentModel.DataAnnotations;

namespace FISIOSPORT.Models
{
    public class EvolucionDiaria
    {
        public int Id { get; set; }

        [Required]
        public int ProgramaRehabilitacionId { get; set; }

        public ProgramaRehabilitacion? ProgramaRehabilitacion { get; set; }

        [Required]
        public int FisioterapeutaId { get; set; }

        public Fisioterapeuta? Fisioterapeuta { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required, MaxLength(3000)]
        public string Informe { get; set; } = string.Empty;

        [MaxLength(3000)]
        public string Observaciones { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Estado { get; set; } = "Completada";
    }
}