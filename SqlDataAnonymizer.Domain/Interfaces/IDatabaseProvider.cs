using System.Data;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Enums;

namespace SqlDataAnonymizer.Domain.Interfaces;

public interface IDatabaseProvider
{
    DatabaseType Type { get; }
    
    IDbConnectionWrapper CreateConnection(string connectionString);

    string GetSensitiveColumnsQuery();

    string GetPrimaryKeysQuery();

    string GetTableRowCountQuery(SensitiveColumnDto column);
    
    string BuildSelectQuery(
        SensitiveColumnDto column,
        List<string> primaryKeys,
        int offset,
        int batchSize);
    
    string BuildSelectWithTempColumnQuery(
        SensitiveColumnDto column,
        string tempColumnName,
        long offset,
        int batchSize);

    string GetAddTempColumnQuery(SensitiveColumnDto column, string tempColumnName);

    string GetDropTempColumnQuery(SensitiveColumnDto column, string tempColumnName);

    string EscapeString(string value);

    string QuoteIdentifier(string identifier);
    
    Task BulkUpdateWithTempTableAsync(
        IDbConnectionWrapper connection,
        SensitiveColumnDto column,
        List<string> primaryKeys,
        List<IDictionary<string, object>> records,
        Dictionary<string, string> anonymizedValues,
        IDbTransactionWrapper transaction,
        int commandTimeout = 300);
}