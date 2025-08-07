using Hospital.Models.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class InvoiceReportDocument : IDocument
{
    public List<InvoiceReportItemModel> Items { get; }

    public InvoiceReportDocument(List<InvoiceReportItemModel> items)
    {
        Items = items;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(30);
            page.Size(PageSizes.A4);

            page.Content().Column(col =>
            {
                col.Spacing(10);

                col.Item().Text("BÁO CÁO HÓA ĐƠN").FontSize(18).Bold().AlignCenter();

                col.Item().Element(ComposeTable);
            });

            page.Footer().AlignCenter().Text($"Ngày xuất: {DateTime.Now:dd/MM/yyyy}");
        });
    }

    void ComposeTable(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1); // Mã HĐ
                columns.RelativeColumn(2); // Bệnh nhân
                columns.RelativeColumn(2); // Thu ngân
                columns.RelativeColumn(2); // Ngày tạo
                columns.RelativeColumn(2); // Tổng tiền
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Element(CellStyleHeader).Text("Mã HĐ");
                header.Cell().Element(CellStyleHeader).Text("Bệnh nhân");
                header.Cell().Element(CellStyleHeader).Text("Thu ngân");
                header.Cell().Element(CellStyleHeader).Text("Ngày tạo");
                header.Cell().Element(CellStyleHeader).AlignRight().Text("Tổng tiền");

                static IContainer CellStyleHeader(IContainer container) =>
                    container.DefaultTextStyle(x => x.SemiBold())
                             .PaddingVertical(5)
                             .BorderBottom(1)
                             .BorderColor(Colors.Black);
            });

            // Rows
            foreach (var i in Items)
            {
                table.Cell().Element(CellStyleRow).Text(i.InvoiceId.ToString());
                table.Cell().Element(CellStyleRow).Text(i.PatientName);
                table.Cell().Element(CellStyleRow).Text(i.CashierName);
                table.Cell().Element(CellStyleRow).Text(i.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
                table.Cell().Element(CellStyleRow).AlignRight().Text($"{i.TotalAmount:N0}đ");

                static IContainer CellStyleRow(IContainer container) =>
                    container.BorderBottom(1)
                             .BorderColor(Colors.Grey.Lighten2)
                             .PaddingVertical(5);
            }
        });
    }
}
