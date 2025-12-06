using System.Data;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Enums;
using SqlDataAnonymizer.Infrastructure.Common;

namespace SqlDataAnonymizer.Infrastructure.Database.Oracle;

public sealed class OracleProvider : BaseDatabaseProvider
{
    public override DatabaseType Type => DatabaseType.Oracle;

    public override IDbConnection CreateConnection(string connectionString)
    {
        return new OracleConnection(connectionString);
    }

    public override string GetSensitiveColumnsQuery()
    {
        return @"
            SELECT 
                atc. OWNER as Schema,
                atc.TABLE_NAME as TableName,
                atc.COLUMN_NAME as ColumnName,
                atc.DATA_TYPE as DataType,
                CASE
                    WHEN UPPER(atc. COLUMN_NAME) LIKE '%EMAIL%' OR UPPER(atc.COLUMN_NAME) LIKE '%CARTA%' THEN 'email'
                    WHEN UPPER(atc.COLUMN_NAME) LIKE '%CPF%' OR UPPER(atc.COLUMN_NAME) LIKE '%DOCUMENTO%' THEN 'cpf'
                    WHEN UPPER(atc.COLUMN_NAME) LIKE '%TELEFONE%' OR UPPER(atc. COLUMN_NAME) LIKE '%PHONE%' OR UPPER(atc.COLUMN_NAME) LIKE '%FONE%' THEN 'telefone'
                    WHEN UPPER(atc.COLUMN_NAME) LIKE '%NOME%' OR UPPER(atc.COLUMN_NAME) LIKE '%NAME%' THEN 'nome'
                    WHEN UPPER(atc.COLUMN_NAME) LIKE '%ENDERECO%' OR UPPER(atc.COLUMN_NAME) LIKE '%ADDRESS%' THEN 'endereco'
                    ELSE 'desconhecido'
                END AS SensitiveType
            FROM ALL_TAB_COLUMNS atc
            INNER JOIN ALL_TABLES at ON atc. OWNER = at. OWNER AND atc.TABLE_NAME = at.TABLE_NAME
            WHERE atc.OWNER NOT IN ('SYS', 'SYSTEM', 'OUTLN', 'DBSNMP', 'WMSYS', 'XDB', 'CTXSYS', 'MDSYS', 'OLAPSYS', 'ORDSYS')
                AND (
                    UPPER(atc. COLUMN_NAME) LIKE '%EMAIL%'
                    OR UPPER(atc.COLUMN_NAME) LIKE '%CARTA%'
                    OR UPPER(atc. COLUMN_NAME) LIKE '%CPF%'
                    OR UPPER(atc.COLUMN_NAME) LIKE '%DOCUMENTO%'
                    OR UPPER(atc.COLUMN_NAME) LIKE '%TELEFONE%'
                    OR UPPER(atc.COLUMN_NAME) LIKE '%PHONE%'
                    OR UPPER(atc.COLUMN_NAME) LIKE '%FONE%'
                    OR UPPER(atc. COLUMN_NAME) LIKE '%NOME%'
                    OR UPPER(atc.COLUMN_NAME) LIKE '%NAME%'
                    OR UPPER(atc.COLUMN_NAME) LIKE '%ENDERECO%'
                    OR UPPER(atc.COLUMN_NAME) LIKE '%ADDRESS%'
                )
            ORDER BY atc.TABLE_NAME, atc.COLUMN_NAME";
    }

    public override string GetPrimaryKeysQuery()
    {
        return @"
            SELECT acc. COLUMN_NAME
            FROM ALL_CONSTRAINTS ac
            INNER JOIN ALL_CONS_COLUMNS acc 
                ON ac.OWNER = acc.OWNER 
                AND ac. CONSTRAINT_NAME = acc.CONSTRAINT_NAME
            WHERE ac. CONSTRAINT_TYPE = 'P'
                AND ac.OWNER = :Schema
                AND ac.TABLE_NAME = :TableName
            ORDER BY acc.POSITION";
    }

