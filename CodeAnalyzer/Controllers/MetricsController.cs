using Microsoft.AspNetCore.Mvc;
using CodeAnalyzer.Services;
using CodeAnalyzer.Models;
using System.Collections.Generic;

namespace CodeAnalyzer.Controllers
{
    [ApiController]
    [Route("api/metrics")]
    public class MetricsController : ControllerBase
    {
        private readonly MetricsVisualizationService _visualizationService;
        private readonly ICodeAnalyzer _codeAnalyzer;

        public MetricsController(
            MetricsVisualizationService visualizationService,
            ICodeAnalyzer codeAnalyzer)
        {
            _visualizationService = visualizationService;
            _codeAnalyzer = codeAnalyzer;
        }

        [HttpGet("halstead")]
        public IActionResult GetHalsteadMetrics()
        {
            var metrics = _codeAnalyzer.GetHalsteadMetrics();
            var data = _visualizationService.PrepareHalsteadData(metrics);
            return Ok(data);
        }

        [HttpGet("gilb")]
        public IActionResult GetGilbMetrics()
        {
            var metrics = _codeAnalyzer.GetGilbMetrics();
            var data = _visualizationService.PrepareGilbData(metrics);
            return Ok(data);
        }

        [HttpGet("chepin")]
        public IActionResult GetChepinMetrics()
        {
            var metrics = _codeAnalyzer.GetChepinMetrics();
            var data = _visualizationService.PrepareChepinData(metrics);
            return Ok(data);
        }

        [HttpGet("error-prediction")]
        public IActionResult GetErrorPredictionData()
        {
            var results = _codeAnalyzer.GetAnalysisResults();
            var data = _visualizationService.PrepareErrorPredictionData(results);
            return Ok(data);
        }
    }
} 