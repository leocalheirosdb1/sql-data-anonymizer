using Microsoft.Extensions.Logging;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Interfaces;
using SqlDataAnonymizer.Infrastructure.Configuration;
using SqlDataAnonymizer.Infrastructure.Repositories.Models;

namespace SqlDataAnonymizer.Infrastructure.Repositories.Strategies;

/// <summary>
/// Estratégia de anonimização para tabelas sem chave primária (usando coluna temporária)
/// </summary>
internal sealed class TempColumnAnonymizationStrategy : ITableAnonymizationStrategy
{
    private readonly DatabaseSettings _settings;
    private readonly ILogger _logger;

    public TempColumnAnonymizationStrategy(DatabaseSettings settings, ILogger logger)
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
        var tempColumn = GenerateTempColumnName();
        
        _logger.LogWarning("Criando coluna temporária {TempColumn} para tabela {Table}", tempColumn, column.FullTableName());

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            await CreateTempColumnAsync(connection, provider, column, tempColumn, transaction);

            var context = new BatchProcessingContext(
                connection,
                provider,
                column,
                strategy,
                transaction,
                totalRows,
                _settings.BatchSize,
                logCallback);

            await ProcessAllBatchesWithTempColumnAsync(context, tempColumn);

            await DropTempColumnAsync(connection, provider, column, tempColumn, transaction);

            await transaction.CommitAsync();
            LogSuccess(column, logCallback);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(transaction, column, ex);
            throw;
        }
    }

    private static string GenerateTempColumnName() => $"__TempRowNum_{Guid.NewGuid():N}";

    private static async Task CreateTempColumnAsync(
        IDbConnectionWrapper connection,
        IDatabaseProvider provider,
        SensitiveColumnDto column,
        string tempColumn,
        IDbTransactionWrapper transaction)
    {
        var addColumnSql = provider.GetAddTempColumnQuery(column, tempColumn);
        await connection.ExecuteAsync(addColumnSql, transaction: transaction);
    }

    private static async Task DropTempColumnAsync(
        IDbConnectionWrapper connection,
        IDatabaseProvider provider,
        SensitiveColumnDto column,
        string tempColumn,
        IDbTransactionWrapper transaction)
    {
        var dropColumnSql = provider.GetDropTempColumnQuery(column, tempColumn);
        await connection.ExecuteAsync(dropColumnSql, transaction: transaction);
    }

    private async Task ProcessAllBatchesWithTempColumnAsync(BatchProcessingContext context, string tempColumn)
    {
        var offset = 1L;
        var processedRows = 0;

        while (offset <= context.TotalRows)
        {
            var batch = await FetchBatchWithTempColumnAsync(context, tempColumn, offset);

            if (batch.IsEmpty)
                break;

            await ProcessBatchWithTempColumnAsync(context, batch, tempColumn);

            processedRows += batch.Count;
            offset += context.BatchSize;

            ReportProgress(context.LogCallback, processedRows, context.TotalRows);
        }
    }

    private async Task<BatchData> FetchBatchWithTempColumnAsync(BatchProcessingContext context, string tempColumn, long offset)
    {
        var selectSql = context.Provider.BuildSelectWithTempColumnQuery(context.Column, tempColumn, offset, context.BatchSize);
        var records = await context.Connection.QueryAsync(selectSql, transaction: context.Transaction);

        var recordsList = records
            .Cast<IDictionary<string, object>>()
            .ToList();

        return new BatchData(recordsList);
    }

    private async Task ProcessBatchWithTempColumnAsync(BatchProcessingContext context, BatchData batch, string tempColumn)
    {
        var anonymizedValues = BuildAnonymizedValues(batch.Records, context.Column.ColumnName, context.Strategy);

        if (anonymizedValues.Count == 0)
            return;

        var updateSql = context.Provider.BuildBulkUpdateWithTempColumnQuery(
            context.Column, tempColumn, batch.Records, anonymizedValues);

        if (string.IsNullOrEmpty(updateSql))
            return;

        await context.Connection.ExecuteAsync(updateSql, transaction: context.Transaction, commandTimeout: 300);
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