using System.ComponentModel.DataAnnotations;

namespace FISIOSPORT.Models
{
    public class Ejercicio
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(3000)]
        public string Descripcion { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Instrucciones { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string VideoUrl { get; set; } = string.Empty;

        public ICollection<EjercicioAsignado> Asignaciones { get; set; }
            = new List<EjercicioAsignado>();
    }
}