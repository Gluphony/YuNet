using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Entidades;
using System.Collections.Generic;

public class CategoriaReporte : IDocument
{
    private readonly List<Categoria> _categorias;

    public CategoriaReporte(List<Categoria> categorias)
    {
        _categorias = categorias;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(40);

            // --- ENCABEZADO ---
            page.Header().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("YuNet STORE").FontSize(24).SemiBold().FontColor("#00bcd4");
                    col.Item().Text("Reporte de Categorías").FontSize(14).Italic().FontColor(Colors.Grey.Medium);
                });

                row.RelativeItem().AlignRight().Column(col => {
                    col.Item().Text(DateTime.Now.ToString("dd/MM/yyyy")).FontSize(12);
                    col.Item().Text(DateTime.Now.ToString("HH:mm tt")).FontSize(10).FontColor(Colors.Grey.Medium);
                });
            });

            // --- CONTENIDO / TABLA ---
            page.Content().PaddingVertical(20).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(50);  // ID
                    columns.RelativeColumn();    // Nombre de Categoría
                    columns.ConstantColumn(100); // Estado
                });

                // Cabecera
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("ID");
                    header.Cell().Element(CellStyle).Text("Nombre de Categoría");
                    header.Cell().Element(CellStyle).AlignRight().Text("Estado");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.Bold())
                                        .PaddingVertical(5)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Black);
                    }
                });

                // Filas (Usando tus propiedades exactas)
                foreach (var item in _categorias)
                {
                    table.Cell().Element(RowStyle).Text($"#{item.ID_CATEGORIA}");
                    table.Cell().Element(RowStyle).Text(item.NOMBRE_CATEGORIA ?? "Sin Nombre");
                    table.Cell().Element(RowStyle).AlignRight()
                        .Text(item.ESTADO ? "Activo" : "Inactivo")
                        .FontColor(item.ESTADO ? "#28a745" : "#dc3545");

                    static IContainer RowStyle(IContainer container)
                    {
                        return container.BorderBottom(0.5f)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(5);
                    }
                }
            });

            // --- PIE DE PÁGINA ---
            page.Footer().AlignRight().Text(x =>
            {
                x.Span("Página ");
                x.CurrentPageNumber();
                x.Span(" de ");
                x.TotalPages();
            });
        });
    }
}