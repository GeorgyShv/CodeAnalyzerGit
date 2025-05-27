using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CodeAnalyzer.Models;
using CodeAnalyzer.Core;
using System.Text.Json;

namespace CodeAnalyzer.Pages
{
    public class AnalysisModel : PageModel
    {
        private readonly IEnumerable<ICodeAnalyzer> _analyzers;
        private readonly ILogger<AnalysisModel> _logger;

        public AnalysisModel(IEnumerable<ICodeAnalyzer> analyzers, ILogger<AnalysisModel> logger)
        {
            _analyzers = analyzers;
            _logger = logger;
        }

        public AnalysisResult? Result { get; private set; }
        public string? FileName { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? SourceCode { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Проверяем, есть ли уже результаты анализа в сессии
                var resultJson = HttpContext.Session.GetString("AnalysisResult");
                if (!string.IsNullOrEmpty(resultJson))
                {
                    Result = JsonSerializer.Deserialize<AnalysisResult>(resultJson);
                    FileName = HttpContext.Session.GetString("OriginalFileName");
                    SourceCode = HttpContext.Session.GetString("SourceCode");
                    return Page();
                }

                var filePath = HttpContext.Session.GetString("AnalyzedFilePath");
                var originalFileName = HttpContext.Session.GetString("OriginalFileName");
                SourceCode = HttpContext.Session.GetString("SourceCode");

                if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(originalFileName))
                {
                    ErrorMessage = "Файл не найден. Пожалуйста, загрузите файл снова.";
                    return Page();
                }

                FileName = originalFileName;
                var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

                // Логируем доступные анализаторы
                _logger.LogInformation("Доступные анализаторы: {Analyzers}", 
                    string.Join(", ", _analyzers.Select(a => a.GetType().Name)));

                // Проверяем, что у нас есть анализаторы
                if (!_analyzers.Any())
                {
                    _logger.LogError("Нет доступных анализаторов");
                    ErrorMessage = "Ошибка конфигурации: нет доступных анализаторов кода";
                    return Page();
                }

                // Выбираем подходящий анализатор
                var analyzer = _analyzers.FirstOrDefault(a => a.GetSupportedExtensions().Contains(extension));
                if (analyzer == null)
                {
                    _logger.LogWarning("Неподдерживаемый тип файла: {Extension}. Доступные расширения: {Extensions}", 
                        extension,
                        string.Join(", ", _analyzers.SelectMany(a => a.GetSupportedExtensions())));
                    ErrorMessage = $"Неподдерживаемый тип файла: {extension}. Поддерживаемые расширения: {string.Join(", ", _analyzers.SelectMany(a => a.GetSupportedExtensions()))}";
                    return Page();
                }

                _logger.LogInformation("Выбран анализатор: {Analyzer} для файла {FileName}", 
                    analyzer.GetType().Name, originalFileName);

                // Читаем содержимое файла
                var sourceCode = await System.IO.File.ReadAllTextAsync(filePath);

                // Анализируем код
                Result = await analyzer.AnalyzeAsync(sourceCode, originalFileName);

                // Сохраняем результаты в сессии
                resultJson = JsonSerializer.Serialize(Result);
                _logger.LogInformation("Сохранение результатов анализа в сессию: {ResultJson}", resultJson);
                HttpContext.Session.SetString("AnalysisResult", resultJson);

                // Удаляем временный файл
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не удалось удалить временный файл {FilePath}", filePath);
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при анализе файла");
                ErrorMessage = $"Произошла ошибка при анализе файла: {ex.Message}";
                return Page();
            }
        }
    }
} 