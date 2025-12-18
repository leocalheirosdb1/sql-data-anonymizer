using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SqlDataAnonymizer.Domain.Interfaces;
using SqlDataAnonymizer.Domain.Models;
using SqlDataAnonymizer.Infrastructure.Configuration;

namespace SqlDataAnonymizer.Infrastructure.Repositories;

public sealed class MongoDbJobRepository : IJobRepository
{
    private readonly IMongoCollection<AnonymizationJobModel> _jobsCollection;
    private readonly ILogger<MongoDbJobRepository> _logger;

    public MongoDbJobRepository(
        MongoDbSettings settings,
        ILogger<MongoDbJobRepository> logger)
    {
        _logger = logger;

        try
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings. DatabaseName);
            _jobsCollection = database.GetCollection<AnonymizationJobModel>(settings. JobsCollectionName);
            
            CreateIndexesAsync().GetAwaiter().GetResult();
            
            _logger.LogInformation(
                "Conectado ao MongoDB:  {Database}. {Collection}", 
                settings.DatabaseName, 
                settings.JobsCollectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao conectar no MongoDB");
            throw;
        }
    }

    private async Task CreateIndexesAsync()
    {
        try
        {
            var startedAtIndex = Builders<AnonymizationJobModel>.IndexKeys
                .Descending(j => j.StartedAt);
            
            await _jobsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<AnonymizationJobModel>(startedAtIndex));
            
            var statusIndex = Builders<AnonymizationJobModel>.IndexKeys
                .Ascending(j => j.Status);
            
            await _jobsCollection.Indexes.CreateOneAsync(
                new CreateIndexModel<AnonymizationJobModel>(statusIndex));

            _logger.LogDebug("Índices criados com sucesso no MongoDB");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao criar índices no MongoDB (pode já existir)");
        }
    }

    public void Add(AnonymizationJobModel job)
    {
        try
        {
            _jobsCollection.InsertOne(job);
            _logger.LogInformation("Job {JobId} adicionado ao MongoDB", job.JobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar Job {JobId} no MongoDB", job.JobId);
            throw;
        }
    }

    public AnonymizationJobModel?  GetById(Guid jobId)
    {
        try
        {
            var filter = Builders<AnonymizationJobModel>.Filter.Eq(j => j.JobId, jobId);
            return _jobsCollection.Find(filter).FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar Job {JobId} no MongoDB", jobId);
            return null;
        }
    }

    public List<AnonymizationJobModel> GetAll()
    {
        try
        {
            return _jobsCollection
                .Find(Builders<AnonymizationJobModel>.Filter.Empty)
                .SortByDescending(j => j.StartedAt)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os Jobs no MongoDB");
            return [];
        }
    }

    public void Update(AnonymizationJobModel job)
    {
        try
        {
            var filter = Builders<AnonymizationJobModel>.Filter.Eq(j => j.JobId, job.JobId);
            var result = _jobsCollection.ReplaceOne(filter, job);
            
            if (result.ModifiedCount > 0)
            {
                _logger.LogDebug("Job {JobId} atualizado no MongoDB - Status: {Status}", 
                    job.JobId, job. Status);
                return;
            }

            _logger.LogWarning("Job {JobId} não foi encontrado para atualizar", job.JobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar Job {JobId} no MongoDB", job. JobId);
            throw;
        }
    }
}