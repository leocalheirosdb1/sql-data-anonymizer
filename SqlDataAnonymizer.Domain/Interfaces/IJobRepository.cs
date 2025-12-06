using SqlDataAnonymizer.Domain.DTO;

namespace SqlDataAnonymizer.Domain.Interfaces;

public interface IJobRepository
{
    void Add(AnonymizationJobDto job);
    AnonymizationJobDto? GetById(Guid jobId);
    List<AnonymizationJobDto> GetAll();
    void Update(AnonymizationJobDto job);
}