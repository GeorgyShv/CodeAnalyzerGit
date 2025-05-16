using CodeAnalyzer.Core;
using CodeAnalyzer.Models;
using System.Text.RegularExpressions;

namespace CodeAnalyzer.Analyzers
{
    public class CSharpAnalyzer : ICodeAnalyzer
    {
        public async Task<AnalysisResult> AnalyzeAsync(string sourceCode, string fileName)
        {
            var result = new AnalysisResult
            {
                FileName = fileName,
                Language = "C#"
            };

            // Разбиваем код на строки
            var lines = sourceCode.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            result.TotalLines = lines.Length;

            // Подсчет строк кода, комментариев и пустых строк
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    result.EmptyLines++;
                }
                else if (trimmedLine.StartsWith("//") || trimmedLine.StartsWith("/*") || trimmedLine.StartsWith("*"))
                {
                    result.CommentLines++;
                }
                else
                {
                    result.CodeLines++;
                }
            }

            // Анализ операторов и операндов
            var operators = new HashSet<string>();
            var operands = new HashSet<string>();
            var operatorFrequency = new Dictionary<string, int>();
            var operandFrequency = new Dictionary<string, int>();

            // Регулярные выражения для поиска операторов и операндов
            var operatorPattern = @"[+\-*/%=<>!&|^~?:]+|\.|\(|\)|\[|\]|\{|\}|,|;";
            var operandPattern = @"\b[a-zA-Z_][a-zA-Z0-9_]*\b|\b\d+\b";

            foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l) && !l.TrimStart().StartsWith("//")))
            {
                // Поиск операторов
                var operatorMatches = Regex.Matches(line, operatorPattern);
                foreach (Match match in operatorMatches)
                {
                    var op = match.Value;
                    operators.Add(op);
                    if (!operatorFrequency.ContainsKey(op))
                        operatorFrequency[op] = 0;
                    operatorFrequency[op]++;
                }

                // Поиск операндов
                var operandMatches = Regex.Matches(line, operandPattern);
                foreach (Match match in operandMatches)
                {
                    var op = match.Value;
                    operands.Add(op);
                    if (!operandFrequency.ContainsKey(op))
                        operandFrequency[op] = 0;
                    operandFrequency[op]++;
                }
            }

            result.UniqueOperators = operators.Count;
            result.UniqueOperands = operands.Count;
            result.TotalOperators = operatorFrequency.Values.Sum();
            result.TotalOperands = operandFrequency.Values.Sum();
            result.OperatorFrequency = operatorFrequency;
            result.OperandFrequency = operandFrequency;

            // Расчет метрик Холстеда
            result.Volume = MetricsCalculator.CalculateHalsteadVolume(result.UniqueOperators, result.UniqueOperands);
            result.Difficulty = MetricsCalculator.CalculateHalsteadDifficulty(result.UniqueOperators, result.UniqueOperands, result.TotalOperators, result.TotalOperands);
            result.Effort = MetricsCalculator.CalculateHalsteadEffort(result.Volume, result.Difficulty);
            result.Time = MetricsCalculator.CalculateHalsteadTime(result.Effort);
            result.Bugs = MetricsCalculator.CalculateHalsteadBugs(result.Volume);

            // Расчет цикломатической сложности
            result.CyclomaticComplexity = CalculateCyclomaticComplexity(sourceCode);
            result.EssentialComplexity = CalculateEssentialComplexity(sourceCode);
            result.DesignComplexity = CalculateDesignComplexity(sourceCode);

            // Расчет метрик Джилба
            result.MaintainabilityIndex = MetricsCalculator.CalculateMaintainabilityIndex(
                result.CyclomaticComplexity,
                result.CodeLines,
                result.CommentLines
            );
            result.CodeQuality = MetricsCalculator.CalculateCodeQuality(
                result.MaintainabilityIndex,
                result.CyclomaticComplexity
            );

            // Добавление предупреждений
            AddWarnings(result);

            return result;
        }

        public string[] GetSupportedExtensions()
        {
            return new[] { ".cs" };
        }

        private int CalculateCyclomaticComplexity(string sourceCode)
        {
            var complexity = 1; // Базовая сложность

            // Подсчет условных операторов
            complexity += Regex.Matches(sourceCode, @"\bif\b|\belse\b|\bswitch\b|\bcase\b|\bdefault\b|\bfor\b|\bwhile\b|\bdo\b|\bforeach\b|\b\?\b").Count;

            // Подсчет операторов catch
            complexity += Regex.Matches(sourceCode, @"\bcatch\b").Count;

            return complexity;
        }

        private int CalculateEssentialComplexity(string sourceCode)
        {
            // Упрощенный расчет существенной сложности
            return CalculateCyclomaticComplexity(sourceCode) / 2;
        }

        private int CalculateDesignComplexity(string sourceCode)
        {
            // Упрощенный расчет проектной сложности
            return CalculateCyclomaticComplexity(sourceCode) / 3;
        }

        private void AddWarnings(AnalysisResult result)
        {
            if (result.CyclomaticComplexity > 10)
            {
                result.Warnings.Add("Высокая цикломатическая сложность кода");
            }

            if (result.CommentLines < result.CodeLines * 0.1)
            {
                result.Warnings.Add("Недостаточное количество комментариев");
            }

            if (result.MaintainabilityIndex < 50)
            {
                result.Warnings.Add("Низкий индекс поддерживаемости кода");
            }

            if (result.Bugs > 0.1)
            {
                result.Warnings.Add("Высокая оценка потенциальных ошибок");
            }
        }
    }
} 