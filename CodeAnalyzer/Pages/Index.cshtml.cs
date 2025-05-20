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
        [Required(ErrorMessage = "Пожалуйста, выберите файл")]
        public IFormFile? UploadedFile { get; set; }

        public string? ErrorMessage { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IEnumerable<ICodeAnalyzer> analyzers)
        {
            _logger = logger;
            _analyzers = analyzers;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Пожалуйста, выберите файл для анализа";
                return Page();
            }

            if (UploadedFile == null)
            {
                ErrorMessage = "Файл не был загружен";
                return Page();
            }

            var extension = Path.GetExtension(UploadedFile.FileName).ToLowerInvariant();
            if (extension != ".cs" && extension != ".cpp" && extension != ".h" && extension != ".hpp")
            {
                ErrorMessage = "Поддерживаются только файлы .cs, .cpp, .h и .hpp";
                return Page();
            }

            // Сохраняем файл во временную директорию
            var tempPath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(tempPath))
            {
                UploadedFile.CopyTo(stream);
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
