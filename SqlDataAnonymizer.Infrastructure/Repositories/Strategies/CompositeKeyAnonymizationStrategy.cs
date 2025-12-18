using Microsoft.Extensions.Logging;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Interfaces;
using SqlDataAnonymizer.Infrastructure.Configuration;
using SqlDataAnonymizer.Infrastructure.Repositories.Models;

namespace SqlDataAnonymizer.Infrastructure.Repositories.Strategies;

internal sealed class CompositeKeyAnonymizationStrategy : ITableAnonymizationStrategy
{
    private readonly DatabaseSettings _settings;
    private readonly ILogger _logger;

    public CompositeKeyAnonymizationStrategy(DatabaseSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task AnonymizeAsync(
        IDbConnectionWrapper connection,
        IDatabaseProvider provider,
        SensitiveColumnDto column,
        IAnonymizationStrategy strategy,
        long totalRows,
        Action<string> logCallback)
    {
        var primaryKeys = await GetPrimaryKeysAsync(connection, provider, column);
        
        _logger.LogInformation(
            "Iniciando anonimização com chave composta ({KeyCount} colunas) de {Table}.{Column} - PKs: {Keys} (temp table + batch insert)",
            primaryKeys.Count, column.FullTableName(), column.ColumnName, string.Join(", ", primaryKeys));

        await ProcessAllBatchesAsync(connection, provider, column, strategy, primaryKeys, totalRows, logCallback);

        logCallback($"Concluído (chave composta): {column.FullTableName()}.{column.ColumnName}");
        _logger.LogInformation("Anonimização com chave composta concluída para {Table}.{Column}", 
            column.FullTableName(), column.ColumnName);
    }

    private static async Task<List<string>> GetPrimaryKeysAsync(
        IDbConnectionWrapper connection,
        IDatabaseProvider provider,
        SensitiveColumnDto column)
    {
        var query = provider.GetPrimaryKeysQuery();
        var keys = await connection.QueryAsync<string>(query, new { column.Schema, column.TableName });
        return keys.ToList();
    }

    private async Task ProcessAllBatchesAsync(
        IDbConnectionWrapper connection,
        IDatabaseProvider provider,
        SensitiveColumnDto column,
        IAnonymizationStrategy strategy,
        List<string> primaryKeys,
        long totalRows,
        Action<string> logCallback)
    {
        var offset = 0;
        var processedRows = 0;
        var failedBatches = new List<int>();

        while (offset < totalRows)
        {
            await using var transaction = await connection.BeginTransactionAsync();
            
            try
            {
                var batch = await FetchBatchAsync(connection, provider, column, primaryKeys, offset, transaction);
                
                if (batch.IsEmpty)
                    break;
                
                var anonymizedValues = BuildAnonymizedValues(batch.Records, column.ColumnName, strategy);

                await provider.BulkUpdateWithTempTableAsync(
                    connection,
                    column,
                    primaryKeys,
                    batch.Records,
                    anonymizedValues,
                    transaction,
                    commandTimeout: 300);

                await transaction.CommitAsync();
                
                _logger.LogDebug("Batch {BatchNum} com chave composta ({Offset}-{End}) commitado", 
                    offset / _settings.BatchSize, offset, offset + batch.Count);

                processedRows += batch.Count;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                failedBatches.Add(offset / _settings.BatchSize);
                
                _logger.LogError(ex, "Erro no batch {BatchNum} com chave composta (offset {Offset})", 
                    offset / _settings.BatchSize, offset);
            }

            offset += _settings.BatchSize;
            ReportProgress(logCallback, processedRows, totalRows);
        }

        if (failedBatches.Count > 0)
        {
            _logger.LogWarning("Anonimização com chave composta concluída com {Count} batches falhados: {Batches}", 
                failedBatches.Count, string.Join(", ", failedBatches));
            
            logCallback($"  {failedBatches.Count} batches falharam: {string.Join(", ", failedBatches)}");
        }
    }

    private async Task<BatchData> FetchBatchAsync(
        IDbConnectionWrapper connection,
        IDatabaseProvider provider,
        SensitiveColumnDto column,
        List<string> primaryKeys,
        int offset,
        IDbTransactionWrapper transaction)
    {
        var selectSql = provider.BuildSelectQuery(column, primaryKeys, offset, _settings.BatchSize);
        var records = await connection.QueryAsync(selectSql, transaction: transaction);
        
        var recordsList = records
            .Cast<IDictionary<string, object>>()
            .ToList();

        return new BatchData(recordsList);
    }

    private static Dictionary<string, string> BuildAnonymizedValues(
        List<IDictionary<string, object>> records,
        string columnName,
        IAnonymizationStrategy strategy)
    {
        var result = new Dictionary<string, string>(capacity: records.Count);

        foreach (var record in records)
        {
            var originalValue = record[columnName]?.ToString();
    
            if (string.IsNullOrWhiteSpace(originalValue))
                continue;
            
            result.TryAdd(originalValue, strategy.Anonymize());
        }

        return result;
    }

    private static void ReportProgress(Action<string> logCallback, int processedRows, long totalRows)
    {
        var percent = Math.Round((double)processedRows / totalRows * 100, 2);
        logCallback($"Progresso: {processedRows:N0}/{totalRows:N0} ({percent}%)");
    }
}