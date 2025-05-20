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

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var filePath = HttpContext.Session.GetString("AnalyzedFilePath");
                var originalFileName = HttpContext.Session.GetString("OriginalFileName");

                if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(originalFileName))
                {
                    ErrorMessage = "Файл не найден. Пожалуйста, загрузите файл снова.";
                    return Page();
                }

                FileName = originalFileName;
                var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

                // Выбираем подходящий анализатор
                var analyzer = _analyzers.FirstOrDefault(a => a.GetSupportedExtensions().Contains(extension));
                if (analyzer == null)
                {
                    ErrorMessage = $"Неподдерживаемый тип файла: {extension}";
                    return Page();
                }

                // Читаем содержимое файла
                var sourceCode = await System.IO.File.ReadAllTextAsync(filePath);

                // Анализируем код
                Result = await analyzer.AnalyzeAsync(sourceCode, originalFileName);

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
                ErrorMessage = "Произошла ошибка при анализе файла. Пожалуйста, попробуйте снова.";
                return Page();
            }
        }
    }
} 