    public override string GetTableRowCountQuery(SensitiveColumnDto column)
    {
        return $"SELECT COUNT(*) FROM {column. FullTableName("\"")}";
    }

    public override string BuildBulkUpdateQuery(
        SensitiveColumnDto column,
        List<string> primaryKeys,
        List<Dictionary<string, object>> records,
        Dictionary<string, string> anonymizedValues)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"UPDATE {column.FullTableName("\"")}");
        sb. AppendLine($"SET \"{column.ColumnName}\" = CASE");

        var hasValidRecords = false;
        var whereConditions = new List<string>();

        foreach (var record in records)
        {
            var originalValue = record[column. ColumnName].ToString();
            if (string.IsNullOrWhiteSpace(originalValue))
                continue;

            if (! anonymizedValues.TryGetValue(originalValue, out var anonymizedValue))
                continue;

            hasValidRecords = true;

            var whenConditions = primaryKeys
                .Select(pk => $"\"{pk}\" = '{EscapeString(record[pk]. ToString() ?? "")}'")
                .ToList();

            var whenClause = string.Join(" AND ", whenConditions);
            sb.AppendLine($"    WHEN {whenClause} THEN '{EscapeString(anonymizedValue)}'");

            var pkConditions = primaryKeys
                .Select(pk => $"\"{pk}\" = '{EscapeString(record[pk].ToString() ??  "")}'")
                .ToList();
            whereConditions. Add($"({string.Join(" AND ", pkConditions)})");
        }

        if (!hasValidRecords)
            return string.Empty;

        sb.AppendLine($"    ELSE \"{column.ColumnName}\"");
        sb.AppendLine("END");
        sb.AppendLine($"WHERE {string. Join(" OR ", whereConditions)}");

        return sb.ToString();
    }

    public override string GetAddTempColumnQuery(SensitiveColumnDto column, string tempColumnName)
    {
        var sequenceName = $"SEQ_{tempColumnName}";
        return $@"
            CREATE SEQUENCE {sequenceName} START WITH 1;
            ALTER TABLE {column. FullTableName("\"")} ADD ""{tempColumnName}"" NUMBER DEFAULT {sequenceName}. NEXTVAL;";
    }

    public override string GetDropTempColumnQuery(SensitiveColumnDto column, string tempColumnName)
    {
        var sequenceName = $"SEQ_{tempColumnName}";
        return $@"
            ALTER TABLE {column.FullTableName("\"")} DROP COLUMN ""{tempColumnName}"";
            DROP SEQUENCE {sequenceName};";
    }

    public override string BuildBulkUpdateWithTempColumnQuery(
        SensitiveColumnDto column,
        string tempColumnName,
        List<Dictionary<string, object>> records,
        Dictionary<string, string> anonymizedValues)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"UPDATE {column.FullTableName("\"")}");
        sb.AppendLine($"SET \"{column.ColumnName}\" = CASE");

        var hasValidRecords = false;
        var rowNumbers = new List<string>();

        foreach (var record in records)
        {
            var rowNum = record[tempColumnName].ToString();
            var originalValue = record[column.ColumnName].ToString();

            if (string.IsNullOrWhiteSpace(originalValue))
                continue;

            if (!anonymizedValues.TryGetValue(originalValue, out var anonymizedValue))
                continue;

            hasValidRecords = true;
            sb.AppendLine($"    WHEN \"{tempColumnName}\" = {rowNum} THEN '{EscapeString(anonymizedValue)}'");
            rowNumbers.Add(rowNum!);
        }

        if (!hasValidRecords)
            return string.Empty;

        sb.AppendLine($"    ELSE \"{column. ColumnName}\"");
        sb.AppendLine("END");
        sb.AppendLine($"WHERE \"{tempColumnName}\" IN ({string.Join(", ", rowNumbers)})");

        return sb.ToString();
    }

    public override string QuoteIdentifier(string identifier)
    {
        return $"\"{identifier}\"";
    }
}