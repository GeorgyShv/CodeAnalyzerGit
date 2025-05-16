using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CodeAnalyzer.Models;
using System.Text.Json;

namespace CodeAnalyzer.Pages
{
    public class AnalysisModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string FileName { get; set; } = string.Empty;

        // Общая информация
        public string Language { get; set; } = string.Empty;
        public int TotalLines { get; set; }
        public int CodeLines { get; set; }
        public int CommentLines { get; set; }
        public int EmptyLines { get; set; }

        // Метрики Холстеда
        public int UniqueOperators { get; set; }
        public int UniqueOperands { get; set; }
        public double Volume { get; set; }
        public double Difficulty { get; set; }
        public double Effort { get; set; }
        public double Time { get; set; }
        public double Bugs { get; set; }

        // Метрики МакКейба
        public int CyclomaticComplexity { get; set; }
        public int EssentialComplexity { get; set; }
        public int DesignComplexity { get; set; }

        // Метрики Джилба
        public double MaintainabilityIndex { get; set; }
        public double CodeQuality { get; set; }

        // Предупреждения
        public List<string> Warnings { get; set; } = new();

        public IActionResult OnGet()
        {
            if (string.IsNullOrEmpty(FileName))
            {
                return RedirectToPage("./Index");
            }

            var analysisResultJson = HttpContext.Session.GetString("AnalysisResult");
            if (string.IsNullOrEmpty(analysisResultJson))
            {
                return RedirectToPage("./Index");
            }

            try
            {
                var result = JsonSerializer.Deserialize<AnalysisResult>(analysisResultJson);
                if (result == null)
                {
                    return RedirectToPage("./Index");
                }

                // Заполняем свойства модели из результата анализа
                Language = result.Language;
                TotalLines = result.TotalLines;
                CodeLines = result.CodeLines;
                CommentLines = result.CommentLines;
                EmptyLines = result.EmptyLines;

                UniqueOperators = result.UniqueOperators;
                UniqueOperands = result.UniqueOperands;
                Volume = result.Volume;
                Difficulty = result.Difficulty;
                Effort = result.Effort;
                Time = result.Time;
                Bugs = result.Bugs;

                CyclomaticComplexity = result.CyclomaticComplexity;
                EssentialComplexity = result.EssentialComplexity;
                DesignComplexity = result.DesignComplexity;

                MaintainabilityIndex = result.MaintainabilityIndex;
                CodeQuality = result.CodeQuality;

                Warnings = result.Warnings;

                return Page();
            }
            catch (Exception)
            {
                return RedirectToPage("./Index");
            }
        }
    }
} 