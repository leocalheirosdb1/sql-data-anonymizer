using SqlDataAnonymizer.Domain.DTO;

namespace SqlDataAnonymizer.Domain.Interfaces;

public interface IAnonymizationRepository
{
    Task AnonymizeColumnAsync(
        string connectionString,
        IDatabaseProvider provider,
        SensitiveColumnDto column,
        IAnonymizationStrategy strategy,
        Action<string> logCallback);
}