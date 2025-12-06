using System.Data;
using System.Text;
using MySqlConnector;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Enums;
using SqlDataAnonymizer.Infrastructure.Common;

namespace SqlDataAnonymizer.Infrastructure.Database.Mysql;

public sealed class MySqlProvider : BaseDatabaseProvider
{
    public override DatabaseType Type => DatabaseType.MySql;

    public override IDbConnection CreateConnection(string connectionString)
    {
        return new MySqlConnection(connectionString);
    }

    public override string GetSensitiveColumnsQuery()
    {
        return @"
            SELECT 
                t.TABLE_SCHEMA as `Schema`,
                t.TABLE_NAME as TableName,
                c.COLUMN_NAME as ColumnName,
                c.DATA_TYPE as DataType,
                CASE
                    WHEN c. COLUMN_NAME LIKE '%EMAIL%' OR c.COLUMN_NAME LIKE '%CARTA%' THEN 'email'
                    WHEN c.COLUMN_NAME LIKE '%CPF%' OR c. COLUMN_NAME LIKE '%DOCUMENTO%' THEN 'cpf'
                    WHEN c.COLUMN_NAME LIKE '%TELEFONE%' OR c. COLUMN_NAME LIKE '%PHONE%' OR c.COLUMN_NAME LIKE '%FONE%' THEN 'telefone'
                    WHEN c. COLUMN_NAME LIKE '%NOME%' OR c.COLUMN_NAME LIKE '%NAME%' THEN 'nome'
                    WHEN c.COLUMN_NAME LIKE '%ENDERECO%' OR c.COLUMN_NAME LIKE '%ADDRESS%' THEN 'endereco'
                    ELSE 'desconhecido'
                END AS SensitiveType
            FROM INFORMATION_SCHEMA.TABLES t
            INNER JOIN INFORMATION_SCHEMA. COLUMNS c 
                ON t.TABLE_NAME = c.TABLE_NAME 
                AND t.TABLE_SCHEMA = c. TABLE_SCHEMA
            WHERE t.TABLE_TYPE = 'BASE TABLE'
                AND t.TABLE_SCHEMA NOT IN ('mysql', 'information_schema', 'performance_schema', 'sys')
                AND (
                    c.COLUMN_NAME LIKE '%EMAIL%' 
                    OR c. COLUMN_NAME LIKE '%CARTA%'
                    OR c.COLUMN_NAME LIKE '%CPF%'
                    OR c. COLUMN_NAME LIKE '%DOCUMENTO%'
                    OR c. COLUMN_NAME LIKE '%TELEFONE%'
                    OR c.COLUMN_NAME LIKE '%PHONE%'
                    OR c.COLUMN_NAME LIKE '%FONE%'
                    OR c.COLUMN_NAME LIKE '%NOME%'
                    OR c.COLUMN_NAME LIKE '%NAME%'
                    OR c.COLUMN_NAME LIKE '%ENDERECO%'
                    OR c. COLUMN_NAME LIKE '%ADDRESS%'
                )
            ORDER BY t.TABLE_NAME, c.COLUMN_NAME";
    }

    public override string GetPrimaryKeysQuery()
    {
        return @"
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
            WHERE CONSTRAINT_NAME = 'PRIMARY'
                AND TABLE_SCHEMA = @Schema
                AND TABLE_NAME = @TableName
            ORDER BY ORDINAL_POSITION";
    }

    public override string GetTableRowCountQuery(SensitiveColumnDto column)
    {
        return $"SELECT COUNT(*) FROM {column.FullTableName("`")}";
    }

    public override string BuildBulkUpdateQuery(
        SensitiveColumnDto column,
        List<string> primaryKeys,
        List<Dictionary<string, object>> records,
        Dictionary<string, string> anonymizedValues)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"UPDATE {column.FullTableName("`")}");
        sb.AppendLine($"SET `{column.ColumnName}` = CASE");

        var hasValidRecords = false;
        var whereConditions = new List<string>();

        foreach (var record in records)
        {
            var originalValue = record[column.ColumnName]?.ToString();
            if (string.IsNullOrWhiteSpace(originalValue))
                continue;

            if (!anonymizedValues.TryGetValue(originalValue, out var anonymizedValue))
                continue;

            hasValidRecords = true;

            var whenConditions = primaryKeys
                .Select(pk => $"`{pk}` = '{EscapeString(record[pk]?.ToString() ??  "")}'")
                .ToList();

            var whenClause = string.Join(" AND ", whenConditions);
            sb.AppendLine($"    WHEN {whenClause} THEN '{EscapeString(anonymizedValue)}'");

            var pkConditions = primaryKeys
                .Select(pk => $"`{pk}` = '{EscapeString(record[pk]?.ToString() ?? "")}'")
                .ToList();
            whereConditions.Add($"({string.Join(" AND ", pkConditions)})");
        }

        if (!hasValidRecords)
            return string. Empty;

        sb.AppendLine($"    ELSE `{column.ColumnName}`");
        sb.AppendLine("END");
        sb.AppendLine($"WHERE {string.Join(" OR ", whereConditions)}");

        return sb.ToString();
    }

    public override string GetAddTempColumnQuery(SensitiveColumnDto column, string tempColumnName)
    {
        return $"ALTER TABLE {column. FullTableName("`")} ADD `{tempColumnName}` BIGINT AUTO_INCREMENT, ADD INDEX (`{tempColumnName}`)";
    }

    public override string GetDropTempColumnQuery(SensitiveColumnDto column, string tempColumnName)
    {
        return $"ALTER TABLE {column.FullTableName("`")} DROP COLUMN `{tempColumnName}`";
    }

    public override string BuildBulkUpdateWithTempColumnQuery(
        SensitiveColumnDto column,
        string tempColumnName,
        List<Dictionary<string, object>> records,
        Dictionary<string, string> anonymizedValues)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"UPDATE {column.FullTableName("`")}");
        sb.AppendLine($"SET `{column.ColumnName}` = CASE");

        var hasValidRecords = false;
        var rowNumbers = new List<string>();

        foreach (var record in records)
        {
            var rowNum = record[tempColumnName]?.ToString();
            var originalValue = record[column.ColumnName]?.ToString();

            if (string.IsNullOrWhiteSpace(originalValue))
                continue;

            if (!anonymizedValues.TryGetValue(originalValue, out var anonymizedValue))
                continue;

            hasValidRecords = true;
            sb.AppendLine($"    WHEN `{tempColumnName}` = {rowNum} THEN '{EscapeString(anonymizedValue)}'");
            rowNumbers. Add(rowNum!);
        }

        if (!hasValidRecords)
            return string.Empty;

        sb.AppendLine($"    ELSE `{column.ColumnName}`");
        sb.AppendLine("END");
        sb.AppendLine($"WHERE `{tempColumnName}` IN ({string.Join(", ", rowNumbers)})");

        return sb.ToString();
    }

    public override string QuoteIdentifier(string identifier)
    {
        return $"`{identifier}`";
    }

    public override string EscapeString(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("'", "\\'")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            . Replace("\r", "\\r");
    }
}