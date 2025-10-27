using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TiendaVirtual.Model;

namespace TiendaVirtual.Service;

public class ReciboCompraDocument : IDocument
{
    private readonly ReciboCompra recibo;

    public ReciboCompraDocument(ReciboCompra recibo)
    {
        this.recibo = recibo;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(30);
            page.Size(PageSizes.A4);
            page.DefaultTextStyle(x => x.FontSize(12));

            page.Header().Text("Recibo de Compra").FontSize(20).Bold().AlignCenter();

            page.Content().Column(col =>
            {
                col.Item().Text($"Fecha: {recibo.Fecha:g}");
                col.Item().Text($"Cliente ID: {recibo.ClienteId}");
                col.Item().Text($"Código: {recibo.CodigoAutenticacion}");

                col.Item().LineHorizontal(1);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                    });

                    table.Header(h =>
                    {
                        h.Cell().Text("Producto").Bold();
                        h.Cell().Text("Precio").Bold();
                        h.Cell().Text("Cantidad").Bold();
                        h.Cell().Text("Total").Bold();
                    });

                    foreach (var item in recibo.Items)
                    {
                        table.Cell().Text(item.Producto.Nombre);
                        table.Cell().Text($"${item.PrecioUnitario:F2}");
                        table.Cell().Text(item.Cantidad.ToString());
                        table.Cell().Text($"${(item.PrecioUnitario * item.Cantidad):F2}");
                    }
                });

                col.Item().LineHorizontal(1);
                col.Item().Text($"Total: ${recibo.Total}").Bold().FontSize(14);
            });

            page.Footer().AlignCenter().Text("Gracias por tu compra").Italic();
        });
    }
}

