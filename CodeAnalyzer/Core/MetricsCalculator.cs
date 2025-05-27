namespace CodeAnalyzer.Core
{
    public static class MetricsCalculator
    {
        private const double S = 18.0; // Число Страуда

        public static double CalculateHalsteadVolume(int uniqueOperators, int uniqueOperands)
        {
            double vocabulary = uniqueOperators + uniqueOperands;
            double length = uniqueOperators + uniqueOperands;
            if (vocabulary == 0) return 0;
            return length * Math.Log2(vocabulary);
        }

        public static double CalculateHalsteadDifficulty(int uniqueOperators, int uniqueOperands, int totalOperators, int totalOperands)
        {
            if (uniqueOperands == 0) return 0;
            return (uniqueOperators * totalOperands) / (2.0 * uniqueOperands);
        }

        public static double CalculateHalsteadEffort(double volume, double difficulty)
        {
            return volume * difficulty;
        }

        public static double CalculateHalsteadTime(double effort)
        {
            return effort / S;
        }

        public static double CalculateHalsteadBugs(double volume)
        {
            return volume / 3000.0;
        }

        public static double CalculateHalsteadPotentialVolume(int numberOfInputOutputParameters)
        {
            double n2Star = numberOfInputOutputParameters;
            if (n2Star + 2 <= 0) return 0;
            return (n2Star + 2) * Math.Log2(n2Star + 2);
        }

        public static double CalculateHalsteadLevel(double volume, double potentialVolume)
        {
            if (volume == 0) return 0;
            return potentialVolume / volume;
        }

        public static double CalculateHalsteadLanguageLevel(double programLevel, double potentialVolume)
        {
            return programLevel * potentialVolume;
        }

        public static double CalculateHalsteadProgrammingEffort(double volume, double programLevel)
        {
            if (programLevel == 0) return 0;
            return volume / programLevel;
        }

        public static double CalculateChepinComplexity(int inputVars, int modifiedVars, int controlVars, int unusedVars)
        {
            return inputVars + 2 * modifiedVars + 3 * controlVars + 0.5 * unusedVars;
        }

        public static double CalculateGilbMaintainabilityIndex(int cyclomaticComplexity, int linesOfCode, int commentLines)
        {
            if (linesOfCode == 0) return 0;
            
            double volume = Math.Log2(cyclomaticComplexity + 1);
            double commentWeight = 100.0 * commentLines / linesOfCode;
            
            return 171 - 5.2 * volume - 0.23 * cyclomaticComplexity - 16.2 * Math.Log2(linesOfCode) + 50 * Math.Sin(Math.Sqrt(2.4 * commentWeight));
        }

        public static double CalculateGilbCodeQuality(double maintainabilityIndex, double cyclomaticComplexity)
        {
            double normalizedMaintainability = Math.Max(0, Math.Min(100, maintainabilityIndex));
            double normalizedComplexity = Math.Max(0, Math.Min(1, 1 - (cyclomaticComplexity / 50)));

            return (normalizedMaintainability * 0.7 + normalizedComplexity * 30) / 100;
        }
    }
} 