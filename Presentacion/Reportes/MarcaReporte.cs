using Entidades;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace Presentacion.Reportes
{
    public class MarcaReporte : IDocument
    {
        private readonly List<Marca> _marcas;

        public MarcaReporte(List<Marca> marcas)
        {
            _marcas = marcas;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(1, Unit.Centimetre);
                page.Header().Row(row =>
                {
                    row.RelativeItem().Text("YuNet STORE").FontSize(20).SemiBold().FontColor("#00bcd4");
                    row.RelativeItem().AlignRight().Text(DateTime.Now.ToString("dd/MM/yyyy"));
                });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);
                        columns.RelativeColumn();
                        columns.ConstantColumn(80);
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).Text("ID");
                        header.Cell().BorderBottom(1).Text("Nombre");
                        header.Cell().BorderBottom(1).Text("Estado");
                    });

                    foreach (var m in _marcas)
                    {
                        table.Cell().Text(m.ID_MARCA.ToString());
                        table.Cell().Text(m.NOMBRE);
                        table.Cell().Text(m.ESTADO ? "Activo" : "Inactivo");
                    }
                });
            });
        }
    }
}