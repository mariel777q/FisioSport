using System.ComponentModel.DataAnnotations;

namespace FISIOSPORT.Models
{
    public class SesionTratamiento
    {
        public int Id { get; set; }

        [Required]
        public int TratamientoId { get; set; }

        public Tratamiento? Tratamiento { get; set; }

        [Required]
        public int NumeroSesion { get; set; }

        [Required, MaxLength(500)]
        public string TrabajoPlanificado { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Observaciones { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Estado { get; set; } = "Pendiente";

        public DateTime? FechaCompletada { get; set; }

        public int? CitaId { get; set; }

        public Cita? Cita { get; set; }
    }
}