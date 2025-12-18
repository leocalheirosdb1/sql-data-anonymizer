using SqlDataAnonymizer.Domain.DTO;

namespace SqlDataAnonymizer.Domain.Interfaces;

public interface ITableAnonymizationStrategy
{
    Task AnonymizeAsync(
        IDbConnectionWrapper connection,
        IDatabaseProvider provider,
        SensitiveColumnDto column,
        IAnonymizationStrategy strategy,
        long totalRows,
        Action<string> logCallback);
}