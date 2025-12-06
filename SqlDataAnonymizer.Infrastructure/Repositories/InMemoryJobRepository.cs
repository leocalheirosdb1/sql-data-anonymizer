using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Interfaces;

namespace SqlDataAnonymizer.Infrastructure.Repositories;

public sealed class InMemoryJobRepository : IJobRepository
{
    private readonly ConcurrentDictionary<Guid, AnonymizationJobDto> _jobs = new();
    private readonly ILogger<InMemoryJobRepository> _logger;

    public InMemoryJobRepository(ILogger<InMemoryJobRepository> logger)
    {
        _logger = logger;
    }

    public void Add(AnonymizationJobDto job)
    {
        _jobs.TryAdd(job.JobId, job);
        _logger.LogInformation("Job {JobId} adicionado", job.JobId);
    }

    public AnonymizationJobDto?  GetById(Guid jobId)
    {
        _jobs.TryGetValue(jobId, out var job);
        return job;
    }

    public List<AnonymizationJobDto> GetAll()
    {
        return _jobs.Values
            .OrderByDescending(j => j.StartedAt)
            .ToList();
    }

    public void Update(AnonymizationJobDto job)
    {
        _jobs[job.JobId] = job;
        _logger.LogDebug("Job {JobId} atualizado - Status: {Status}", job.JobId, job.Status);
    }
}