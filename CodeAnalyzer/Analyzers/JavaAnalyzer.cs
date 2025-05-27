using CodeAnalyzer.Core;
using CodeAnalyzer.Models;
using System.Text.RegularExpressions;

namespace CodeAnalyzer.Analyzers
{
    public class JavaAnalyzer : ICodeAnalyzer
    {
        private static readonly Regex SingleLineCommentRegex = new(@"^\s*//.*$");
        private static readonly Regex MultiLineCommentStartRegex = new(@"^\s*/\*.*$");
        private static readonly Regex MultiLineCommentEndRegex = new(@".*\*/\s*$");
        private static readonly Regex EmptyLineRegex = new(@"^\s*$");
        private static readonly Regex CodeLineRegex = new(@"^\s*[^{}\s].*$");

        public async Task<AnalysisResult> AnalyzeAsync(string sourceCode, string fileName)
        {
            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                throw new ArgumentException("Исходный код не может быть пустым", nameof(sourceCode));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("Имя файла не может быть пустым", nameof(fileName));
            }

            try
            {
                var result = new AnalysisResult
                {
                    FileName = fileName,
                    Language = "Java",
                    HalsteadMetrics = new HalsteadMetrics(),
                    GilbMetrics = new GilbMetrics(),
                    ChepinMetrics = new ChepinMetrics(),
                    Warnings = new List<string>()
                };

                // Получаем размер файла
                if (System.IO.File.Exists(fileName))
                {
                    var fileInfo = new System.IO.FileInfo(fileName);
                    result.FileSize = fileInfo.Length;
                }
                else
                {
                    result.FileSize = System.Text.Encoding.UTF8.GetByteCount(sourceCode);
                }

                // Нормализуем переносы строк
                sourceCode = sourceCode.Replace("\r\n", "\n").Replace("\r", "\n");
                
                // Разбиваем код на строки, сохраняя пустые строки
                var lines = sourceCode.Split('\n');
                result.TotalLines = lines.Length;

                if (result.TotalLines == 0)
                {
                    result.Warnings.Add("Файл не содержит кода");
                    return result;
                }

                // Подсчет строк кода, комментариев и пустых строк
                bool inMultiLineComment = false;
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    
                    if (string.IsNullOrWhiteSpace(trimmedLine))
                    {
                        result.EmptyLines++;
                        continue;
                    }

                    if (inMultiLineComment)
                    {
                        result.CommentLines++;
                        if (MultiLineCommentEndRegex.IsMatch(trimmedLine))
                        {
                            inMultiLineComment = false;
                        }
                    }
                    else if (SingleLineCommentRegex.IsMatch(trimmedLine))
                    {
                        result.CommentLines++;
                    }
                    else if (MultiLineCommentStartRegex.IsMatch(trimmedLine))
                    {
                        result.CommentLines++;
                        if (!MultiLineCommentEndRegex.IsMatch(trimmedLine))
                        {
                            inMultiLineComment = true;
                        }
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

                // Анализ операторов
                var operatorMatches = Regex.Matches(sourceCode, operatorPattern);
                foreach (Match match in operatorMatches)
                {
                    var op = match.Value;
                    operators.Add(op);
                    if (!operatorFrequency.ContainsKey(op))
                        operatorFrequency[op] = 0;
                    operatorFrequency[op]++;
                }

                // Анализ операндов
                var operandMatches = Regex.Matches(sourceCode, operandPattern);
                foreach (Match match in operandMatches)
                {
                    var op = match.Value;
                    operands.Add(op);
                    if (!operandFrequency.ContainsKey(op))
                        operandFrequency[op] = 0;
                    operandFrequency[op]++;
                }

                // Заполняем метрики Холстеда (первичные)
                result.HalsteadMetrics.UniqueOperators = operators.Count;
                result.HalsteadMetrics.UniqueOperands = operands.Count;
                result.HalsteadMetrics.TotalOperators = operatorFrequency.Values.Sum();
                result.HalsteadMetrics.TotalOperands = operandFrequency.Values.Sum();
                result.HalsteadMetrics.OperatorFrequency = operatorFrequency;
                result.HalsteadMetrics.OperandFrequency = operandFrequency;
                result.HalsteadMetrics.ProgramLength = (double)(result.HalsteadMetrics.TotalOperators + result.HalsteadMetrics.TotalOperands); // Расчет длины программы (N)
                result.HalsteadMetrics.Vocabulary = result.HalsteadMetrics.UniqueOperators + result.HalsteadMetrics.UniqueOperands; // Расчет словаря (n)

                // Определение числа входных/выходных параметров (n2*)
                result.NumberOfInputOutputParameters = AnalyzeInputOutputParameters(sourceCode);

                // Расчет метрик Холстеда (с использованием n2*)
                result.HalsteadMetrics.Volume = MetricsCalculator.CalculateHalsteadVolume(
                    result.HalsteadMetrics.UniqueOperators,
                    result.HalsteadMetrics.UniqueOperands
                );

                result.HalsteadMetrics.Difficulty = MetricsCalculator.CalculateHalsteadDifficulty(
                    result.HalsteadMetrics.UniqueOperators,
                    result.HalsteadMetrics.UniqueOperands,
                    result.HalsteadMetrics.TotalOperators,
                    result.HalsteadMetrics.TotalOperands
                );

                result.HalsteadMetrics.Effort = MetricsCalculator.CalculateHalsteadEffort(
                    result.HalsteadMetrics.Volume,
                    result.HalsteadMetrics.Difficulty
                );

                result.HalsteadMetrics.Time = MetricsCalculator.CalculateHalsteadTime(
                    result.HalsteadMetrics.Effort
                );

                result.HalsteadMetrics.Bugs = MetricsCalculator.CalculateHalsteadBugs(
                    result.HalsteadMetrics.Volume
                );

                // Расчет дополнительных метрик Холстеда
                result.HalsteadMetrics.PotentialVolume = MetricsCalculator.CalculateHalsteadPotentialVolume(result.NumberOfInputOutputParameters);
                result.HalsteadMetrics.ProgramLevel = MetricsCalculator.CalculateHalsteadLevel(result.HalsteadMetrics.Volume, result.HalsteadMetrics.PotentialVolume);
                result.HalsteadMetrics.LanguageLevel = MetricsCalculator.CalculateHalsteadLanguageLevel(result.HalsteadMetrics.ProgramLevel, result.HalsteadMetrics.PotentialVolume);
                result.HalsteadMetrics.ProgrammingEffort = MetricsCalculator.CalculateHalsteadProgrammingEffort(result.HalsteadMetrics.Volume, result.HalsteadMetrics.ProgramLevel);

                // Расчет метрик сложности из примера (CL, cl)
                var numberOfConditionalOperators = CountConditionalOperators(sourceCode);
                result.AbsoluteComplexityCL = MetricsCalculator.CalculateAbsoluteComplexityCL(numberOfConditionalOperators);
                result.RelativeComplexityCL = MetricsCalculator.CalculateRelativeComplexityCL(
                    result.AbsoluteComplexityCL,
                    result.HalsteadMetrics.ProgramLength // Общее число операторов и операндов (N1 + N2)
                );

                // Анализ переменных для метрики Чепина
                var variables = AnalyzeVariables(sourceCode);
                result.ChepinMetrics.InputVariables = variables.Count(v => v.Value == "P");
                result.ChepinMetrics.ModifiedVariables = variables.Count(v => v.Value == "M");
                result.ChepinMetrics.ControlVariables = variables.Count(v => v.Value == "C");
                result.ChepinMetrics.UnusedVariables = variables.Count(v => v.Value == "T");
                result.ChepinMetrics.VariableTypes = variables;

                // Расчет метрики Чепина
                result.ChepinMetrics.Complexity = MetricsCalculator.CalculateChepinComplexity(
                    result.ChepinMetrics.InputVariables,
                    result.ChepinMetrics.ModifiedVariables,
                    result.ChepinMetrics.ControlVariables,
                    result.ChepinMetrics.UnusedVariables
                );

                // Расчет метрик Джилба
                result.GilbMetrics.MaintainabilityIndex = MetricsCalculator.CalculateGilbMaintainabilityIndex(
                    CalculateCyclomaticComplexity(sourceCode),
                    result.CodeLines,
                    result.CommentLines
                );

                result.GilbMetrics.CodeQuality = MetricsCalculator.CalculateGilbCodeQuality(
                    result.GilbMetrics.MaintainabilityIndex,
                    CalculateCyclomaticComplexity(sourceCode) // Возможно, здесь нужно использовать абсолютную сложность CL, если это соответствует примеру
                );

                // Добавление предупреждений
                AddWarnings(result);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при анализе кода: {ex.Message}", ex);
            }
        }

