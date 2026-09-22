using System.ComponentModel.DataAnnotations;

namespace FISIOSPORT.Models
{
    public class Ejercicio
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del ejercicio es obligatorio.")]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(3000)]
        public string Descripcion { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Instrucciones { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string VideoUrl { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string ImagenUrl { get; set; } = string.Empty;

        // =========================================================
        // ETIQUETAS DEL CATÁLOGO
        // =========================================================

        [Required]
        [MaxLength(50)]
        public string TipoContraccion { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RegionAnatomica { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ObjetivoTerapeutico { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FasePaciente { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        // =========================================================
        // RELACIÓN CON EJERCICIOS ASIGNADOS
        // =========================================================

        public ICollection<EjercicioAsignado> Asignaciones { get; set; }
            = new List<EjercicioAsignado>();
    }
}