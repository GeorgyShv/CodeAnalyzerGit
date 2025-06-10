using CodeAnalyzer.Core;
using CodeAnalyzer.Models;
using System.Text.RegularExpressions;

namespace CodeAnalyzer.Analyzers
{
    public class CppAnalyzer : ICodeAnalyzer
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
                    Language = "C++",
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

                // Регулярные выражения для поиска операторов и операндов в C++
                var operatorPattern = @"[+\-*/%=<>!&|^~?:]+|\.|\(|\)|\[|\]|\{|\}|,|;|::|->|\.\*|->\*|new|delete|sizeof|typeid|alignof|noexcept|decltype|co_await|co_yield|co_return";
                var operandPattern = @"\b[a-zA-Z_][a-zA-Z0-9_]*\b|\b\d+\b|\b0[xX][0-9a-fA-F]+\b|\b0[bB][01]+\b";

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
            
            // Добавляем системные типы и заголовочные файлы
            var systemTypes = new HashSet<string> { "std", "cout", "cin", "cerr", "endl", "string", "vector", "map", "set", "list", "deque", "array", "queue", "stack", "priority_queue" };
            var includeDirectives = new HashSet<string>();
            
            // Находим все include директивы
            var includeMatches = Regex.Matches(sourceCode, @"#include\s+[<""]([^>""]+)[>""]");
            foreach (Match match in includeMatches)
            {
                var includeDirective = match.Groups[1].Value.Trim();
                includeDirectives.Add(includeDirective);
                variables[includeDirective] = "P"; // Include директивы считаются входными переменными
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
            var controlPatterns = new[]
            {
                $@"\bfor\s*\(\s*{varName}\s*[=<>!]",
                $@"\bwhile\s*\(\s*{varName}\s*[=<>!]",
                $@"\bif\s*\(\s*{varName}\s*[=<>!]",
                $@"\bswitch\s*\(\s*{varName}\s*\)",
                $@"\bforeach\s*\(\s*[^)]*{varName}[^)]*\)",
                $@"\bcase\s+{varName}\s*:",
                $@"\b{varName}\s*\?\s*[^:]*\s*:",
                $@"\bdo\s*\{{[^}}]*\}}\s*while\s*\(\s*{varName}\s*[=<>!]"
            };

            return controlPatterns.Any(pattern => Regex.IsMatch(sourceCode, pattern));
        }

        private bool IsInputVariable(string varName, string sourceCode)
        {
            var inputPatterns = new[]
            {
                $@"\b{varName}\s*=\s*cin\s*>>",
                $@"\b{varName}\s*=\s*getline\s*\(",
                $@"\b{varName}\s*=\s*scanf\s*\(",
                $@"\b{varName}\s*=\s*fgets\s*\(",
                $@"\b{varName}\s*=\s*fread\s*\(",
                $@"\b{varName}\s*=\s*read\s*\(",
                $@"\b{varName}\s*=\s*atoi\s*\(",
                $@"\b{varName}\s*=\s*atof\s*\(",
                $@"\b{varName}\s*=\s*strtol\s*\(",
                $@"\b{varName}\s*=\s*strtod\s*\("
            };

            return inputPatterns.Any(pattern => Regex.IsMatch(sourceCode, pattern));
        }

        private bool IsModifiedVariable(string varName, string sourceCode)
        {
            var modificationPatterns = new[]
            {
                $@"\b{varName}\s*\+=",
                $@"\b{varName}\s*-=",
                $@"\b{varName}\s*\*=",
                $@"\b{varName}\s*/=",
                $@"\b{varName}\s*%=",
                $@"\b{varName}\s*&=",
                $@"\b{varName}\s*\|=",
                $@"\b{varName}\s*\^=",
                $@"\b{varName}\s*<<=",
                $@"\b{varName}\s*>>=",
                $@"\b{varName}\s*\+\+",
                $@"\b{varName}\s*--",
                $@"\b{varName}\s*=\s*[^;]+[+\-*/%&|^]",
                $@"\b{varName}\s*=\s*[^;]+\.",
                $@"\b{varName}\s*=\s*new\s+",
                $@"\b{varName}\s*=\s*\[",
                $@"\b{varName}\s*=\s*\{{"
            };

            return modificationPatterns.Any(pattern => Regex.IsMatch(sourceCode, pattern));
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

        private int AnalyzeInputOutputParameters(string sourceCode)
        {
            var ioParameters = new HashSet<string>();

            // Поиск переменных и литералов в cin/cout вызовах
            var ioMatches = Regex.Matches(sourceCode, @"(cin|cout)\s*([<>]|<<|>>)\s*(.*?)(?:;|$)");
            foreach (Match ioMatch in ioMatches)
            {
                var content = ioMatch.Groups[3].Value.Trim();
                // Ищем переменные и строковые/символьные литералы
                var parameterMatches = Regex.Matches(content, @"\b([a-zA-Z_][a-zA-Z0-9_]*)\b|""(.*?)""|'(.*?)'");
                foreach (Match paramMatch in parameterMatches)
                {
                    if (paramMatch.Groups[1].Success) ioParameters.Add(paramMatch.Groups[1].Value); // Переменная
                    if (paramMatch.Groups[2].Success) ioParameters.Add($"\"" + paramMatch.Groups[2].Value + "\""); // Строковый литерал
                    if (paramMatch.Groups[3].Success) ioParameters.Add("'" + paramMatch.Groups[3].Value + "'"); // Символьный литерал
                }
            }

            return ioParameters.Count;
        }

        private void AddWarnings(AnalysisResult result)
        {
            if (result.ChepinMetrics.Complexity > 50)
            {
                result.Warnings.Add("Высокая сложность по метрике Чепина");
            }

            if (result.CommentLines < result.CodeLines * 0.1)
            {
                result.Warnings.Add("Недостаточное количество комментариев");
            }

            if (result.GilbMetrics.MaintainabilityIndex < 50)
            {
                result.Warnings.Add("Низкий индекс поддерживаемости кода");
            }

            if (result.HalsteadMetrics.Bugs > 0.1)
            {
                result.Warnings.Add("Высокая оценка потенциальных ошибок");
            }

            if (result.ChepinMetrics.UnusedVariables > 0)
            {
                result.Warnings.Add("Обнаружены неиспользуемые переменные");
            }
        }

        public string[] GetSupportedExtensions()
        {
            return new[] { ".cpp", ".hpp", ".h", ".cc", ".cxx" };
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
            return count;
        }
    }
} 