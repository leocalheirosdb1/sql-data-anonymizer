using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Models;

namespace SqlDataAnonymizer.Domain.Interfaces;

public interface IJobRepository
{
    void Add(AnonymizationJobModel job);
    AnonymizationJobModel? GetById(Guid jobId);
    List<AnonymizationJobModel> GetAll();
    void Update(AnonymizationJobModel job);
}