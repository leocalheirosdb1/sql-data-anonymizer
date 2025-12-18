using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Interfaces;
using System.Threading.Channels;
using SqlDataAnonymizer.Domain.Models;

namespace SqlDataAnonymizer.Application.Processors;

public sealed class BackgroundJobProcessor : BackgroundService
{
    private readonly Channel<AnonymizationJobModel> _jobQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundJobProcessor> _logger;

    public BackgroundJobProcessor(
        Channel<AnonymizationJobModel> jobQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<BackgroundJobProcessor> logger)
    {
        _jobQueue = jobQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Job Processor iniciado");

        await foreach (var job in _jobQueue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("Processando job {JobId} em background", job.JobId);
                
                await using var scope = _scopeFactory.CreateAsyncScope();
                var anonymizationService = scope.ServiceProvider.GetRequiredService<IAnonymizationService>();

                await anonymizationService.ProcessJobAsync(job);

                _logger.LogInformation("Job {JobId} processado com sucesso", job.JobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro fatal ao processar job {JobId}", job.JobId);
            }
        }

        _logger.LogInformation("Background Job Processor encerrado");
    }
}