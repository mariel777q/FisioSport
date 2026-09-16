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

        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}