        private Dictionary<string, string> AnalyzeVariables(string sourceCode)
        {
            var variables = new Dictionary<string, string>();
            
            // Добавляем системные типы и import директивы
            var systemTypes = new HashSet<string> { "System", "String", "Integer", "Double", "Boolean", "Date", "List", "Map", "ArrayList", "HashMap" };
            var importDirectives = new HashSet<string>();
            
            // Находим все import директивы
            var importMatches = Regex.Matches(sourceCode, @"import\s+([^;]+);");
            foreach (Match match in importMatches)
            {
                var importDirective = match.Groups[1].Value.Trim();
                importDirectives.Add(importDirective);
                variables[importDirective] = "P"; // Import директивы считаются входными переменными
            }

            // Находим все переменные и типы
            var variablePattern = @"\b([a-zA-Z_][a-zA-Z0-9_]*)\b";
            var matches = Regex.Matches(sourceCode, variablePattern);

            foreach (Match match in matches)
            {
                var varName = match.Value;
                if (!variables.ContainsKey(varName))
                {
                    // Проверяем, является ли переменная системным типом
                    if (systemTypes.Contains(varName))
                    {
                        variables[varName] = "P"; // Системные типы считаются входными переменными
                        continue;
                    }

                    // Определяем тип переменной
                    if (IsControlVariable(varName, sourceCode))
                        variables[varName] = "C";
                    else if (IsInputVariable(varName, sourceCode))
                        variables[varName] = "P";
                    else if (IsModifiedVariable(varName, sourceCode))
                        variables[varName] = "M";
                    else
                        variables[varName] = "T";
                }
            }

            return variables;
        }

