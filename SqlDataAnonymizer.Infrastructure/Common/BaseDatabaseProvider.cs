using System.Data;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Enums;
using SqlDataAnonymizer.Domain.Interfaces;

namespace SqlDataAnonymizer.Infrastructure.Common;

public abstract class BaseDatabaseProvider : IDatabaseProvider
{
    public abstract DatabaseType Type { get; }
    
    public abstract IDbConnection CreateConnection(string connectionString);
    
    public abstract string GetSensitiveColumnsQuery();
    
    public abstract string GetPrimaryKeysQuery();
    
    public abstract string GetTableRowCountQuery(SensitiveColumnDto column);
    
    public abstract string BuildBulkUpdateQuery(
        SensitiveColumnDto column,
        List<string> primaryKeys,
        List<Dictionary<string, object>> records,
        Dictionary<string, string> anonymizedValues);
    
    public abstract string GetAddTempColumnQuery(SensitiveColumnDto column, string tempColumnName);
    
    public abstract string GetDropTempColumnQuery(SensitiveColumnDto column, string tempColumnName);
    
    public abstract string BuildBulkUpdateWithTempColumnQuery(
        SensitiveColumnDto column,
        string tempColumnName,
        List<Dictionary<string, object>> records,
        Dictionary<string, string> anonymizedValues);
    
    public virtual string EscapeString(string value)
    {
        return value. Replace("'", "''");
    }
    
    public abstract string QuoteIdentifier(string identifier);
}