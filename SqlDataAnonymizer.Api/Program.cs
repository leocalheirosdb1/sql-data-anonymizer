using SqlDataAnonymizer.Application.Processors;
using SqlDataAnonymizer.Application.Services;
using SqlDataAnonymizer.Application.Strategies;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Interfaces;
using SqlDataAnonymizer.Infrastructure.Configuration;
using SqlDataAnonymizer.Infrastructure.Factories;
using SqlDataAnonymizer.Infrastructure.Repositories;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsProduction())
{
    builder.Logging.AddEventLog();
}

var dbSettings = builder.Configuration
    .GetSection("DatabaseSettings")
    .Get<DatabaseSettings>() ?? new DatabaseSettings();

builder.Services.AddSingleton(dbSettings);
builder.Services.AddSingleton<ConnectionStringFactory>();
builder.Services.AddSingleton<DatabaseProviderFactory>();

builder.Services.AddSingleton(Channel.CreateUnbounded<AnonymizationJobDto>(new UnboundedChannelOptions
{
    SingleReader = true, 
    SingleWriter = false
}));


builder.Services.AddSingleton<IJobRepository, InMemoryJobRepository>();
builder.Services.AddScoped<ISensitiveColumnRepository, SensitiveColumnRepository>();
builder.Services.AddScoped<IAnonymizationRepository, AnonymizationRepository>();

builder.Services.AddScoped<IAnonymizationStrategy, EmailAnonymizationStrategy>();
builder.Services.AddScoped<IAnonymizationStrategy, CpfAnonymizationStrategy>();
builder.Services.AddScoped<IAnonymizationStrategy, TelefoneAnonymizationStrategy>();

builder.Services.AddScoped<AnonymizationService>();
builder.Services.AddScoped<IAnonymizationService>(sp => sp.GetRequiredService<AnonymizationService>());

builder.Services.AddHostedService<BackgroundJobProcessor>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "SQL Data Anonymizer API",
        Version = "v1.0",
        Description = "API multi-database para anonimização de dados sensíveis"
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SQL Data Anonymizer API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("SQL Data Anonymizer API Started");
logger.LogInformation("Supported Databases: SqlServer, Oracle, MySql, PostgreSql");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Batch Size: {BatchSize}", dbSettings.BatchSize);

await app.RunAsync();