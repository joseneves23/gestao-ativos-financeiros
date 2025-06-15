using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace AtivosFinanceiros.Services.Reports
{
    public class PdfReportService
    {
        public byte[] GerarTabelaPdf<T>(
            string titulo,
            string[] colunas,
            List<T> dados,
            Func<T, object[]> mapLinha,
            DateOnly dataInicio,
            DateOnly dataFim)
        {
            // Validação dos parâmetros
            if (colunas == null)
                throw new ArgumentNullException(nameof(colunas));
    
            if (dados == null)
                throw new ArgumentNullException(nameof(dados));
    
            if (mapLinha == null)
                throw new ArgumentNullException(nameof(mapLinha));
            
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Text($"{titulo} ({dataInicio:dd/MM/yyyy} - {dataFim:dd/MM/yyyy})")
                                 .SemiBold().FontSize(16).FontColor(Colors.Blue.Medium);

                    page.Content().Table(table =>
                    {
                        // Define colunas
                        table.ColumnsDefinition(columns =>
                        {
                            foreach (var _ in colunas)
                                columns.RelativeColumn();
                        });

                        // Cabeçalho
                        table.Header(header =>
                        {
                            foreach (var col in colunas)
                            {
                                header.Cell().Element(CellStyle).Text(col).SemiBold();
                            }

                            static IContainer CellStyle(IContainer container) =>
                                container.DefaultTextStyle(x => x.SemiBold())
                                         .PaddingVertical(5)
                                         .Background(Colors.Grey.Lighten3);
                        });

                        // Dados
                        foreach (var item in dados)
                        {
                            var valores = mapLinha(item);
                            foreach (var val in valores)
                            {
                                table.Cell().Text(val?.ToString() ?? "");
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Gerado em: ")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);

                        x.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                            .SemiBold()
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    });
                });
            }).GeneratePdf();
        }
    }
}
