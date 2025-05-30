using System.Text.Json;
using CodeAnalyzer.Models;
using ScottPlot;

namespace CodeAnalyzer.Services
{
    public interface IChartImageService
    {
        byte[] GenerateHalsteadChart(HalsteadMetrics metrics);
        byte[] GenerateGilbChart(GilbMetrics metrics);
        byte[] GenerateChepinChart(ChepinMetrics metrics);
        byte[] GenerateErrorPredictionChart(HalsteadMetrics metrics);
        byte[] GenerateChepinPieChart(ChepinMetrics metrics);
    }

    public class ChartImageService : IChartImageService
    {
        private readonly MetricsVisualizationService _visualizationService;

        public ChartImageService(MetricsVisualizationService visualizationService)
        {
            _visualizationService = visualizationService;
        }

        public byte[] GenerateHalsteadChart(HalsteadMetrics metrics)
        {
            var plt = new Plot(800, 400);
            
            // Создаем данные для графика
            double[] values = new double[]
            {
                metrics.Volume,
                metrics.Difficulty,
                metrics.Effort,
                metrics.Time,
                metrics.Bugs
            };
            
            string[] labels = new string[]
            {
                "Объем (V)",
                "Сложность (D)",
                "Усилия (E)",
                "Время (T)",
                "Ошибки (B)"
            };

            // Создаем столбчатую диаграмму
            var bar = plt.AddBar(values);
            bar.FillColor = System.Drawing.Color.Blue;

            // Настраиваем оси
            plt.XTicks(Enumerable.Range(0, labels.Length).Select(x => (double)x).ToArray(), labels);
            plt.YLabel("Значение");
            plt.Title("Метрики Холстеда");

            // Сохраняем график в массив байтов
            return plt.GetImageBytes();
        }

        public byte[] GenerateGilbChart(GilbMetrics metrics)
        {
            var plt = new Plot(800, 400);
            
            // Создаем данные для графика
            double[] values = new double[]
            {
                metrics.MaintainabilityIndex,
                metrics.CodeQuality
            };
            
            string[] labels = new string[]
            {
                "Индекс поддерживаемости",
                "Качество кода"
            };

            // Создаем столбчатую диаграмму
            var bar = plt.AddBar(values);
            bar.FillColor = System.Drawing.Color.Green;

            // Настраиваем оси
            plt.XTicks(Enumerable.Range(0, labels.Length).Select(x => (double)x).ToArray(), labels);
            plt.YLabel("Значение");
            plt.Title("Метрики Гилба");

            // Сохраняем график в массив байтов
            return plt.GetImageBytes();
        }

        public byte[] GenerateChepinChart(ChepinMetrics metrics)
        {
            var plt = new Plot(800, 400);
            
            // Создаем данные для графика
            double[] values = new double[]
            {
                metrics.InputVariables,
                metrics.ModifiedVariables,
                metrics.ControlVariables,
                metrics.UnusedVariables
            };
            
            string[] labels = new string[]
            {
                "Входные",
                "Модифицируемые",
                "Управляющие",
                "Неиспользуемые"
            };

            // Создаем столбчатую диаграмму
            var bar = plt.AddBar(values);
            bar.FillColor = System.Drawing.Color.Orange;

            // Настраиваем оси
            plt.XTicks(Enumerable.Range(0, labels.Length).Select(x => (double)x).ToArray(), labels);
            plt.YLabel("Количество переменных");
            plt.Title("Метрики Чепина");

            // Сохраняем график в массив байтов
            return plt.GetImageBytes();
        }

        public byte[] GenerateChepinPieChart(ChepinMetrics metrics)
        {
            var plt = new Plot(800, 400);
            
            // Создаем данные для графика
            double[] values = new double[]
            {
                metrics.InputVariables,
                metrics.ModifiedVariables,
                metrics.ControlVariables,
                metrics.UnusedVariables
            };
            
            string[] labels = new string[]
            {
                "Входные (P)",
                "Модифицируемые (M)",
                "Управляющие (C)",
                "Неиспользуемые (T)"
            };

            // Создаем круговую диаграмму
            var pie = plt.AddPie(values);
            pie.SliceLabels = labels;
            pie.SliceFillColors = new System.Drawing.Color[]
            {
                System.Drawing.Color.LightBlue,
                System.Drawing.Color.LightGreen,
                System.Drawing.Color.LightSalmon,
                System.Drawing.Color.LightGray
            };

            // Настраиваем заголовок
            plt.Title("Распределение переменных по метрике Чепина");

            // Сохраняем график в массив байтов
            return plt.GetImageBytes();
        }

        public byte[] GenerateErrorPredictionChart(HalsteadMetrics metrics)
        {
            var plt = new Plot(800, 400);
            
            // Создаем данные для графика
            double[] values = new double[]
            {
                metrics.Bugs,
                metrics.Volume / 3000.0 // Предсказанное количество ошибок
            };
            
            string[] labels = new string[]
            {
                "Фактические",
                "Предсказанные"
            };

            // Создаем столбчатую диаграмму
            var bar = plt.AddBar(values);
            bar.FillColor = System.Drawing.Color.Red;

            // Настраиваем оси
            plt.XTicks(Enumerable.Range(0, labels.Length).Select(x => (double)x).ToArray(), labels);
            plt.YLabel("Количество ошибок");
            plt.Title("Предсказание ошибок");

            // Сохраняем график в массив байтов
            return plt.GetImageBytes();
        }
    }
} 