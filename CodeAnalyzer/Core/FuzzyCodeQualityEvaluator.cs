using CodeAnalyzer.Models;

namespace CodeAnalyzer.Core
{
    public static class FuzzyCodeQualityEvaluator
    {
        private const double RuleVisibilityThreshold = 0.05;

        public static FuzzyQualityResult Evaluate(AnalysisResult result)
        {
            var chepinComplexity = result.ChepinMetrics.Complexity;
            var potentialBugs = result.HalsteadMetrics.Bugs;
            var maintainabilityIndex = result.GilbMetrics.MaintainabilityIndex;
            var commentRatio = result.CodeLines == 0
                ? 1.0
                : (double)result.CommentLines / result.CodeLines;
            var unusedVariables = result.ChepinMetrics.UnusedVariables;
            var relativeComplexity = result.RelativeComplexityCL;

            var inputMemberships = new Dictionary<string, double>
            {
                ["Сложность Чепина: низкая"] = LeftShoulder(chepinComplexity, 15, 30),
                ["Сложность Чепина: средняя"] = Triangle(chepinComplexity, 20, 45, 70),
                ["Сложность Чепина: высокая"] = Triangle(chepinComplexity, 55, 85, 115),
                ["Сложность Чепина: критическая"] = RightShoulder(chepinComplexity, 90, 120),

                ["Ошибки Холстеда: низкие"] = LeftShoulder(potentialBugs, 0.03, 0.08),
                ["Ошибки Холстеда: средние"] = Triangle(potentialBugs, 0.05, 0.12, 0.20),
                ["Ошибки Холстеда: высокие"] = RightShoulder(potentialBugs, 0.15, 0.30),

                ["Поддерживаемость: низкая"] = LeftShoulder(maintainabilityIndex, 45, 60),
                ["Поддерживаемость: средняя"] = Triangle(maintainabilityIndex, 50, 70, 85),
                ["Поддерживаемость: высокая"] = RightShoulder(maintainabilityIndex, 75, 90),

                ["Комментарии: недостаточные"] = LeftShoulder(commentRatio, 0.05, 0.12),
                ["Комментарии: приемлемые"] = Triangle(commentRatio, 0.08, 0.18, 0.30),
                ["Комментарии: хорошие"] = RightShoulder(commentRatio, 0.22, 0.35),

                ["Неиспользуемые переменные: отсутствуют"] = LeftShoulder(unusedVariables, 0, 1),
                ["Неиспользуемые переменные: есть"] = Triangle(unusedVariables, 0, 5, 15),
                ["Неиспользуемые переменные: много"] = RightShoulder(unusedVariables, 10, 30),

                ["Относительная сложность: низкая"] = LeftShoulder(relativeComplexity, 0.01, 0.03),
                ["Относительная сложность: средняя"] = Triangle(relativeComplexity, 0.02, 0.06, 0.10),
                ["Относительная сложность: высокая"] = RightShoulder(relativeComplexity, 0.08, 0.15)
            };

            var rules = new List<FuzzyRule>
            {
                new(
                    "Если сложность низкая, поддерживаемость высокая и ошибок мало, то качество высокое",
                    Min(
                        inputMemberships["Сложность Чепина: низкая"],
                        inputMemberships["Поддерживаемость: высокая"],
                        inputMemberships["Ошибки Холстеда: низкие"]),
                    0.90),
                new(
                    "Если поддерживаемость высокая, комментариев достаточно и нет неиспользуемых переменных, то качество высокое",
                    Min(
                        inputMemberships["Поддерживаемость: высокая"],
                        Max(
                            inputMemberships["Комментарии: приемлемые"],
                            inputMemberships["Комментарии: хорошие"]),
                        inputMemberships["Неиспользуемые переменные: отсутствуют"]),
                    0.85),
                new(
                    "Если сложность средняя, поддерживаемость средняя и ошибки средние, то качество среднее",
                    Min(
                        inputMemberships["Сложность Чепина: средняя"],
                        inputMemberships["Поддерживаемость: средняя"],
                        inputMemberships["Ошибки Холстеда: средние"]),
                    0.55),
                new(
                    "Если поддерживаемость средняя, ошибок мало и относительная сложность невысокая, то качество выше среднего",
                    Min(
                        inputMemberships["Поддерживаемость: средняя"],
                        inputMemberships["Ошибки Холстеда: низкие"],
                        Max(
                            inputMemberships["Относительная сложность: низкая"],
                            inputMemberships["Относительная сложность: средняя"])),
                    0.65),
                new(
                    "Если сложность высокая и ошибок много, то качество низкое",
                    Min(
                        inputMemberships["Сложность Чепина: высокая"],
                        inputMemberships["Ошибки Холстеда: высокие"]),
                    0.20),
                new(
                    "Если сложность критическая, то качество критически низкое",
                    inputMemberships["Сложность Чепина: критическая"],
                    0.10),
                new(
                    "Если поддерживаемость низкая, то качество низкое",
                    inputMemberships["Поддерживаемость: низкая"],
                    0.25),
                new(
                    "Если комментариев недостаточно и поддерживаемость низкая, то качество низкое",
                    Min(
                        inputMemberships["Комментарии: недостаточные"],
                        inputMemberships["Поддерживаемость: низкая"]),
                    0.20),
                new(
                    "Если много неиспользуемых переменных и сложность высокая, то качество низкое",
                    Min(
                        inputMemberships["Неиспользуемые переменные: много"],
                        inputMemberships["Сложность Чепина: высокая"]),
                    0.25),
                new(
                    "Если относительная сложность высокая и ошибки не низкие, то качество низкое",
                    Min(
                        inputMemberships["Относительная сложность: высокая"],
                        Max(
                            inputMemberships["Ошибки Холстеда: средние"],
                            inputMemberships["Ошибки Холстеда: высокие"])),
                    0.30),
                new(
                    "Если часть переменных не используется, но комментарии приемлемые, то качество среднее",
                    Min(
                        inputMemberships["Неиспользуемые переменные: есть"],
                        inputMemberships["Комментарии: приемлемые"]),
                    0.50)
            };

            var score = CalculateSugenoScore(rules, result.GilbMetrics.CodeQuality);
            var qualityMemberships = new Dictionary<string, double>
            {
                ["Низкое качество"] = LeftShoulder(score, 0.35, 0.55),
                ["Среднее качество"] = Triangle(score, 0.40, 0.60, 0.80),
                ["Высокое качество"] = RightShoulder(score, 0.70, 0.90)
            };

            return new FuzzyQualityResult
            {
                Score = Math.Round(score, 3),
                Level = GetLevel(score),
                InputMemberships = RoundTerms(inputMemberships),
                QualityMemberships = RoundTerms(qualityMemberships),
                TriggeredRules = rules
                    .Where(rule => rule.Activation >= RuleVisibilityThreshold)
                    .OrderByDescending(rule => rule.Activation)
                    .Select(rule => $"{rule.Description} (μ = {rule.Activation:F2})")
                    .ToList()
            };
        }

