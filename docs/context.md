# Контекст проекта Code Analyzer

## Общее описание
Code Analyzer - это веб-приложение для анализа исходного кода программного обеспечения. Приложение позволяет загружать файлы с исходным кодом и получать детальный анализ различных метрик качества кода.

## Поддерживаемые языки
- C# (.cs)
- C++ (.cpp)
- C (.c)

## Основные метрики анализа

### Метрики Холстеда
- Уникальные операторы
- Уникальные операнды
- Объем программы
- Сложность
- Усилия
- Время
- Оценка ошибок

### Метрики МакКейба
- Цикломатическая сложность
- Существенная сложность
- Проектная сложность

### Метрики Джилба
- Индекс поддерживаемости
- Качество кода

## Структура проекта

### Основные компоненты
- `Pages/` - Razor Pages приложения
  - `Index.cshtml` - Главная страница с формой загрузки файла
  - `Analysis.cshtml` - Страница с результатами анализа
- `Models/` - Модели данных
  - `AnalysisResult.cs` - Модель результатов анализа
- `Core/` - Основная логика анализа
  - `ICodeAnalyzer.cs` - Интерфейс анализатора кода
  - `MetricsCalculator.cs` - Калькулятор метрик

### Модели данных

#### AnalysisResult
Основная модель для хранения результатов анализа:
```csharp
public class AnalysisResult
{
    // Общая информация
    public string FileName { get; set; }
    public string Language { get; set; }
    public int TotalLines { get; set; }
    public int CodeLines { get; set; }
    public int CommentLines { get; set; }
    public int EmptyLines { get; set; }
    public long FileSize { get; set; }
    public string AnalysisDate { get; set; }

    // Метрики
    public HalsteadMetrics HalsteadMetrics { get; set; }
    public McCabeMetrics McCabeMetrics { get; set; }
    public GilbMetrics GilbMetrics { get; set; }

    // Предупреждения
    public List<string> Warnings { get; set; }
}
```

#### HalsteadMetrics
Метрики Холстеда:
```csharp
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
    public Dictionary<string, int> OperatorFrequency { get; set; }
    public Dictionary<string, int> OperandFrequency { get; set; }
}
```

#### McCabeMetrics
Метрики МакКейба:
```csharp
public class McCabeMetrics
{
    public int CyclomaticComplexity { get; set; }
    public int EssentialComplexity { get; set; }
    public int DesignComplexity { get; set; }
}
```

#### GilbMetrics
Метрики Джилба:
```csharp
public class GilbMetrics
{
    public double MaintainabilityIndex { get; set; }
    public double CodeQuality { get; set; }
}
```

## Процесс анализа
1. Пользователь загружает файл с исходным кодом через веб-интерфейс
2. Файл обрабатывается анализатором кода
3. Результаты анализа сохраняются в сессии
4. Пользователь перенаправляется на страницу с результатами
5. Результаты отображаются в структурированном виде с разбивкой по категориям метрик

## Интерфейс
- Главная страница содержит форму загрузки файла и информацию о поддерживаемых языках
- Страница результатов анализа отображает:
  - Общую информацию о файле
  - Метрики Холстеда
  - Метрики МакКейба
  - Метрики Джилба
  - Предупреждения (если есть)

## Технический стек
- ASP.NET Core 7.0
- Razor Pages
- Bootstrap 5
- C# 