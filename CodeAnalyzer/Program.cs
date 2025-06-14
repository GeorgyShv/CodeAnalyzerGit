using CodeAnalyzer.Core;
using CodeAnalyzer.Analyzers;
using CodeAnalyzer.Services;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Добавляем поддержку сессий
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Регистрируем анализаторы
builder.Services.AddScoped<ICodeAnalyzer, CSharpAnalyzer>();
builder.Services.AddScoped<ICodeAnalyzer, CppAnalyzer>();
builder.Services.AddScoped<ICodeAnalyzer, JavaAnalyzer>();
builder.Services.AddScoped<MetricsVisualizationService>();
builder.Services.AddScoped<IPdfReportService, PdfReportService>();
builder.Services.AddScoped<IChartImageService, ChartImageService>();

var app = builder.Build();

// Проверяем регистрацию анализаторов
using (var scope = app.Services.CreateScope())
{
    var analyzers = scope.ServiceProvider.GetServices<ICodeAnalyzer>().ToList();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    if (!analyzers.Any())
    {
        logger.LogError("Не удалось зарегистрировать анализаторы кода");
        throw new InvalidOperationException("Не удалось зарегистрировать анализаторы кода");
    }

    logger.LogInformation("Зарегистрированы анализаторы: {Analyzers}", 
        string.Join(", ", analyzers.Select(a => a.GetType().Name)));
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Добавляем middleware для сессий
app.UseSession();

app.MapRazorPages();
app.MapControllers();

app.Run();
