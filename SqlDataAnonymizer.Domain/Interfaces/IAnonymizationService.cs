using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Enums;
using SqlDataAnonymizer.Domain.Models;

namespace SqlDataAnonymizer.Domain.Interfaces;

public interface IAnonymizationService
{
    Task<Guid> StartAnonymizationAsync(string server, string database, DatabaseType dbType);
    
    AnonymizationJobModel? GetJobStatus(Guid jobId);
    
    Task ProcessJobAsync(AnonymizationJobModel job);
}