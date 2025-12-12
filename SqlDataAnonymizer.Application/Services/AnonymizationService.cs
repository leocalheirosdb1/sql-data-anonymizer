using Microsoft.Extensions.Logging;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Enums;
using SqlDataAnonymizer.Domain.Interfaces;
using SqlDataAnonymizer.Infrastructure.Factories;
using System.Threading.Channels;

namespace SqlDataAnonymizer.Application.Services;

public sealed class AnonymizationService : IAnonymizationService
{
    private readonly ISensitiveColumnRepository _columnRepository;
    private readonly IAnonymizationRepository _anonymizationRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IEnumerable<IAnonymizationStrategy> _strategies;
    private readonly ConnectionStringFactory _connectionStringFactory;
    private readonly DatabaseProviderFactory _providerFactory;
    private readonly Channel<AnonymizationJobDto> _jobQueue;
    private readonly ILogger<AnonymizationService> _logger;

    public AnonymizationService(
        ISensitiveColumnRepository columnRepository,
        IAnonymizationRepository anonymizationRepository,
        IJobRepository jobRepository,
        IEnumerable<IAnonymizationStrategy> strategies,
        ConnectionStringFactory connectionStringFactory,
        DatabaseProviderFactory providerFactory,
        Channel<AnonymizationJobDto> jobQueue,
        ILogger<AnonymizationService> logger)
    {
        _columnRepository = columnRepository;
        _anonymizationRepository = anonymizationRepository;
        _jobRepository = jobRepository;
        _strategies = strategies;
        _connectionStringFactory = connectionStringFactory;
        _providerFactory = providerFactory;
        _jobQueue = jobQueue;
        _logger = logger;
    }
    
    public async Task<Guid> StartAnonymizationAsync(string server, string database, DatabaseType dbType)
    {
        var job = CreateJob(server, database, dbType);
        _jobRepository.Add(job);

        _logger.LogInformation("📋 Anonimização enfileirada - JobId: {JobId}, Server: {Server}, Database: {Database}, Type: {Type}", 
            job.JobId, server, database, dbType);
        
        await _jobQueue.Writer.WriteAsync(job);
        
        _logger.LogInformation("✅ Job {JobId} enfileirado com sucesso", job.JobId);

        return job.JobId;
    }

    public AnonymizationJobDto? GetJobStatus(Guid jobId)
    {
        return _jobRepository.GetById(jobId);
    }

    public async Task ProcessJobAsync(AnonymizationJobDto job)
    {
        try
        {
            UpdateJobStatus(job, "Processing", $"🚀 Iniciando anonimização no {job.DatabaseType}");

            var connectionString = _connectionStringFactory.Create(job.Server, job.Database, job.DatabaseType);
            var provider = _providerFactory.GetProvider(job.DatabaseType);

            AddLog(job, "🔍 Detectando colunas sensíveis...");
            var columns = await _columnRepository.GetSensitiveColumnsAsync(connectionString, provider);

            if (!columns.Any())
            {
                UpdateJobStatus(job, "Completed", "ℹ️ Nenhuma coluna sensível encontrada");
                return;
            }

            AddLog(job, $"✅ Encontradas {columns.Count} colunas sensíveis em {columns.Select(c => c.FullTableName()).Distinct().Count()} tabelas");

            await ProcessColumnsAsync(job, connectionString, provider, columns);

            UpdateJobStatus(job, "Completed", "✅ Anonimização concluída com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao processar job {JobId}", job.JobId);
            UpdateJobStatus(job, "Failed", $"❌ Erro: {ex.Message}");
            job.ErrorMessage = ex.Message;
            _jobRepository.Update(job);
        }
    }

    private AnonymizationJobDto CreateJob(string server, string database, DatabaseType dbType)
    {
        return new AnonymizationJobDto
        {
            JobId = Guid.NewGuid(),
            Server = server,
            Database = database,
            DatabaseType = dbType,
            Status = "Queued",
            StartedAt = DateTime.UtcNow
        };
    }

    private async Task ProcessColumnsAsync(
        AnonymizationJobDto job,
        string connectionString,
        IDatabaseProvider provider,
        List<SensitiveColumnDto> columns)
    {
        foreach (var column in columns)
        {
            var strategy = FindStrategy(column.SensitiveType);

            if (strategy == null)
            {
                var message = $"⚠️ Nenhuma estratégia encontrada para tipo '{column.SensitiveType}' na coluna {column.FullTableName()}.{column.ColumnName}";
                AddLog(job, message);
                _logger.LogWarning(message);
                continue;
            }

            AddLog(job, $"📊 Processando {column.FullTableName()}.{column.ColumnName} (Tipo: {column.SensitiveType})");

            await _anonymizationRepository.AnonymizeColumnAsync(
                connectionString,
                provider,
                column,
                strategy,
                log => AddLog(job, log));

            _jobRepository.Update(job);
        }
    }

    private IAnonymizationStrategy? FindStrategy(string type)
    {
        return _strategies.FirstOrDefault(s => s.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
    }

    private void UpdateJobStatus(AnonymizationJobDto job, string status, string message)
    {
        job.Status = status;

        if (status is "Completed" or "Failed")
        {
            job.CompletedAt = DateTime.UtcNow;
        }

        AddLog(job, message);
        _jobRepository.Update(job);
    }

    private void AddLog(AnonymizationJobDto job, string message)
    {
        var log = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}";
        
        job.Logs.Add(log);
        
        _logger.LogInformation("{Message}", message);
    }
}