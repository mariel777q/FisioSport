using System;
using System.Collections.Generic;

namespace FISIOSPORT.Models
{
    // =========================================================
    // CATÁLOGO Y ASIGNACIÓN DE EJERCICIOS
    // =========================================================

    public class EjerciciosIndexViewModel
    {
        public int CitaId { get; set; }

        public string PacienteNombre { get; set; } = string.Empty;

        public DateTime FechaCita { get; set; }

        public string HoraCita { get; set; } = string.Empty;

        // ÚNICO filtro de búsqueda. Es opcional.
        public string? Buscar { get; set; }

        public List<string> TodosLosNombres { get; set; } = new();

        public List<EjercicioSeleccionViewModel> Ejercicios { get; set; } = new();
    }

    public class EjercicioSeleccionViewModel
    {
        public int EjercicioId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public string Instrucciones { get; set; } = string.Empty;

        public string? VideoUrl { get; set; }

        public string? ImagenUrl { get; set; }

        public string TipoContraccion { get; set; } = string.Empty;

        public string RegionAnatomica { get; set; } = string.Empty;

        public string ObjetivoTerapeutico { get; set; } = string.Empty;

        public string FasePaciente { get; set; } = string.Empty;

        public bool Seleccionado { get; set; }

        public int Series { get; set; } = 3;

        public int Repeticiones { get; set; } = 10;

        // OPCIONAL.
        public string? InstruccionesPersonalizadas { get; set; }
    }

    // =========================================================
    // EJERCICIOS ASIGNADOS A UNA CITA
    // =========================================================

    public class EjerciciosCitaViewModel
    {
        public int CitaId { get; set; }

        public string PacienteNombre { get; set; } = string.Empty;

        public List<EjercicioPendienteItemViewModel> Ejercicios { get; set; } = new();
    }

    public class EjercicioPendienteItemViewModel
    {
        public int AsignacionId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public string ImagenUrl { get; set; } = string.Empty;

        public string VideoUrl { get; set; } = string.Empty;

        public string TipoContraccion { get; set; } = string.Empty;

        public string RegionAnatomica { get; set; } = string.Empty;

        public string ObjetivoTerapeutico { get; set; } = string.Empty;

        public string FasePaciente { get; set; } = string.Empty;

        public int Series { get; set; }

        public int Repeticiones { get; set; }

        public string Instrucciones { get; set; } = string.Empty;

        public bool Completado { get; set; }

        public DateTime? FechaCompletado { get; set; }
    }

    // =========================================================
    // EJERCICIOS MOSTRADOS EN EL DASHBOARD DEL PACIENTE
    // =========================================================

    public class EjercicioPacienteViewModel
    {
        public int AsignacionId { get; set; }

        public int CitaId { get; set; }

        public DateTime FechaCita { get; set; }

        public string HoraCita { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public string ImagenUrl { get; set; } = string.Empty;

        public string VideoUrl { get; set; } = string.Empty;

        public string TipoContraccion { get; set; } = string.Empty;

        public string RegionAnatomica { get; set; } = string.Empty;

        public string ObjetivoTerapeutico { get; set; } = string.Empty;

        public string FasePaciente { get; set; } = string.Empty;

        public int Series { get; set; }

        public int Repeticiones { get; set; }

        public string Instrucciones { get; set; } = string.Empty;

        public bool Completado { get; set; }

        public DateTime? FechaCompletado { get; set; }
    }
}


