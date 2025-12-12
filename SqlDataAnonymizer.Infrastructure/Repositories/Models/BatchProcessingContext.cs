using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Interfaces;

namespace SqlDataAnonymizer.Infrastructure.Repositories.Models;

/// <summary>
/// Value Object que encapsula o contexto de processamento de um batch
/// </summary>
internal sealed record BatchProcessingContext(
    IDbConnectionWrapper Connection,
    IDatabaseProvider Provider,
    SensitiveColumnDto Column,
    IAnonymizationStrategy Strategy,
    IDbTransactionWrapper Transaction,
    long TotalRows,
    int BatchSize,
    Action<string> LogCallback);