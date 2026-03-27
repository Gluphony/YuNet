using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Entidades;

public class ProductoReporte : IDocument
{
    private readonly List<Producto> _productos;

    public ProductoReporte(List<Producto> productos)
    {
        _productos = productos;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(40);

            // Cabecera Neón YuNet
            page.Header().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("YuNet STORE").FontSize(22).SemiBold().FontColor("#00bcd4");
                    col.Item().Text("Inventario General de Productos").FontSize(12).Italic();
                });
                row.RelativeItem().AlignRight().Text(DateTime.Now.ToString("dd/MM/yyyy")).FontSize(10);
            });

            page.Content().PaddingVertical(20).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);  // ID
                    columns.RelativeColumn(3);   // Modelo/Producto
                    columns.RelativeColumn(1);   // Precio
                    columns.RelativeColumn(1);   // Stock
                    columns.RelativeColumn(1);   // Estado
                });

                table.Header(header =>
                {
                    header.Cell().BorderBottom(1).Text("ID").Bold();
                    header.Cell().BorderBottom(1).Text("Modelo").Bold();
                    header.Cell().BorderBottom(1).AlignRight().Text("Precio").Bold();
                    header.Cell().BorderBottom(1).AlignCenter().Text("Stock").Bold();
                    header.Cell().BorderBottom(1).AlignRight().Text("Estado").Bold();
                });

                foreach (var item in _productos)
                {
                    table.Cell().PaddingVertical(5).Text(item.ID_PRODUCTO.ToString());
                    table.Cell().PaddingVertical(5).Text(item.MODELO ?? "N/A");
                    table.Cell().PaddingVertical(5).AlignRight().Text(item.PRECIO.ToString("C"));
                    table.Cell().PaddingVertical(5).AlignCenter().Text(item.EXISTENCIAS.ToString());
                    table.Cell().PaddingVertical(5).AlignRight()
                        .Text(item.ESTADO ? "Activo" : "Inactivo")
                        .FontColor(item.ESTADO ? "#28a745" : "#dc3545");
                }
            });

            page.Footer().AlignCenter().Text(x => {
                x.Span("Página "); x.CurrentPageNumber();
            });
        });
    }
}