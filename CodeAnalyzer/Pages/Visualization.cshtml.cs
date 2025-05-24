using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CodeAnalyzer.Services;
using CodeAnalyzer.Models;

namespace CodeAnalyzer.Pages
{
    public class VisualizationModel : PageModel
    {
        private readonly MetricsVisualizationService _visualizationService;

        public VisualizationModel(MetricsVisualizationService visualizationService)
        {
            _visualizationService = visualizationService;
        }

        public void OnGet()
        {
            // Страница загружается через AJAX
        }
    }
} 