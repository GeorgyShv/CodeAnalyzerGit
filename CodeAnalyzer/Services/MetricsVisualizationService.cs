using CodeAnalyzer.Models;
using System.Collections.Generic;

namespace CodeAnalyzer.Services
{
    public class MetricsVisualizationService
    {
        public object PrepareHalsteadData(HalsteadMetrics metrics)
        {
            return new
            {
                programLength = metrics.ProgramLength,
                programVolume = metrics.ProgramVolume,
                potentialVolume = metrics.PotentialVolume
            };
        }

        public object PrepareGilbData(GilbMetrics metrics)
        {
            return new
            {
                absoluteComplexity = metrics.AbsoluteComplexity,
                relativeComplexity = metrics.RelativeComplexity,
                moduleCount = metrics.ModuleCount,
                moduleConnections = metrics.ModuleConnections
            };
        }

        public object PrepareChepinData(ChepinMetrics metrics)
        {
            return new
            {
                inputVariables = metrics.InputVariables,
                modifiedVariables = metrics.ModifiedVariables,
                controlVariables = metrics.ControlVariables,
                parasiticVariables = metrics.ParasiticVariables
            };
        }

        public object PrepareErrorPredictionData(List<AnalysisResult> results)
        {
            return new
            {
                volumes = results.Select(r => r.HalsteadMetrics.ProgramVolume).ToList(),
                errors = results.Select(r => r.HalsteadMetrics.EstimatedErrors).ToList()
            };
        }
    }
} 