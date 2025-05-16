namespace CodeAnalyzer.Core
{
    public static class MetricsCalculator
    {
        public static double CalculateHalsteadVolume(int uniqueOperators, int uniqueOperands)
        {
            return (uniqueOperators + uniqueOperands) * Math.Log2(uniqueOperators + uniqueOperands);
        }

        public static double CalculateHalsteadDifficulty(int uniqueOperators, int uniqueOperands, int totalOperators, int totalOperands)
        {
            return (uniqueOperators * totalOperands) / (2.0 * uniqueOperands);
        }

        public static double CalculateHalsteadEffort(double volume, double difficulty)
        {
            return volume * difficulty;
        }

        public static double CalculateHalsteadTime(double effort)
        {
            return effort / 18.0;
        }

        public static double CalculateHalsteadBugs(double volume)
        {
            return volume / 3000.0;
        }

        public static double CalculateMaintainabilityIndex(int cyclomaticComplexity, int linesOfCode, int commentLines)
        {
            double volume = Math.Log2(cyclomaticComplexity + 1);
            double commentWeight = 100 * commentLines / linesOfCode;
            
            return 171 - 5.2 * volume - 0.23 * cyclomaticComplexity - 16.2 * Math.Log2(linesOfCode) + 50 * Math.Sin(Math.Sqrt(2.4 * commentWeight));
        }

        public static double CalculateCodeQuality(double maintainabilityIndex, double cyclomaticComplexity)
        {
            // Нормализация значений
            double normalizedMaintainability = Math.Max(0, Math.Min(100, maintainabilityIndex));
            double normalizedComplexity = Math.Max(0, Math.Min(1, 1 - (cyclomaticComplexity / 50)));

            // Взвешенная оценка качества кода
            return (normalizedMaintainability * 0.7 + normalizedComplexity * 30) / 100;
        }
    }
} 