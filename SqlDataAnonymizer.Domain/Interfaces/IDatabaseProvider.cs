using System.Data;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Enums;

namespace SqlDataAnonymizer.Domain.Interfaces;

public interface IDatabaseProvider
{
    DatabaseType Type { get; }
    IDbConnection CreateConnection(string connectionString);
    string GetSensitiveColumnsQuery();
    string GetPrimaryKeysQuery();
    string GetTableRowCountQuery(SensitiveColumnDto column);
    string BuildBulkUpdateQuery(
        SensitiveColumnDto column,
        List<string> primaryKeys,
        List<Dictionary<string, object>> records,
        Dictionary<string, string> anonymizedValues);
    string GetAddTempColumnQuery(SensitiveColumnDto column, string tempColumnName);
    string GetDropTempColumnQuery(SensitiveColumnDto column, string tempColumnName);
    string BuildBulkUpdateWithTempColumnQuery(
        SensitiveColumnDto column,
        string tempColumnName,
        List<Dictionary<string, object>> records,
        Dictionary<string, string> anonymizedValues);
    string EscapeString(string value);
    string QuoteIdentifier(string identifier);
}