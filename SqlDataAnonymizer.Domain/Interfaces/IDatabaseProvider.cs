using System.Data;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Enums;

namespace SqlDataAnonymizer.Domain.Interfaces;

public interface IDatabaseProvider
{
    DatabaseType Type { get; }
    
    /// <summary>
    /// Cria uma conexão wrapper testável
    /// </summary>
    IDbConnectionWrapper CreateConnection(string connectionString);
    
    string GetSensitiveColumnsQuery();
    
    string GetPrimaryKeysQuery();
    
    string GetTableRowCountQuery(SensitiveColumnDto column);
    
    /// <summary>
    /// Constrói query SELECT para buscar registros em lote (com chave primária)
    /// </summary>
    string BuildSelectQuery(
        SensitiveColumnDto column,
        List<string> primaryKeys,
        int offset,
        int batchSize);
    
    /// <summary>
    /// Constrói query SELECT para buscar registros em lote (com coluna temporária)
    /// </summary>
    string BuildSelectWithTempColumnQuery(
        SensitiveColumnDto column,
        string tempColumnName,
        long offset,
        int batchSize);
    
    /// <summary>
    /// Constrói query de UPDATE em lote para tabelas com chave primária
    /// </summary>
    string BuildBulkUpdateQuery(
        SensitiveColumnDto column,
        List<string> primaryKeys,
        List<IDictionary<string, object>> records,
        Dictionary<string, string> anonymizedValues);
    
    string GetAddTempColumnQuery(SensitiveColumnDto column, string tempColumnName);
    
    string GetDropTempColumnQuery(SensitiveColumnDto column, string tempColumnName);
    
    /// <summary>
    /// Constrói query de UPDATE em lote para tabelas sem chave primária (usando coluna temporária)
    /// </summary>
    string BuildBulkUpdateWithTempColumnQuery(
        SensitiveColumnDto column,
        string tempColumnName,
        List<IDictionary<string, object>> records,
        Dictionary<string, string> anonymizedValues);
    
    string EscapeString(string value);
    
    string QuoteIdentifier(string identifier);
}