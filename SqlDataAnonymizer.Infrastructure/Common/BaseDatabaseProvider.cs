using System.Data.Common;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Enums;
using SqlDataAnonymizer.Domain.Interfaces;
using SqlDataAnonymizer.Infrastructure.Database;

namespace SqlDataAnonymizer.Infrastructure.Common;

public abstract class BaseDatabaseProvider : IDatabaseProvider
{
    public abstract DatabaseType Type { get; }
    
    /// <summary>
    /// Método protegido que subclasses implementam para criar DbConnection específico
    /// </summary>
    protected abstract DbConnection CreateDbConnection(string connectionString);
    
    /// <summary>
    /// Método público da interface que retorna wrapper testável
    /// </summary>
    public IDbConnectionWrapper CreateConnection(string connectionString)
    {
        var dbConnection = CreateDbConnection(connectionString);
        return new DbConnectionWrapper(dbConnection);
    }
    
    public abstract string GetSensitiveColumnsQuery();
    
    public abstract string GetPrimaryKeysQuery();
    
    public abstract string GetTableRowCountQuery(SensitiveColumnDto column);
    
    // ✅ NOVOS MÉTODOS ABSTRATOS
    public abstract string BuildSelectQuery(
        SensitiveColumnDto column,
        List<string> primaryKeys,
        int offset,
        int batchSize);
    
    public abstract string BuildSelectWithTempColumnQuery(
        SensitiveColumnDto column,
        string tempColumnName,
        long offset,
        int batchSize);
    
    public abstract string BuildBulkUpdateQuery(
        SensitiveColumnDto column,
        List<string> primaryKeys,
        List<IDictionary<string, object>> records,
        Dictionary<string, string> anonymizedValues);
    
    public abstract string GetAddTempColumnQuery(SensitiveColumnDto column, string tempColumnName);
    
    public abstract string GetDropTempColumnQuery(SensitiveColumnDto column, string tempColumnName);
    
    public abstract string BuildBulkUpdateWithTempColumnQuery(
        SensitiveColumnDto column,
        string tempColumnName,
        List<IDictionary<string, object>> records,
        Dictionary<string, string> anonymizedValues);
    
    public virtual string EscapeString(string value)
    {
        return value.Replace("'", "''");
    }
    
    public abstract string QuoteIdentifier(string identifier);
}