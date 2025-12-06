using SqlDataAnonymizer.Application.Services;
using SqlDataAnonymizer.Domain.Interfaces;
using SqlDataAnonymizer.Infrastructure.Configuration;
using SqlDataAnonymizer.Infrastructure.Factories;
using SqlDataAnonymizer.Infrastructure.Repositories;
using SqlDataAnonymizer.Infrastructure. Strategies;

var builder = WebApplication. CreateBuilder(args);

// ==========================================
// LOGGING NATIVO DO . NET
// ==========================================
builder. Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsProduction())
{
    builder. Logging.AddEventLog();
}

// ==========================================
// CONFIGURATION
// ==========================================
var dbSettings = builder.Configuration
    .GetSection("DatabaseSettings")
    .Get<DatabaseSettings>() ?? new DatabaseSettings();

builder.Services.AddSingleton(dbSettings);
builder.Services.AddSingleton<ConnectionStringFactory>();
builder.Services.AddSingleton<DatabaseProviderFactory>();

// ==========================================
// REPOSITORIES
// ==========================================
builder. Services.AddSingleton<IJobRepository, InMemoryJobRepository>();
builder. Services.AddScoped<ISensitiveColumnRepository, SensitiveColumnRepository>();
builder.Services.AddScoped<IAnonymizationRepository, AnonymizationRepository>();

// ==========================================
// STRATEGIES
// ==========================================
builder. Services.AddScoped<IAnonymizationStrategy, EmailAnonymizationStrategy>();
builder.Services.AddScoped<IAnonymizationStrategy, CpfAnonymizationStrategy>();
builder.Services.AddScoped<IAnonymizationStrategy, TelefoneAnonymizationStrategy>();

// ==========================================
// APPLICATION SERVICES
// ==========================================
builder.Services.AddScoped<AnonymizationService>();

// ==========================================
// API
// ==========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c. SwaggerDoc("v1", new()
    {
        Title = "SQL Data Anonymizer API",
        Version = "v1. 0",
        Description = "API multi-database para anonimização de dados sensíveis seguindo Clean Architecture",
        Contact = new()
        {
            Name = "Seu Nome",
            Email = "seu.email@exemplo. com"
        }
    });

    // Incluir comentários XML (opcional)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName(). Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c. IncludeXmlComments(xmlPath);
    }
});

// ==========================================
// CORS
// ==========================================
builder. Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy. AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ==========================================
// HTTP CLIENT (se necessário no futuro)
// ==========================================
builder.Services.AddHttpClient();

var app = builder.Build();

// ==========================================
// MIDDLEWARE PIPELINE
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SQL Data Anonymizer API v1");
        c.RoutePrefix = string. Empty; // Swagger na raiz
    });
}

app.UseHttpsRedirection();
app. UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// ==========================================
// STARTUP LOG
// ==========================================
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 SQL Data Anonymizer API Started");
logger.LogInformation("📊 Supported Databases: SqlServer, Oracle, MySql, PostgreSql");
logger.LogInformation("🌐 Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("⚙️  Batch Size: {BatchSize}", dbSettings.BatchSize);

await app.RunAsync();