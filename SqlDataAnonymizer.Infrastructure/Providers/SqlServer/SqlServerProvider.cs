using System.Data.Common;
using System.Text;
using Microsoft.Data.SqlClient;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Enums;
using SqlDataAnonymizer.Domain.Interfaces;
using SqlDataAnonymizer.Infrastructure.Common;

namespace SqlDataAnonymizer.Infrastructure.Providers.SqlServer;

public sealed class SqlServerProvider : BaseDatabaseProvider
{
    public override DatabaseType Type => DatabaseType.SqlServer;

    protected override DbConnection CreateDbConnection(string connectionString)
    {
        return new SqlConnection(connectionString);
    }

    public override string GetSensitiveColumnsQuery()
    {
        return """

                           SELECT 
                               t.TABLE_SCHEMA as [Schema],
                               t.TABLE_NAME as TableName,
                               c.COLUMN_NAME as ColumnName,
                               c.DATA_TYPE as DataType,
                               CASE
                                   WHEN c.COLUMN_NAME LIKE '%EMAIL%' THEN 'email'
                                   WHEN c.COLUMN_NAME LIKE '%CPF%' THEN 'cpf'
                                   WHEN c.COLUMN_NAME LIKE '%TELEFONE%' THEN 'telefone'
                                   ELSE 'desconhecido'
                               END AS SensitiveType
                           FROM INFORMATION_SCHEMA.TABLES t
                           INNER JOIN INFORMATION_SCHEMA.COLUMNS c 
                               ON t.TABLE_NAME = c.TABLE_NAME 
                               AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
                           WHERE t.TABLE_TYPE = 'BASE TABLE'
                               AND (
                                   c.COLUMN_NAME LIKE '%EMAIL%' 
                                   OR c.COLUMN_NAME LIKE '%CPF%'
                                   OR c.COLUMN_NAME LIKE '%TELEFONE%'
                               )
                           ORDER BY t.TABLE_NAME, c.COLUMN_NAME
               """;
    }

    public override string GetPrimaryKeysQuery()
    {
        return """

                           SELECT COLUMN_NAME
                           FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                           WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1
                               AND TABLE_SCHEMA = @Schema
                               AND TABLE_NAME = @TableName
                           ORDER BY ORDINAL_POSITION
               """;
    }

    public override string GetTableRowCountQuery(SensitiveColumnDto column)
    {
        return $"SELECT COUNT(*) FROM {column.FullTableName()}";
    }

    public override string BuildSelectQuery(SensitiveColumnDto column, List<string> primaryKeys, int offset, int batchSize)
    {
        var pkColumns = string.Join(", ", primaryKeys.Select(pk => QuoteIdentifier(pk)));
        var columnQuoted = QuoteIdentifier(column.ColumnName);
        var pkOrderBy = string.Join(", ", primaryKeys.Select(pk => QuoteIdentifier(pk)));

        return $"SELECT {pkColumns}, {columnQuoted} " +
               $"FROM {column.FullTableName()} WITH (NOLOCK) " +
               $"ORDER BY {pkOrderBy} " +
               $"OFFSET {offset} ROWS FETCH NEXT {batchSize} ROWS ONLY";
    }

    public override string BuildSelectWithTempColumnQuery(SensitiveColumnDto column, string tempColumnName, long offset, int batchSize)
    {
        var tempColQuoted = QuoteIdentifier(tempColumnName);
        var columnQuoted = QuoteIdentifier(column.ColumnName);

        return $"SELECT {tempColQuoted}, {columnQuoted} " +
               $"FROM {column.FullTableName()} " +
               $"WHERE {tempColQuoted} BETWEEN {offset} AND {offset + batchSize - 1} " +
               $"AND {columnQuoted} IS NOT NULL";
    }

    public override string GetAddTempColumnQuery(SensitiveColumnDto column, string tempColumnName)
    {
        return $"ALTER TABLE {column.FullTableName()} ADD [{tempColumnName}] BIGINT IDENTITY(1,1)";
    }

    public override string GetDropTempColumnQuery(SensitiveColumnDto column, string tempColumnName)
    {
        return $"ALTER TABLE {column.FullTableName()} DROP COLUMN [{tempColumnName}]";
    }

    public override string QuoteIdentifier(string identifier)
    {
        return $"[{identifier}]";
    }


    protected override string GenerateTempTableName()
    {
        return $"#AnonymizationBatch_{Guid.NewGuid():N}";
    }

    protected override string BuildCreateTempTableQuery(string tempTableName, List<string> primaryKeys)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"CREATE TABLE {tempTableName} (");

        var pkDefinitions = primaryKeys.Select(pk => $"    [{pk}] VARCHAR(255) NOT NULL");
        sb.AppendLine(string.Join(",\n", pkDefinitions) + ",");

        sb.AppendLine("    [AnonymizedValue] NVARCHAR(MAX) NOT NULL,");

        var pkList = string.Join(", ", primaryKeys.Select(pk => $"[{pk}]"));
        sb.AppendLine($"    PRIMARY KEY ({pkList})");

        sb.AppendLine(")");

        return sb.ToString();
    }

    protected override string BuildBatchInsertQuery(string tempTableName, List<string> primaryKeys, List<AnonymizationRow> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"INSERT INTO {tempTableName} ({string.Join(", ", primaryKeys.Select(pk => $"[{pk}]"))}, [AnonymizedValue])");
        sb.AppendLine("VALUES");

        var values = rows.Select(row =>
        {
            var pkValues = primaryKeys.Select(pk => $"'{EscapeString(row.PrimaryKeyValues[pk])}'");
            var anonymized = $"'{EscapeString(row.AnonymizedValue)}'";
            return $"    ({string.Join(", ", pkValues)}, {anonymized})";
        });

        sb.AppendLine(string.Join(",\n", values));

        return sb.ToString();
    }

    protected override string BuildUpdateFromTempTableQuery(string tempTableName, SensitiveColumnDto column, List<string> primaryKeys)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"UPDATE t WITH (ROWLOCK)");
        sb.AppendLine($"SET t.[{column.ColumnName}] = temp.[AnonymizedValue]");
        sb.AppendLine($"FROM {column.FullTableName()} t");
        sb.AppendLine($"INNER JOIN {tempTableName} temp");

        var joinConditions = primaryKeys.Select(pk => $"    t.[{pk}] = temp.[{pk}]");
        sb.AppendLine($"ON {string.Join("\n   AND ", joinConditions)}");

        sb.AppendLine("OPTION (OPTIMIZE FOR UNKNOWN)");

        return sb.ToString();
    }

    protected override async Task DropTempTableSafelyAsync(IDbConnectionWrapper connection, string tempTableName, IDbTransactionWrapper transaction)
    {
        var dropSql = $"IF OBJECT_ID('tempdb..{tempTableName}') IS NOT NULL DROP TABLE {tempTableName}";
        await connection.ExecuteAsync(dropSql, transaction: transaction, commandTimeout: 60);
    }
}