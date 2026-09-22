namespace FISIOSPORT.Models
{
    public class CitaPacienteCardViewModel
    {
        public int Id { get; set; }

        public string NombreFisioterapeuta { get; set; }
            = string.Empty;

        public string Especialidad { get; set; }
            = string.Empty;

        public DateTime Fecha { get; set; }

        public string Hora { get; set; }
            = string.Empty;

        public string Estado { get; set; }
            = string.Empty;
    }

    public class DashboardPacienteViewModel
    {
        public string Nombre { get; set; }
            = string.Empty;

        public List<CitaPacienteCardViewModel> Citas { get; set; }
            = new();

        public List<EjercicioPacienteViewModel>
            EjerciciosPendientes
        { get; set; }
            = new();
    }
}