using System.Data.Common;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Enums;
using SqlDataAnonymizer.Domain.Interfaces;
using SqlDataAnonymizer.Infrastructure.Database;

namespace SqlDataAnonymizer.Infrastructure.Common;

public abstract class BaseDatabaseProvider : IDatabaseProvider
{
    public abstract DatabaseType Type { get; }

    protected abstract DbConnection CreateDbConnection(string connectionString);

    public IDbConnectionWrapper CreateConnection(string connectionString)
    {
        var dbConnection = CreateDbConnection(connectionString);
        return new DbConnectionWrapper(dbConnection);
    }

    public abstract string GetSensitiveColumnsQuery();
    public abstract string GetPrimaryKeysQuery();
    public abstract string GetTableRowCountQuery(SensitiveColumnDto column);
    public abstract string BuildSelectQuery(SensitiveColumnDto column, List<string> primaryKeys, int offset, int batchSize);
    public abstract string BuildSelectWithTempColumnQuery(SensitiveColumnDto column, string tempColumnName, long offset, int batchSize);
    public abstract string GetAddTempColumnQuery(SensitiveColumnDto column, string tempColumnName);
    public abstract string GetDropTempColumnQuery(SensitiveColumnDto column, string tempColumnName);
    
    public virtual string EscapeString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        
        if (!value.Contains('\'') && !value.Contains('\\') && !value.Contains('\n') && !value.Contains('\r'))
            return value;
        
        return value.Replace("\\", "\\\\")
            .Replace("'", "''")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
    }

    public abstract string QuoteIdentifier(string identifier);

    public virtual async Task BulkUpdateWithTempTableAsync(
        IDbConnectionWrapper connection,
        SensitiveColumnDto column,
        List<string> primaryKeys,
        List<IDictionary<string, object>> records,
        Dictionary<string, string> anonymizedValues,
        IDbTransactionWrapper transaction,
        int commandTimeout = 300)
    {
        if (records.Count == 0 || anonymizedValues.Count == 0)
            return;

        var tempTableName = GenerateTempTableName();

        try
        {
            var createTableSql = BuildCreateTempTableQuery(tempTableName, primaryKeys);
            await connection.ExecuteAsync(createTableSql, transaction: transaction, commandTimeout: 60);
            
            var validRows = PrepareValidRows(column, primaryKeys, records, anonymizedValues);

            if (validRows.Count == 0)
                return;
            
            await InsertBatchesAsync(connection, tempTableName, primaryKeys, validRows, transaction);
            
            var updateSql = BuildUpdateFromTempTableQuery(tempTableName, column, primaryKeys);
            await connection.ExecuteAsync(updateSql, transaction: transaction, commandTimeout: commandTimeout);
        }
        finally
        {
            await DropTempTableSafelyAsync(connection, tempTableName, transaction);
        }
    }

    protected abstract string GenerateTempTableName();

    protected abstract string BuildCreateTempTableQuery(string tempTableName, List<string> primaryKeys);

    protected abstract string BuildBatchInsertQuery(string tempTableName, List<string> primaryKeys, List<AnonymizationRow> rows);

    protected abstract string BuildUpdateFromTempTableQuery(string tempTableName, SensitiveColumnDto column, List<string> primaryKeys);

    protected abstract Task DropTempTableSafelyAsync(IDbConnectionWrapper connection, string tempTableName, IDbTransactionWrapper transaction);

    protected List<AnonymizationRow> PrepareValidRows(
        SensitiveColumnDto column,
        List<string> primaryKeys,
        List<IDictionary<string, object>> records,
        Dictionary<string, string> anonymizedValues)
    {
        return records
            .Select(record => new
            {
                Record = record,
                OriginalValue = record[column.ColumnName]?.ToString()
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.OriginalValue))
            .Where(x => anonymizedValues.ContainsKey(x.OriginalValue!))
            .Select(x => new AnonymizationRow
            {
                PrimaryKeyValues = primaryKeys.ToDictionary(
                    pk => pk,
                    pk => Truncate(x.Record[pk]?.ToString() ?? "", 255)),
                AnonymizedValue = anonymizedValues[x.OriginalValue!]
            })
            .ToList();
    }

    protected async Task InsertBatchesAsync(
        IDbConnectionWrapper connection,
        string tempTableName,
        List<string> primaryKeys,
        List<AnonymizationRow> rows,
        IDbTransactionWrapper transaction)
    {
        const int batchSize = 500;
        var totalBatches = (int)Math.Ceiling((double)rows.Count / batchSize);

        for (var i = 0; i < totalBatches; i++)
        {
            var batch = rows.Skip(i * batchSize).Take(batchSize).ToList();
            var insertSql = BuildBatchInsertQuery(tempTableName, primaryKeys, batch);
            await connection.ExecuteAsync(insertSql, transaction: transaction, commandTimeout: 120);
        }
    }

    protected static string Truncate(string value, int maxLength)
        => value.Length > maxLength ? value.Substring(0, maxLength) : value;

    protected class AnonymizationRow
    {
        public Dictionary<string, string> PrimaryKeyValues { get; set; } = new();
        public string AnonymizedValue { get; set; } = string.Empty;
    }
}