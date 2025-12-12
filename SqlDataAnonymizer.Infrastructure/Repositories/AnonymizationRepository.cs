using Microsoft.Extensions.Logging;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Interfaces;
using SqlDataAnonymizer.Infrastructure.Configuration;
using SqlDataAnonymizer.Infrastructure.Repositories.Strategies;

namespace SqlDataAnonymizer.Infrastructure.Repositories;

public sealed class AnonymizationRepository : IAnonymizationRepository
{
    private readonly DatabaseSettings _settings;
    private readonly ILogger<AnonymizationRepository> _logger;

    public AnonymizationRepository(
        DatabaseSettings settings,
        ILogger<AnonymizationRepository> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task AnonymizeColumnAsync(
        string connectionString,
        IDatabaseProvider provider,
        SensitiveColumnDto column,
        IAnonymizationStrategy strategy,
        Action<string> logCallback)
    {
        await using var connection = provider.CreateConnection(connectionString);
        await connection.OpenAsync();

        if (await IsTableEmptyAsync(connection, provider, column, logCallback))
            return;

        var totalRows = await GetTableRowCountAsync(connection, provider, column);
        logCallback($"Total de registros: {totalRows:N0}");

        var primaryKeys = await GetPrimaryKeysAsync(connection, provider, column);

        var anonymizationStrategy = CreateAnonymizationStrategy(primaryKeys);
        await anonymizationStrategy.AnonymizeAsync(connection, provider, column, strategy, totalRows, logCallback);
    }

    private static async Task<bool> IsTableEmptyAsync(
        IDbConnectionWrapper connection,
        IDatabaseProvider provider,
        SensitiveColumnDto column,
        Action<string> logCallback)
    {
        var totalRows = await GetTableRowCountAsync(connection, provider, column);
        
        if (totalRows == 0)
        {
            logCallback($"  Tabela {column.FullTableName()} está vazia");
            return true;
        }

        return false;
    }

    private static async Task<long> GetTableRowCountAsync(
        IDbConnectionWrapper connection,
        IDatabaseProvider provider,
        SensitiveColumnDto column)
    {
        var query = provider.GetTableRowCountQuery(column);
        return await connection.ExecuteScalarAsync<long>(query);
    }

    private static async Task<List<string>> GetPrimaryKeysAsync(
        IDbConnectionWrapper connection,
        IDatabaseProvider provider,
        SensitiveColumnDto column)
    {
        var query = provider.GetPrimaryKeysQuery();
        var keys = await connection.QueryAsync<string>(query, new { Schema = column.Schema, TableName = column.TableName });
        return keys.ToList();
    }

    private ITableAnonymizationStrategy CreateAnonymizationStrategy(List<string> primaryKeys)
    {
        if (!primaryKeys.Any())
        {
            _logger.LogWarning("Tabela sem chave primária - usando estratégia com coluna temporária");
            return new TempColumnAnonymizationStrategy(_settings, _logger);
        }

        if (primaryKeys.Count > 1)
        {
            _logger.LogInformation("Tabela com chave composta ({Count} colunas) - usando estratégia otimizada", primaryKeys.Count);
            return new CompositeKeyAnonymizationStrategy(_settings, _logger);
        }

        return new PrimaryKeyAnonymizationStrategy(_settings, _logger);
    }
}