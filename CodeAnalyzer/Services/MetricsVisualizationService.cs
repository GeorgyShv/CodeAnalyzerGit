using CodeAnalyzer.Models;
using System.Collections.Generic;
using System.Linq;

namespace CodeAnalyzer.Services
{
    public class MetricsVisualizationService
    {
        public object PrepareHalsteadData(HalsteadMetrics metrics)
        {
            return new
            {
                programLength = metrics.ProgramLength,
                volume = metrics.Volume,
                difficulty = metrics.Difficulty,
                effort = metrics.Effort,
                time = metrics.Time,
                bugs = metrics.Bugs
            };
        }

        public object PrepareGilbData(GilbMetrics metrics)
        {
            return new
            {
                maintainabilityIndex = metrics.MaintainabilityIndex,
                codeQuality = metrics.CodeQuality
            };
        }

        public object PrepareChepinData(ChepinMetrics metrics)
        {
            return new
            {
                inputVariables = metrics.InputVariables,
                modifiedVariables = metrics.ModifiedVariables,
                controlVariables = metrics.ControlVariables,
                unusedVariables = metrics.UnusedVariables,
                complexity = metrics.Complexity
            };
        }

        public object PrepareErrorPredictionData(List<AnalysisResult> results)
        {
            return new
            {
                volumes = results.Select(r => r.HalsteadMetrics.Volume).ToList(),
                errors = results.Select(r => r.HalsteadMetrics.Bugs).ToList()
            };
        }
    }
} 