        private static double CalculateSugenoScore(IEnumerable<FuzzyRule> rules, double fallbackScore)
        {
            var activeRules = rules.Where(rule => rule.Activation > 0).ToList();
            if (!activeRules.Any())
            {
                return Math.Clamp(fallbackScore, 0, 1);
            }

            var activationSum = activeRules.Sum(rule => rule.Activation);
            var score = activeRules.Sum(rule => rule.Activation * rule.OutputScore) / activationSum;
            return Math.Clamp(score, 0, 1);
        }

        private static string GetLevel(double score)
        {
            if (score < 0.40)
            {
                return "Низкое качество";
            }

            if (score < 0.70)
            {
                return "Среднее качество";
            }

            return "Высокое качество";
        }

        private static Dictionary<string, double> RoundTerms(Dictionary<string, double> terms)
        {
            return terms.ToDictionary(
                term => term.Key,
                term => Math.Round(term.Value, 2));
        }

        private static double Triangle(double x, double left, double peak, double right)
        {
            if (x <= left || x >= right)
            {
                return 0;
            }

            if (Math.Abs(x - peak) < double.Epsilon)
            {
                return 1;
            }

            return x < peak
                ? (x - left) / (peak - left)
                : (right - x) / (right - peak);
        }

        private static double LeftShoulder(double x, double fullUntil, double zeroAt)
        {
            if (x <= fullUntil)
            {
                return 1;
            }

            if (x >= zeroAt)
            {
                return 0;
            }

            return (zeroAt - x) / (zeroAt - fullUntil);
        }

        private static double RightShoulder(double x, double zeroAt, double fullFrom)
        {
            if (x <= zeroAt)
            {
                return 0;
            }

            if (x >= fullFrom)
            {
                return 1;
            }

            return (x - zeroAt) / (fullFrom - zeroAt);
        }

        private static double Min(params double[] values)
        {
            return values.Min();
        }

        private static double Max(params double[] values)
        {
            return values.Max();
        }

        private readonly record struct FuzzyRule(string Description, double Activation, double OutputScore);
    }
}
