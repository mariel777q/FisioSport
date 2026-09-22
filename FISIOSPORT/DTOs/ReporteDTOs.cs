using System;

namespace FISIOSPORT.DTOs
{
    public class CitaReporteDto
    {
        public int Id { get; set; }

        public string NombrePaciente { get; set; } = string.Empty;

        public DateTime FechaCita { get; set; }

        public string EstadoCita { get; set; } = string.Empty;
    }
}