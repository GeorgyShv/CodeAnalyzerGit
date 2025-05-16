using CodeAnalyzer.Models;

namespace CodeAnalyzer.Core
{
    public interface ICodeAnalyzer
    {
        /// <summary>
        /// Анализирует исходный код и возвращает результаты анализа
        /// </summary>
        /// <param name="sourceCode">Исходный код для анализа</param>
        /// <param name="fileName">Имя анализируемого файла</param>
        /// <returns>Результаты анализа, содержащие различные метрики</returns>
        Task<AnalysisResult> AnalyzeAsync(string sourceCode, string fileName);

        /// <summary>
        /// Получает поддерживаемые расширения файлов для этого анализатора
        /// </summary>
        /// <returns>Массив поддерживаемых расширений файлов (например, [".cs", ".cpp", ".c"])</returns>
        string[] GetSupportedExtensions();
    }
} 