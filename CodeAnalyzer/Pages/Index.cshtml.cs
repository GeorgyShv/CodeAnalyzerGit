using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using CodeAnalyzer.Models;
using CodeAnalyzer.Core;

namespace CodeAnalyzer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ICodeAnalyzer _codeAnalyzer;

        [BindProperty]
        public string FileContent { get; set; } = string.Empty;

        [BindProperty]
        public string FileName { get; set; } = string.Empty;

        public IndexModel(ILogger<IndexModel> logger, ICodeAnalyzer codeAnalyzer)
        {
            _logger = logger;
            _codeAnalyzer = codeAnalyzer;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Page();
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (extension != ".cs")
                {
                    return Page();
                }

                // Чтение файла
                using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
                FileContent = await reader.ReadToEndAsync();
                FileName = file.FileName;

                // Базовый анализ
                var lines = FileContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                var result = new AnalysisResult
                {
                    FileName = FileName,
                    Language = "C#",
                    TotalLines = lines.Length,
                    CodeLines = lines.Count(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("//") && !line.TrimStart().StartsWith("/*")),
                    CommentLines = lines.Count(line => line.TrimStart().StartsWith("//") || line.TrimStart().StartsWith("/*") || line.TrimStart().StartsWith("*")),
                    EmptyLines = lines.Count(line => string.IsNullOrWhiteSpace(line))
                };

                // Сохраняем результат в сессию
                HttpContext.Session.SetString("AnalysisResult", System.Text.Json.JsonSerializer.Serialize(result));

                return RedirectToPage("./Analysis", new { fileName = FileName });
            }
            catch (Exception)
            {
                return Page();
            }
        }
    }
}
