using Microsoft.AspNetCore.Mvc;
using CodeAnalyzer.Services;
using CodeAnalyzer.Models;
using CodeAnalyzer.Core;
using System.Collections.Generic;
using System.Text.Json;

namespace CodeAnalyzer.Controllers
{
    [ApiController]
    [Route("api/metrics")]
    public class MetricsController : ControllerBase
    {
        private readonly MetricsVisualizationService _visualizationService;
        private readonly ILogger<MetricsController> _logger;

        public MetricsController(
            MetricsVisualizationService visualizationService,
            ILogger<MetricsController> logger)
        {
            _visualizationService = visualizationService;
            _logger = logger;
        }

        [HttpGet("halstead")]
        public IActionResult GetHalsteadMetrics()
        {
            try
            {
                _logger.LogInformation("Получение метрик Холстеда");
                var resultJson = HttpContext.Session.GetString("AnalysisResult");
                _logger.LogInformation("Данные из сессии: {ResultJson}", resultJson);

                if (string.IsNullOrEmpty(resultJson))
                {
                    _logger.LogWarning("Результаты анализа не найдены в сессии");
                    return NotFound(new { error = "Результаты анализа не найдены" });
                }

                var result = JsonSerializer.Deserialize<AnalysisResult>(resultJson);
                if (result == null)
                {
                    _logger.LogWarning("Не удалось десериализовать результаты анализа");
                    return NotFound(new { error = "Не удалось десериализовать результаты анализа" });
                }

                _logger.LogInformation("Метрики Холстеда успешно получены");
                var data = _visualizationService.PrepareHalsteadData(result.HalsteadMetrics);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении метрик Холстеда");
                return StatusCode(500, new { error = "Ошибка при получении метрик Холстеда", message = ex.Message });
            }
        }

        [HttpGet("gilb")]
        public IActionResult GetGilbMetrics()
        {
            try
            {
                var resultJson = HttpContext.Session.GetString("AnalysisResult");
                if (string.IsNullOrEmpty(resultJson))
                {
                    return NotFound(new { error = "Результаты анализа не найдены" });
                }

                var result = JsonSerializer.Deserialize<AnalysisResult>(resultJson);
                if (result == null)
                {
                    return NotFound(new { error = "Не удалось десериализовать результаты анализа" });
                }

                var data = _visualizationService.PrepareGilbData(result.GilbMetrics);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении метрик Джилба");
                return StatusCode(500, new { error = "Ошибка при получении метрик Джилба", message = ex.Message });
            }
        }

        [HttpGet("chepin")]
        public IActionResult GetChepinMetrics()
        {
            try
            {
                var resultJson = HttpContext.Session.GetString("AnalysisResult");
                if (string.IsNullOrEmpty(resultJson))
                {
                    return NotFound(new { error = "Результаты анализа не найдены" });
                }

                var result = JsonSerializer.Deserialize<AnalysisResult>(resultJson);
                if (result == null)
                {
                    return NotFound(new { error = "Не удалось десериализовать результаты анализа" });
                }

                var data = _visualizationService.PrepareChepinData(result.ChepinMetrics);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении метрик Чепина");
                return StatusCode(500, new { error = "Ошибка при получении метрик Чепина", message = ex.Message });
            }
        }

        [HttpGet("error-prediction")]
        public IActionResult GetErrorPredictionData()
        {
            try
            {
                var resultJson = HttpContext.Session.GetString("AnalysisResult");
                if (string.IsNullOrEmpty(resultJson))
                {
                    return NotFound(new { error = "Результаты анализа не найдены" });
                }

                var result = JsonSerializer.Deserialize<AnalysisResult>(resultJson);
                if (result == null)
                {
                    return NotFound(new { error = "Не удалось десериализовать результаты анализа" });
                }

                var data = _visualizationService.PrepareErrorPredictionData(new List<AnalysisResult> { result });
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных для предсказания ошибок");
                return StatusCode(500, new { error = "Ошибка при получении данных для предсказания ошибок", message = ex.Message });
            }
        }
    }
} 