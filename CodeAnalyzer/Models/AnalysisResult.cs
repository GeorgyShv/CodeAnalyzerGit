using System;
using System.Collections.Generic;

namespace CodeAnalyzer.Models
{
    public class AnalysisResult
    {
        public string FileName { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public int TotalLines { get; set; }
        public int CodeLines { get; set; }
        public int CommentLines { get; set; }
        public int EmptyLines { get; set; }
        
        // Дополнительная информация
        public long FileSize { get; set; }
        public string AnalysisDate { get; set; } = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

        // Метрики в структурированном виде
        public HalsteadMetrics HalsteadMetrics { get; set; } = new();
        public McCabeMetrics McCabeMetrics { get; set; } = new();
        public GilbMetrics GilbMetrics { get; set; } = new();
        public ChepinMetrics ChepinMetrics { get; set; } = new();

        // Дополнительные параметры для метрик Холстеда
        public int NumberOfInputOutputParameters { get; set; } // n2*

        // Предупреждения
        public List<string> Warnings { get; set; } = new();
    }

    public class HalsteadMetrics
    {
        public int UniqueOperators { get; set; }
        public int UniqueOperands { get; set; }
        public int TotalOperators { get; set; }
        public int TotalOperands { get; set; }
        public double ProgramLength { get; set; }
        public double Vocabulary { get; set; }
        public double Volume { get; set; }
        public double Difficulty { get; set; }
        public double Effort { get; set; }
        public double Time { get; set; }
        public double Bugs { get; set; }
        public Dictionary<string, int> OperatorFrequency { get; set; } = new();
        public Dictionary<string, int> OperandFrequency { get; set; } = new();

        // Дополнительные метрики Холстеда
        public double PotentialVolume { get; set; } // V*
        public double ProgramLevel { get; set; } // L
        public double LanguageLevel { get; set; } // lambda
        public double ProgrammingEffort { get; set; } // E
    }

    public class McCabeMetrics
    {
        public int CyclomaticComplexity { get; set; }
        public int EssentialComplexity { get; set; }
        public int DesignComplexity { get; set; }
    }

    public class GilbMetrics
    {
        public double MaintainabilityIndex { get; set; }
        public double CodeQuality { get; set; }
    }

    public class ChepinMetrics
    {
        public int InputVariables { get; set; }  // P
        public int ModifiedVariables { get; set; }  // M
        public int ControlVariables { get; set; }  // C
        public int UnusedVariables { get; set; }  // T
        public double Complexity { get; set; }  // Q
        public Dictionary<string, string> VariableTypes { get; set; } = new();  // Типы переменных
    }
} 