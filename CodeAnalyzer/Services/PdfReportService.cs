using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CodeAnalyzer.Models;

namespace CodeAnalyzer.Services
{
    public interface IPdfReportService
    {
        byte[] GenerateReport(AnalysisResult result);
    }

    public class PdfReportService : IPdfReportService
    {
        private readonly IChartImageService _chartImageService;

        public PdfReportService(IChartImageService chartImageService)
        {
            _chartImageService = chartImageService;
        }

        public byte[] GenerateReport(AnalysisResult result)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(x => ComposeContent(x, result));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Отчет по анализу кода").FontSize(20).SemiBold().FontFamily("Arial");
                    column.Item().Text(text =>
                    {
                        text.Span("Дата генерации: ").SemiBold().FontFamily("Arial");
                        text.Span($"{DateTime.Now:dd.MM.yyyy HH:mm}").FontFamily("Arial");
                    });
                });
            });
        }

        private void ComposeContent(IContainer container, AnalysisResult result)
        {
            container.Column(column =>
            {
                // Информация о файле
                column.Item().Text("Информация о файле").FontSize(16).SemiBold().FontFamily("Arial");
                column.Item().Text($"Имя файла: {result.FileName}").FontFamily("Arial");
                column.Item().Text($"Язык программирования: {result.Language}").FontFamily("Arial");
                column.Item().PaddingBottom(10);

                // Основные метрики
                column.Item().Text("Основные метрики").FontSize(16).SemiBold().FontFamily("Arial");
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });
                    table.Header(header =>
                    {
                        header.Cell().Text("Всего строк").SemiBold().FontFamily("Arial");
                        header.Cell().Text("Строк кода").SemiBold().FontFamily("Arial");
                        header.Cell().Text("Комментариев").SemiBold().FontFamily("Arial");
                        header.Cell().Text("Пустых строк").SemiBold().FontFamily("Arial");
                    });
                    table.Cell().Text(result.TotalLines.ToString()).FontFamily("Arial");
                    table.Cell().Text(result.CodeLines.ToString()).FontFamily("Arial");
                    table.Cell().Text(result.CommentLines.ToString()).FontFamily("Arial");
                    table.Cell().Text(result.EmptyLines.ToString()).FontFamily("Arial");
                });
                column.Item().PaddingBottom(10);

                // Метрики Холстеда
                column.Item().Text("Метрики Холстеда").FontSize(16).SemiBold().FontFamily("Arial");
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });
                    AddMetricRow(table, "Уникальные операторы (n1)", result.HalsteadMetrics.UniqueOperators.ToString());
                    AddMetricRow(table, "Уникальные операнды (n2)", result.HalsteadMetrics.UniqueOperands.ToString());
                    AddMetricRow(table, "Всего операторов (N1)", result.HalsteadMetrics.TotalOperators.ToString());
                    AddMetricRow(table, "Всего операндов (N2)", result.HalsteadMetrics.TotalOperands.ToString());
                    AddMetricRow(table, "Словарь (n)", result.HalsteadMetrics.Vocabulary.ToString("F0"));
                    AddMetricRow(table, "Длина (N)", result.HalsteadMetrics.ProgramLength.ToString("F0"));
                    AddMetricRow(table, "Объем (V)", result.HalsteadMetrics.Volume.ToString("F2"));
                    AddMetricRow(table, "Сложность (D)", result.HalsteadMetrics.Difficulty.ToString("F2"));
                    AddMetricRow(table, "Усилия (E)", result.HalsteadMetrics.Effort.ToString("F2"));
                    AddMetricRow(table, "Время (T)", result.HalsteadMetrics.Time.ToString("F2"));
                    AddMetricRow(table, "Ошибки (B)", result.HalsteadMetrics.Bugs.ToString("F2"));
                });

                // График метрик Холстеда
                var halsteadChartImage = _chartImageService.GenerateHalsteadChart(result.HalsteadMetrics);
                if (halsteadChartImage.Length > 0)
                {
                    column.Item().PaddingTop(10);
                    column.Item().Text("Визуализация метрик Холстеда").FontSize(14).SemiBold().FontFamily("Arial");
                    column.Item().Image(halsteadChartImage);
                }

                column.Item().PaddingBottom(10);

                // Метрики Чепина
                column.Item().Text("Метрики Чепина").FontSize(16).SemiBold().FontFamily("Arial");
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });
                    AddMetricRow(table, "Входные переменные", result.ChepinMetrics.InputVariables.ToString());
                    AddMetricRow(table, "Модифицируемые", result.ChepinMetrics.ModifiedVariables.ToString());
                    AddMetricRow(table, "Управляющие", result.ChepinMetrics.ControlVariables.ToString());
                    AddMetricRow(table, "Неиспользуемые", result.ChepinMetrics.UnusedVariables.ToString());
                });

                // График метрик Чепина
                var chepinChartImage = _chartImageService.GenerateChepinChart(result.ChepinMetrics);
                if (chepinChartImage.Length > 0)
                {
                    column.Item().PaddingTop(10);
                    column.Item().Text("Визуализация метрик Чепина").FontSize(14).SemiBold().FontFamily("Arial");
                    column.Item().Image(chepinChartImage);
                }

                // Круговая диаграмма метрик Чепина
                var chepinPieChartImage = _chartImageService.GenerateChepinPieChart(result.ChepinMetrics);
                if (chepinPieChartImage.Length > 0)
                {
                    column.Item().PaddingTop(10);
                    column.Item().Text("Распределение переменных по метрике Чепина").FontSize(14).SemiBold().FontFamily("Arial");
                    column.Item().Image(chepinPieChartImage);
                }

                column.Item().PaddingBottom(10);

                // График предсказания ошибок
                var errorPredictionChartImage = _chartImageService.GenerateErrorPredictionChart(result.HalsteadMetrics);
                if (errorPredictionChartImage.Length > 0)
                {
                    column.Item().PaddingTop(10);
                    column.Item().Text("Предсказание ошибок").FontSize(14).SemiBold().FontFamily("Arial");
                    column.Item().Image(errorPredictionChartImage);
                }

                column.Item().PaddingBottom(10);

                // Предупреждения
                if (result.Warnings.Any())
                {
                    column.Item().PaddingTop(10);
                    column.Item().Text("Предупреждения").FontSize(16).SemiBold().FontFamily("Arial");
                    column.Item().Background(Colors.Orange.Lighten5).Padding(10).Column(column =>
                    {
                        foreach (var warning in result.Warnings)
                        {
                            column.Item().Text($"• {warning}").FontFamily("Arial");
                        }
                    });
                }
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("Страница ").FontFamily("Arial");
                    text.CurrentPageNumber().FontFamily("Arial");
                    text.Span(" из ").FontFamily("Arial");
                    text.TotalPages().FontFamily("Arial");
                });
                row.RelativeItem().AlignRight().Text("Code Analyzer").FontFamily("Arial");
            });
        }

        private void AddMetricRow(TableDescriptor table, string name, string value)
        {
            table.Cell().Text(name).SemiBold().FontFamily("Arial");
            table.Cell().Text(value).FontFamily("Arial");
        }
    }
} 