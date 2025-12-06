using SqlDataAnonymizer.Domain.DTO;

namespace SqlDataAnonymizer.Domain.Interfaces;

public interface IAnonymizationRepository
{
    Task AnonymizeColumnAsync(
        string connectionString,
        IDatabaseProvider provider,
        SensitiveColumnDto column,
        Func<string, string> anonymizer,
        Action<string> logCallback);
}