        private bool IsControlVariable(string varName, string sourceCode)
        {
            // Проверяем использование в условных операторах
            var controlPatterns = new[]
            {
                $@"if\s*\(\s*{varName}\s*[=!<>]",
                $@"while\s*\(\s*{varName}\s*[=!<>]",
                $@"for\s*\(\s*.*{varName}\s*[=!<>]",
                $@"switch\s*\(\s*{varName}\s*\)"
            };

            return controlPatterns.Any(pattern => Regex.IsMatch(sourceCode, pattern));
        }

        private bool IsInputVariable(string varName, string sourceCode)
        {
            // Проверяем использование в методах ввода
            var inputPatterns = new[]
            {
                $@"Scanner\s*.*{varName}",
                $@"BufferedReader\s*.*{varName}",
                $@"System\.in\s*.*{varName}",
                $@"readLine\s*\(\s*\)\s*.*{varName}"
            };

            return inputPatterns.Any(pattern => Regex.IsMatch(sourceCode, pattern));
        }

        private bool IsModifiedVariable(string varName, string sourceCode)
        {
            // Проверяем присваивание значений
            var modificationPatterns = new[]
            {
                $@"{varName}\s*=",
                $@"{varName}\s*\+=",
                $@"{varName}\s*-=",
                $@"{varName}\s*\*=",
                $@"{varName}\s*/=",
                $@"{varName}\s*\+\+",
                $@"{varName}\s*--"
            };

            return modificationPatterns.Any(pattern => Regex.IsMatch(sourceCode, pattern));
        }

        private int CalculateCyclomaticComplexity(string sourceCode)
        {
            var complexity = 1; // Базовая сложность

            // Подсчет условных операторов
            complexity += Regex.Matches(sourceCode, @"\bif\b").Count;
            complexity += Regex.Matches(sourceCode, @"\belse\b").Count;
            complexity += Regex.Matches(sourceCode, @"\bfor\b").Count;
            complexity += Regex.Matches(sourceCode, @"\bwhile\b").Count;
            complexity += Regex.Matches(sourceCode, @"\bdo\b").Count;
            complexity += Regex.Matches(sourceCode, @"\bcase\b").Count;
            complexity += Regex.Matches(sourceCode, @"\bcatch\b").Count;
            complexity += Regex.Matches(sourceCode, @"\b&&\b").Count;
            complexity += Regex.Matches(sourceCode, @"\b\|\|\b").Count;

            return complexity;
        }

