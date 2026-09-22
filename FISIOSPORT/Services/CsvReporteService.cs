using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using FISIOSPORT.DTOs;

namespace FISIOSPORT.Services
{
    public class CsvReporteService
    {
        public byte[] GenerarReporteCitas(
            IEnumerable<CitaReporteDto> citas)
        {
            using var memoria = new MemoryStream();

            var encoding = new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: true);

            using (var escritor = new StreamWriter(
                memoria,
                encoding,
                bufferSize: 1024,
                leaveOpen: true))
            {
                var configuracion =
                    new CsvConfiguration(
                        CultureInfo.InvariantCulture)
                    {
                        Delimiter = ";",
                        HasHeaderRecord = true
                    };

                using var csv =
                    new CsvWriter(escritor, configuracion);

                csv.WriteField("ID");
                csv.WriteField("Nombre paciente");
                csv.WriteField("Fecha de cita");
                csv.WriteField("Estado de cita");
                csv.NextRecord();

                foreach (var cita in citas)
                {
                    csv.WriteField(cita.Id);
                    csv.WriteField(cita.NombrePaciente);
                    csv.WriteField(
                        cita.FechaCita.ToString("dd/MM/yyyy"));
                    csv.WriteField(cita.EstadoCita);
                    csv.NextRecord();
                }

                escritor.Flush();
            }

            return memoria.ToArray();
        }
    }
}