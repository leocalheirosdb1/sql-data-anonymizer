using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Enums;

namespace SqlDataAnonymizer.Domain.Interfaces;

public interface IAnonymizationService
{
    /// <summary>
    /// Inicia um processo de anonimização (enfileira o job)
    /// </summary>
    Task<Guid> StartAnonymizationAsync(string server, string database, DatabaseType dbType);

    /// <summary>
    /// Consulta o status de um job
    /// </summary>
    AnonymizationJobDto? GetJobStatus(Guid jobId);

    /// <summary>
    /// </summary>
    Task ProcessJobAsync(AnonymizationJobDto job);
}