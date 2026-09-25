using ClosedXML.Excel;
using FISIOSPORT.DTOs;

namespace FISIOSPORT.Services
{
    public class ExcelReporteService
    {
        public byte[] GenerarReporteCitas(
            IEnumerable<CitaReporteDto> citas)
        {
            using var memoria = new MemoryStream();

            using (var libro = new XLWorkbook())
            {
                var hoja = libro.Worksheets.Add("Citas");

                // Título
                hoja.Cell("A1").Value = "REPORTE DE CITAS - FISIOSPORT";
                hoja.Range("A1:D1").Merge();

                hoja.Cell("A1").Style.Font.Bold = true;
                hoja.Cell("A1").Style.Font.FontSize = 16;
                hoja.Cell("A1").Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;
                hoja.Cell("A1").Style.Alignment.Vertical =
                    XLAlignmentVerticalValues.Center;

                hoja.Row(1).Height = 30;

                // Encabezados
                hoja.Cell("A3").Value = "ID";
                hoja.Cell("B3").Value = "Nombre paciente";
                hoja.Cell("C3").Value = "Fecha de cita";
                hoja.Cell("D3").Value = "Estado de cita";

                var encabezados = hoja.Range("A3:D3");

                encabezados.Style.Font.Bold = true;
                encabezados.Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;
                encabezados.Style.Alignment.Vertical =
                    XLAlignmentVerticalValues.Center;

                // Datos
                var fila = 4;

                foreach (var cita in citas)
                {
                    hoja.Cell(fila, 1).Value = cita.Id;
                    hoja.Cell(fila, 2).Value = cita.NombrePaciente;
                    hoja.Cell(fila, 3).Value = cita.FechaCita;
                    hoja.Cell(fila, 4).Value = cita.EstadoCita;

                    hoja.Cell(fila, 3).Style.DateFormat.Format =
                        "dd/MM/yyyy";

                    fila++;
                }

                // Crear tabla de Excel
                if (fila > 4)
                {
                    var rangoTabla =
                        hoja.Range($"A3:D{fila - 1}");

                    var tabla = rangoTabla.CreateTable();

                    tabla.Name = "TablaCitas";
                    tabla.Theme = XLTableTheme.TableStyleMedium2;
                }

                // Filtro y congelar encabezados
                hoja.SheetView.FreezeRows(3);

                // Ajustar columnas
                hoja.Columns().AdjustToContents();

                // Anchos máximos razonables
                hoja.Column("A").Width = 10;
                hoja.Column("B").Width = 30;
                hoja.Column("C").Width = 18;
                hoja.Column("D").Width = 20;

                libro.SaveAs(memoria);
            }

            return memoria.ToArray();
        }
    }
}