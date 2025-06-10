using Microsoft.AspNetCore.Mvc;
using CodeAnalyzer.Services;
using CodeAnalyzer.Models;
using System.Text.Json;

namespace CodeAnalyzer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IPdfReportService _pdfReportService;

        public ReportController(IPdfReportService pdfReportService)
        {
            _pdfReportService = pdfReportService;
        }

        [HttpGet("pdf")]
        public IActionResult GeneratePdfReport()
        {
            var resultJson = HttpContext.Session.GetString("AnalysisResult");
            if (string.IsNullOrEmpty(resultJson))
                return NotFound("Результаты анализа не найдены");
            var result = JsonSerializer.Deserialize<AnalysisResult>(resultJson);
            if (result == null)
                return NotFound("Не удалось десериализовать результаты анализа");
            var pdfBytes = _pdfReportService.GenerateReport(result);
            var fileName = $"analysis_report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
    }
} 