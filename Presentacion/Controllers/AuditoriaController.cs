using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;
using System;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Presentacion.Controllers
{
    // 🔥 EL ESCUDO: Solo permitimos el acceso EXCLUSIVO a los que tengan el rol "Auditor"
    [Authorize(Roles = "Auditor")]
    public class AuditoriaController : Controller
    {
        private readonly logicaAuditoria _logicaAuditoria = new logicaAuditoria();

        // 1. VISTA PRINCIPAL
        public IActionResult Index()
        {
            var listaAuditoria = _logicaAuditoria.ListarAuditoriaGeneral();
            return View(listaAuditoria);
        }

        // 2. FILTRAR POR FECHA
        [HttpPost]
        public IActionResult FiltrarPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var listaFiltrada = _logicaAuditoria.ListarAuditoriaPorFecha(fechaInicio, fechaFin);

                ViewBag.FechaInicio = fechaInicio.ToString("yyyy-MM-dd");
                ViewBag.FechaFin = fechaFin.ToString("yyyy-MM-dd");

                return View("Index", listaFiltrada);
            }
            catch (ArgumentException ex)
            {
                ViewBag.Error = ex.Message;
                return View("Index", _logicaAuditoria.ListarAuditoriaGeneral());
            }
            catch (Exception)
            {
                ViewBag.Error = "Ocurrió un error inesperado al procesar la solicitud.";
                return View("Index", _logicaAuditoria.ListarAuditoriaGeneral());
            }
        }

        // 3. EXPORTAR A PDF USANDO QUESTPDF
        [HttpPost]
        public IActionResult ExportarPDF(DateTime? fechaInicio, DateTime? fechaFin)
        {
            // Configuración obligatoria para usar la versión gratis de QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;

            // Obtener los datos dependiendo de si se enviaron fechas de filtro
            var listaAuditoria = (fechaInicio.HasValue && fechaFin.HasValue)
                ? _logicaAuditoria.ListarAuditoriaPorFecha(fechaInicio.Value, fechaFin.Value)
                : _logicaAuditoria.ListarAuditoriaGeneral();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Reporte de Auditoría - YuNet Store").FontSize(20).SemiBold().FontColor("#2563EB");
                            col.Item().Text($"Generado por: {User.Identity.Name} el {DateTime.Now:dd/MM/yyyy HH:mm}");
                            if (fechaInicio.HasValue)
                            {
                                col.Item().Text($"Filtro aplicado: Del {fechaInicio.Value:dd/MM/yyyy} al {fechaFin.Value:dd/MM/yyyy}").FontColor(Colors.Grey.Medium);
                            }
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                    {
                        // Definir el ancho de las columnas
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);  // ID
                            columns.ConstantColumn(90);  // Fecha
                            columns.RelativeColumn(1);   // Usuario
                            columns.ConstantColumn(80);  // Acción
                            columns.ConstantColumn(90);  // Tabla
                            columns.RelativeColumn(2);   // Detalle
                            columns.ConstantColumn(80);  // IP
                        });

                        // Encabezados de la tabla
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("ID");
                            header.Cell().Element(CellStyle).Text("Fecha");
                            header.Cell().Element(CellStyle).Text("Usuario");
                            header.Cell().Element(CellStyle).Text("Acción");
                            header.Cell().Element(CellStyle).Text("Tabla");
                            header.Cell().Element(CellStyle).Text("Detalle");
                            header.Cell().Element(CellStyle).Text("IP");

                            static IContainer CellStyle(IContainer container) =>
                                container.DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White)).PaddingVertical(5).PaddingHorizontal(2).Background("#0F172A");
                        });

                        // Iterar los registros para llenar el PDF con las propiedades en MAYÚSCULAS
                        foreach (var item in listaAuditoria)
                        {
                            table.Cell().Element(CellPadding).Text(item.ID_AUDITORIA.ToString());
                            table.Cell().Element(CellPadding).Text(Convert.ToDateTime(item.FECHA_REGISTRO).ToString("dd/MM/yyyy HH:mm"));
                            table.Cell().Element(CellPadding).Text(item.NOMBRE_USUARIO?.ToString() ?? "");
                            table.Cell().Element(CellPadding).Text(item.ACCION?.ToString() ?? "");
                            table.Cell().Element(CellPadding).Text(item.TABLA_AFECTADA?.ToString() ?? "");
                            table.Cell().Element(CellPadding).Text(item.DETALLE_ACTIVIDAD?.ToString() ?? "");
                            table.Cell().Element(CellPadding).Text(item.DIRECCION_IP?.ToString() ?? "");

                            static IContainer CellPadding(IContainer container) =>
                                container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).PaddingHorizontal(2);
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"ReporteAuditoria_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
    }
}