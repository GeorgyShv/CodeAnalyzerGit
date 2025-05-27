using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using CodeAnalyzer.Models;
using CodeAnalyzer.Core;
using System.ComponentModel.DataAnnotations;

namespace CodeAnalyzer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IEnumerable<ICodeAnalyzer> _analyzers;

        [BindProperty]
        [Required(ErrorMessage = "Пожалуйста, выберите файл для анализа")]
        public IFormFile? UploadedFile { get; set; }

        public string? ErrorMessage { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IEnumerable<ICodeAnalyzer> analyzers)
        {
            _logger = logger;
            _analyzers = analyzers;
        }

        public void OnGet()
        {
            // Очищаем результаты предыдущего анализа при загрузке главной страницы
            HttpContext.Session.Remove("AnalysisResult");
            HttpContext.Session.Remove("AnalyzedFilePath");
            HttpContext.Session.Remove("OriginalFileName");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (UploadedFile == null)
            {
                ModelState.AddModelError("UploadedFile", "Пожалуйста, выберите файл для анализа");
                return Page();
            }

            // Создаем временную директорию, если она не существует
            var tempDir = Path.Combine(Path.GetTempPath(), "CodeAnalyzer");
            Directory.CreateDirectory(tempDir);

            // Генерируем уникальное имя файла
            var tempPath = Path.Combine(tempDir, $"{Guid.NewGuid()}{Path.GetExtension(UploadedFile.FileName)}");

            // Сохраняем файл
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await UploadedFile.CopyToAsync(stream);
            }

            // Сохраняем путь к файлу в сессии
            HttpContext.Session.SetString("AnalyzedFilePath", tempPath);
            HttpContext.Session.SetString("OriginalFileName", UploadedFile.FileName);

            return RedirectToPage("/Analysis");
        }

        public IActionResult OnPostToggleTheme()
        {
            var currentTheme = Request.Cookies["theme"] ?? "dark";
            var newTheme = currentTheme == "dark" ? "light" : "dark";
            
            Response.Cookies.Append("theme", newTheme, new CookieOptions
            {
                Expires = DateTime.Now.AddYears(1),
                IsEssential = true
            });

            return RedirectToPage();
        }
    }
}
