using Dapper;
using Microsoft.Extensions.Logging;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Interfaces;
using SqlDataAnonymizer.Infrastructure.Configuration;

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
        Func<string, string> anonymizer,
        Action<string> logCallback)
    {
        using var connection = provider.CreateConnection(connectionString);
        connection.Open();

        var totalRows = await GetTableRowCountAsync(connection, provider, column);

        if (totalRows == 0)
        {
            logCallback($"  Tabela {column. FullTableName()} está vazia");
            _logger.LogInformation("Tabela {Table} está vazia", column. FullTableName());
            return;
        }

        logCallback($"  Total de registros: {totalRows:N0}");
        _logger.LogInformation("Tabela {Table} possui {Count} registros", column. FullTableName(), totalRows);

        var primaryKeys = await GetPrimaryKeysAsync(connection, provider, column);

        if (!primaryKeys.Any())
        {
            _logger.LogWarning("Tabela {Table} não possui chave primária", column. FullTableName());
            logCallback($"  ⚠️ Tabela {column.FullTableName()} não possui chave primária.  Usando estratégia alternativa.");
            await AnonymizeWithoutPrimaryKeyAsync(connection, provider, column, anonymizer, totalRows, logCallback);
            return;
        }

        await AnonymizeWithPrimaryKeyAsync(connection, provider, column, primaryKeys, anonymizer, totalRows, logCallback);
        connection.Close();
    }

    private async Task<long> GetTableRowCountAsync(
        System.Data.IDbConnection connection,
        IDatabaseProvider provider,
        SensitiveColumnDto column)
    {
        var query = provider.GetTableRowCountQuery(column);
        return await connection.ExecuteScalarAsync<long>(query);
    }

    private async Task<List<string>> GetPrimaryKeysAsync(
        System.Data.IDbConnection connection,
        IDatabaseProvider provider,
        SensitiveColumnDto column)
    {
        var query = provider.GetPrimaryKeysQuery();
        var keys = await connection.QueryAsync<string>(query, new { Schema = column.Schema, TableName = column.TableName });
        return keys.ToList();
    }

    private async Task AnonymizeWithPrimaryKeyAsync(
        System.Data.IDbConnection connection,
        IDatabaseProvider provider,
        SensitiveColumnDto column,
        List<string> primaryKeys,
        Func<string, string> anonymizer,
        long totalRows,
        Action<string> logCallback)
    {
        var offset = 0;
        var batchSize = _settings.BatchSize;
        var processedRows = 0;

        _logger.LogInformation("Iniciando anonimização de {Table}. {Column} com {BatchSize} registros por lote", 
            column. FullTableName(), column. ColumnName, batchSize);

        while (offset < totalRows)
        {
            var selectSql = BuildSelectQuery(provider, column, primaryKeys, offset, batchSize);
            var records = await connection.QueryAsync(selectSql);
            var recordsList = records.Select(r => (IDictionary<string, object>)r).ToList();

            if (!recordsList.Any())
                break;

            var recordsDictList = recordsList.Select(r => r.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)).ToList();
            var anonymizedValues = BuildAnonymizedValuesDictionary(recordsDictList, column.ColumnName, anonymizer);

            if (anonymizedValues.Any())
            {
                var updateSql = provider.BuildBulkUpdateQuery(column, primaryKeys, recordsDictList, anonymizedValues);

                if (!string.IsNullOrEmpty(updateSql))
                {
                    await connection.ExecuteAsync(updateSql, commandTimeout: 300);
                    _logger.LogDebug("Lote de {Count} registros atualizado", recordsList.Count);
                }
            }

            processedRows += recordsList.Count;
            offset += batchSize;

            var percent = Math.Round((double)processedRows / totalRows * 100, 2);
            logCallback($"  Progresso: {processedRows:N0}/{totalRows:N0} ({percent}%)");
        }

        logCallback($"  ✅ Concluído: {column.FullTableName()}. {column.ColumnName}");
        _logger.LogInformation("Anonimização concluída para {Table}.{Column}", column.FullTableName(), column.ColumnName);
    }

    private async Task AnonymizeWithoutPrimaryKeyAsync(
        System.Data.IDbConnection connection,
        IDatabaseProvider provider,
        SensitiveColumnDto column,
        Func<string, string> anonymizer,
        long totalRows,
        Action<string> logCallback)
    {
        var tempColumn = $"__TempRowNum_{Guid.NewGuid():N}";

        _logger.LogWarning("Criando coluna temporária {TempColumn} para tabela {Table}", tempColumn, column.FullTableName());

        try
        {
            var addColumnSql = provider.GetAddTempColumnQuery(column, tempColumn);
            await connection.ExecuteAsync(addColumnSql);

            var offset = 1L;
            var batchSize = _settings.BatchSize;
            var processedRows = 0;

            while (offset <= totalRows)
            {
                var selectSql = BuildSelectWithTempColumnQuery(provider, column, tempColumn, offset, batchSize);
                var records = await connection.QueryAsync(selectSql);
                var recordsList = records.Select(r => (IDictionary<string, object>)r).ToList();

                if (recordsList.Any())
                {
                    var recordsDictList = recordsList.Select(r => r.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)).ToList();
                    var anonymizedValues = BuildAnonymizedValuesDictionary(recordsDictList, column.ColumnName, anonymizer);

                    if (anonymizedValues.Any())
                    {
                        var updateSql = provider.BuildBulkUpdateWithTempColumnQuery(column, tempColumn, recordsDictList, anonymizedValues);

                        if (!string.IsNullOrEmpty(updateSql))
                        {
                            await connection.ExecuteAsync(updateSql, commandTimeout: 300);
                        }
                    }

                    processedRows += recordsList.Count;
                }

                offset += batchSize;

                var percent = Math.Round((double)processedRows / totalRows * 100, 2);
                logCallback($"  Progresso: {processedRows:N0}/{totalRows:N0} ({percent}%)");
            }

            logCallback($"  ✅ Concluído: {column. FullTableName()}.{column.ColumnName}");
            _logger.LogInformation("Anonimização concluída para {Table}.{Column}", column. FullTableName(), column.ColumnName);
        }
        finally
        {
            try
            {
                var dropColumnSql = provider.GetDropTempColumnQuery(column, tempColumn);
                await connection.ExecuteAsync(dropColumnSql);
                _logger.LogInformation("Coluna temporária {TempColumn} removida", tempColumn);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao remover coluna temporária {TempColumn}", tempColumn);
            }
        }
    }

    private string BuildSelectQuery(
        IDatabaseProvider provider,
        SensitiveColumnDto column,
        List<string> primaryKeys,
        int offset,
        int batchSize)
    {
        var pkColumns = string.Join(", ", primaryKeys. Select(pk => provider.QuoteIdentifier(pk)));
        var columnQuoted = provider.QuoteIdentifier(column. ColumnName);
        var pkOrderBy = string.Join(", ", primaryKeys.Select(pk => provider.QuoteIdentifier(pk)));

        return provider.Type switch
        {
            Domain.Enums.DatabaseType. SqlServer => 
                $"SELECT {pkColumns}, {columnQuoted} FROM {column.FullTableName()} ORDER BY {pkOrderBy} OFFSET {offset} ROWS FETCH NEXT {batchSize} ROWS ONLY",
            
            Domain.Enums.DatabaseType.Oracle => 
                $"SELECT {pkColumns}, {columnQuoted} FROM (SELECT {pkColumns}, {columnQuoted}, ROW_NUMBER() OVER (ORDER BY {pkOrderBy}) AS rn FROM {column.FullTableName("\"")}) WHERE rn > {offset} AND rn <= {offset + batchSize}",
            
            Domain.Enums.DatabaseType.MySql => 
                $"SELECT {pkColumns}, {columnQuoted} FROM {column.FullTableName("`")} ORDER BY {pkOrderBy} LIMIT {batchSize} OFFSET {offset}",
            
            _ => throw new NotSupportedException($"Database type {provider.Type} is not supported")
        };
    }

    private string BuildSelectWithTempColumnQuery(
        IDatabaseProvider provider,
        SensitiveColumnDto column,
        string tempColumn,
        long offset,
        int batchSize)
    {
        var tempColQuoted = provider.QuoteIdentifier(tempColumn);
        var columnQuoted = provider.QuoteIdentifier(column.ColumnName);

        return $"SELECT {tempColQuoted}, {columnQuoted} FROM {column.FullTableName()} WHERE {tempColQuoted} BETWEEN {offset} AND {offset + batchSize - 1} AND {columnQuoted} IS NOT NULL";
    }

    private Dictionary<string, string> BuildAnonymizedValuesDictionary(
        List<Dictionary<string, object>> records,
        string columnName,
        Func<string, string> anonymizer)
    {
        var result = new Dictionary<string, string>();

        foreach (var record in records)
        {
            var originalValue = record[columnName]?.ToString();
            if (string.IsNullOrWhiteSpace(originalValue))
                continue;

            if (! result.ContainsKey(originalValue))
            {
                result[originalValue] = anonymizer(originalValue);
            }
        }

        return result;
    }
}