        private void AddWarnings(AnalysisResult result)
        {
            // Проверка на слишком большую сложность
            if (result.ChepinMetrics.Complexity > 10)
            {
                result.Warnings.Add("Высокая сложность кода по метрике Чепина");
            }

            // Проверка на слишком много ошибок по метрике Холстеда
            if (result.HalsteadMetrics.Bugs > 1)
            {
                result.Warnings.Add("Высокая оценка потенциальных ошибок по метрике Холстеда");
            }

            // Проверка на низкое качество кода
            if (result.GilbMetrics.CodeQuality < 0.5)
            {
                result.Warnings.Add("Низкое качество кода по метрике Джилба");
            }
        }

        private int AnalyzeInputOutputParameters(string sourceCode)
        {
            var ioParameters = new HashSet<string>();

            // Поиск переменных и литералов в Scanner и System.out вызовах
            var ioMatches = Regex.Matches(sourceCode, @"(Scanner.*?next.*?\(\)|System\.out\.print(?:ln)?\(.*?\))");
            foreach (Match ioMatch in ioMatches)
            {
                var content = ioMatch.Groups[1].Value;
                // Ищем переменные и строковые/символьные литералы
                 var parameterMatches = Regex.Matches(content, @"\b([a-zA-Z_][a-zA-Z0-9_]*)\b|""(.*?)""|'(.*?)'");
                foreach (Match paramMatch in parameterMatches)
                {
                    if (paramMatch.Groups[1].Success) ioParameters.Add(paramMatch.Groups[1].Value); // Переменная
                    if (paramMatch.Groups[2].Success) ioParameters.Add($"\"" + paramMatch.Groups[2].Value + "\""); // Строковый литерал
                    if (paramMatch.Groups[3].Success) ioParameters.Add("'" + paramMatch.Groups[3].Value + "'"); // Символьный литерал
                }
            }

            // Ищем переменные, которым присваивается результат чтения из Scanner
            var scannerReadMatches = Regex.Matches(sourceCode, @"([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*new\s+Scanner.*?next.*?\(\)");
            foreach (Match match in scannerReadMatches)
            {
                ioParameters.Add(match.Groups[1].Value);
            }

            return ioParameters.Count;
        }

        public string[] GetSupportedExtensions()
        {
            return new[] { ".java" };
        }

        public HalsteadMetrics GetHalsteadMetrics()
        {
            return new HalsteadMetrics
            {
                UniqueOperators = 0,
                UniqueOperands = 0,
                TotalOperators = 0,
                TotalOperands = 0,
                Volume = 0,
                Difficulty = 0,
                Effort = 0,
                Time = 0,
                Bugs = 0,
                OperatorFrequency = new Dictionary<string, int>(),
                OperandFrequency = new Dictionary<string, int>()
            };
        }

        public GilbMetrics GetGilbMetrics()
        {
            return new GilbMetrics
            {
                MaintainabilityIndex = 0,
                CodeQuality = 0
            };
        }

        public ChepinMetrics GetChepinMetrics()
        {
            return new ChepinMetrics
            {
                InputVariables = 0,
                ModifiedVariables = 0,
                ControlVariables = 0,
                UnusedVariables = 0,
                Complexity = 0,
                VariableTypes = new Dictionary<string, string>()
            };
        }

        public List<AnalysisResult> GetAnalysisResults()
        {
            return new List<AnalysisResult>();
        }

        // Метод для подсчета условных операторов
        private int CountConditionalOperators(string sourceCode)
        {
            // Подсчет условных операторов (те же, что для цикломатической сложности, но без базовой единицы)
            var count = Regex.Matches(sourceCode, @"\bif\b|\belse\b|\bswitch\b|\bcase\b|\bdefault\b|\bfor\b|\bwhile\b|\bdo\b|\bforeach").Count;
             count += Regex.Matches(sourceCode, @"\?").Count; // Тернарный оператор
             count += Regex.Matches(sourceCode, @"\bcatch\b").Count; // catch блок
             count += Regex.Matches(sourceCode, @"\b&&\b").Count; // Логическое И
             count += Regex.Matches(sourceCode, @"\b\|\|\b").Count; // Логическое ИЛИ
            return count;
        }
    }
} 