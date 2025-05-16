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
        
        // Метрики Холстеда
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

        // Метрики МакКейба
        public int CyclomaticComplexity { get; set; }
        public int EssentialComplexity { get; set; }
        public int DesignComplexity { get; set; }

        // Метрики Джилба
        public double MaintainabilityIndex { get; set; }
        public double CodeQuality { get; set; }

        // Дополнительные метрики
        public Dictionary<string, int> OperatorFrequency { get; set; } = new();
        public Dictionary<string, int> OperandFrequency { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
} 