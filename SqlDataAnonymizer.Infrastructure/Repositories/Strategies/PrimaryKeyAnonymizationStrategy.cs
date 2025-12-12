using Microsoft.Extensions.Logging;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Interfaces;
using SqlDataAnonymizer.Infrastructure.Configuration;
using SqlDataAnonymizer.Infrastructure.Repositories.Models;

namespace SqlDataAnonymizer.Infrastructure.Repositories.Strategies;

/// <summary>
/// Estratégia de anonimização para tabelas com chave primária
/// </summary>
internal sealed class PrimaryKeyAnonymizationStrategy : ITableAnonymizationStrategy
{
    private readonly DatabaseSettings _settings;
    private readonly ILogger _logger;

    public PrimaryKeyAnonymizationStrategy(DatabaseSettings settings, ILogger logger)
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
        
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            LogStart(column, _settings.BatchSize);

            var context = new BatchProcessingContext(
                connection,
                provider,
                column,
                strategy,
                transaction,
                totalRows,
                _settings.BatchSize,
                logCallback);

            await ProcessAllBatchesAsync(context, primaryKeys);

            await transaction.CommitAsync();
            LogSuccess(column, logCallback);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(transaction, column, ex);
            throw;
        }
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

    private async Task ProcessAllBatchesAsync(BatchProcessingContext context, List<string> primaryKeys)
    {
        var offset = 0;
        var processedRows = 0;

        while (offset < context.TotalRows)
        {
            var batch = await FetchBatchAsync(context, primaryKeys, offset);
            
            if (batch.IsEmpty)
                break;

            await ProcessBatchAsync(context, batch, primaryKeys);

            processedRows += batch.Count;
            offset += context.BatchSize;

            ReportProgress(context.LogCallback, processedRows, context.TotalRows);
        }
    }

    private async Task<BatchData> FetchBatchAsync(BatchProcessingContext context, List<string> primaryKeys, int offset)
    {
        var selectSql = context.Provider.BuildSelectQuery(context.Column, primaryKeys, offset, context.BatchSize);
        var records = await context.Connection.QueryAsync(selectSql, transaction: context.Transaction);
        
        var recordsList = records
            .Cast<IDictionary<string, object>>()
            .ToList();

        return new BatchData(recordsList);
    }

    private async Task ProcessBatchAsync(BatchProcessingContext context, BatchData batch, List<string> primaryKeys)
    {
        var anonymizedValues = BuildAnonymizedValues(batch.Records, context.Column.ColumnName, context.Strategy);

        if (anonymizedValues.Count == 0)
            return;

        var updateSql = context.Provider.BuildBulkUpdateQuery(context.Column, primaryKeys, batch.Records, anonymizedValues);

        if (string.IsNullOrEmpty(updateSql))
            return;

        await context.Connection.ExecuteAsync(updateSql, transaction: context.Transaction, commandTimeout: 300);
        _logger.LogDebug("Lote de {Count} registros atualizado", batch.Count);
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
        logCallback($"  Progresso: {processedRows:N0}/{totalRows:N0} ({percent}%)");
    }

    private void LogStart(SensitiveColumnDto column, int batchSize)
    {
        _logger.LogInformation(
            "Iniciando anonimização de {Table}.{Column} com {BatchSize} registros por lote (com transação)",
            column.FullTableName(), column.ColumnName, batchSize);
    }

    private void LogSuccess(SensitiveColumnDto column, Action<string> logCallback)
    {
        logCallback($"  ✅ Concluído: {column.FullTableName()}.{column.ColumnName}");
        _logger.LogInformation("Anonimização concluída para {Table}.{Column} (transação commitada)", 
            column.FullTableName(), column.ColumnName);
    }

    private async Task HandleErrorAsync(IDbTransactionWrapper transaction, SensitiveColumnDto column, Exception ex)
    {
        _logger.LogError(ex, "Erro durante anonimização de {Table}.{Column} - transação será revertida",
            column.FullTableName(), column.ColumnName);
        await transaction.RollbackAsync();
    }
}