using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Interfaces;

namespace SqlDataAnonymizer.Infrastructure.Repositories.Strategies;

/// <summary>
/// Interface para estratégias de anonimização de tabelas
/// </summary>
internal interface ITableAnonymizationStrategy
{
    Task AnonymizeAsync(
        IDbConnectionWrapper connection,
        IDatabaseProvider provider,
        SensitiveColumnDto column,
        IAnonymizationStrategy strategy,
        long totalRows,
        Action<string> logCallback);
}