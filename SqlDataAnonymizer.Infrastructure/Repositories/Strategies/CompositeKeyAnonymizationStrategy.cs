using Microsoft.Extensions.Logging;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Interfaces;
using SqlDataAnonymizer.Infrastructure.Configuration;
using SqlDataAnonymizer.Infrastructure.Repositories.Models;

namespace SqlDataAnonymizer.Infrastructure.Repositories.Strategies;

/// <summary>
/// Estratégia otimizada para tabelas com chaves primárias compostas (2+ colunas)
/// </summary>
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
            "Usando estratégia de chave composta para {Table}.{Column} com {KeyCount} colunas na PK: {Keys}",
            column.FullTableName(), column.ColumnName, primaryKeys.Count, string.Join(", ", primaryKeys));

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            LogStart(column, _settings.BatchSize, primaryKeys.Count);

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
        var anonymizedValues = BuildAnonymizedValuesForCompositeKey(
            batch.Records, 
            context.Column.ColumnName, 
            primaryKeys, 
            context.Strategy);

        if (anonymizedValues.Count == 0)
            return;

        var updateSql = BuildCompositeKeyUpdateQuery(
            context.Provider, 
            context.Column, 
            primaryKeys, 
            batch.Records, 
            anonymizedValues);

        if (string.IsNullOrEmpty(updateSql))
            return;

        await context.Connection.ExecuteAsync(updateSql, transaction: context.Transaction, commandTimeout: 300);
        _logger.LogDebug("Lote de {Count} registros com chave composta atualizado", batch.Count);
    }

    /// <summary>
    /// Gera valores anonimizados únicos baseados na chave composta
    /// </summary>
    private static Dictionary<string, string> BuildAnonymizedValuesForCompositeKey(
        List<IDictionary<string, object>> records,
        string columnName,
        List<string> primaryKeys,
        IAnonymizationStrategy strategy)
    {
        var result = new Dictionary<string, string>(capacity: records.Count);

        foreach (var record in records)
        {
            var originalValue = record[columnName]?.ToString();
            
            if (string.IsNullOrWhiteSpace(originalValue))
                continue;

            var compositeKey = BuildCompositeKey(record, primaryKeys);

            result[compositeKey] = strategy.Anonymize();
        }

        return result;
    }

    /// <summary>
    /// Constrói chave composta concatenando valores das PKs
    /// </summary>
    private static string BuildCompositeKey(IDictionary<string, object> record, List<string> primaryKeys)
    {
        var keyParts = new List<string>(primaryKeys.Count);
        
        foreach (var pk in primaryKeys)
        {
            var value = record[pk]?.ToString() ?? "NULL";
            keyParts.Add(value);
        }

        return string.Join("|", keyParts);
    }

    /// <summary>
    /// Constrói query UPDATE otimizada para chaves compostas
    /// </summary>
    private static string BuildCompositeKeyUpdateQuery(
        IDatabaseProvider provider,
        SensitiveColumnDto column,
        List<string> primaryKeys,
        List<IDictionary<string, object>> records,
        Dictionary<string, string> anonymizedValues)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"UPDATE {column.FullTableName()}");
        sb.AppendLine($"SET {provider.QuoteIdentifier(column.ColumnName)} = CASE");

        var hasValidRecords = false;
        var whereConditions = new List<string>();

        foreach (var record in records)
        {
            var originalValue = record[column.ColumnName]?.ToString();
            if (string.IsNullOrWhiteSpace(originalValue))
                continue;

            var compositeKey = BuildCompositeKey(record, primaryKeys);
            
            if (!anonymizedValues.TryGetValue(compositeKey, out var anonymizedValue))
                continue;

            hasValidRecords = true;

            var whenConditions = primaryKeys
                .Select(pk => $"{provider.QuoteIdentifier(pk)} = '{provider.EscapeString(record[pk]?.ToString() ?? "")}'")
                .ToList();

            var whenClause = string.Join(" AND ", whenConditions);
            sb.AppendLine($"    WHEN {whenClause} THEN '{provider.EscapeString(anonymizedValue)}'");

            var pkConditions = primaryKeys
                .Select(pk => $"{provider.QuoteIdentifier(pk)} = '{provider.EscapeString(record[pk]?.ToString() ?? "")}'")
                .ToList();
            
            whereConditions.Add($"({string.Join(" AND ", pkConditions)})");
        }

        if (!hasValidRecords)
            return string.Empty;

        sb.AppendLine($"    ELSE {provider.QuoteIdentifier(column.ColumnName)}");
        sb.AppendLine("END");
        sb.AppendLine($"WHERE {string.Join(" OR ", whereConditions)}");

        return sb.ToString();
    }

    private static void ReportProgress(Action<string> logCallback, int processedRows, long totalRows)
    {
        var percent = Math.Round((double)processedRows / totalRows * 100, 2);
        logCallback($"  Progresso: {processedRows:N0}/{totalRows:N0} ({percent}%)");
    }

    private void LogStart(SensitiveColumnDto column, int batchSize, int keyCount)
    {
        _logger.LogInformation(
            "Iniciando anonimização com chave composta ({KeyCount} colunas) de {Table}.{Column} com {BatchSize} registros por lote",
            keyCount, column.FullTableName(), column.ColumnName, batchSize);
    }

    private void LogSuccess(SensitiveColumnDto column, Action<string> logCallback)
    {
        logCallback($"  ✅ Concluído (chave composta): {column.FullTableName()}.{column.ColumnName}");
        _logger.LogInformation("Anonimização com chave composta concluída para {Table}.{Column}", 
            column.FullTableName(), column.ColumnName);
    }

    private async Task HandleErrorAsync(IDbTransactionWrapper transaction, SensitiveColumnDto column, Exception ex)
    {
        _logger.LogError(ex, "Erro durante anonimização com chave composta de {Table}.{Column}",
            column.FullTableName(), column.ColumnName);
        await transaction.RollbackAsync();
    }
}