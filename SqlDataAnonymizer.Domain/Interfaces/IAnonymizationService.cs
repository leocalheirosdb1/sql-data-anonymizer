using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Enums;

namespace SqlDataAnonymizer.Domain.Interfaces;

public interface IAnonymizationService
{
    Task<Guid> StartAnonymizationAsync(string server, string database, DatabaseType dbType);
    
    AnonymizationJobDto? GetJobStatus(Guid jobId);
    
    Task ProcessJobAsync(AnonymizationJobDto job);
}