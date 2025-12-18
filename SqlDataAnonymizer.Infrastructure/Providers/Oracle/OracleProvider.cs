using System.Data.Common;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using SqlDataAnonymizer.Domain.DTO;
using SqlDataAnonymizer.Domain.Enums;
using SqlDataAnonymizer.Domain.Interfaces;
using SqlDataAnonymizer.Infrastructure.Common;

namespace SqlDataAnonymizer.Infrastructure.Providers.Oracle;

public sealed class OracleProvider : BaseDatabaseProvider
{
    public override DatabaseType Type => DatabaseType.Oracle;

    protected override DbConnection CreateDbConnection(string connectionString)
    {
        return new OracleConnection(connectionString);
    }

    public override string GetSensitiveColumnsQuery()
    {
        return """

                           SELECT 
                               atc.OWNER as Schema,
                               atc.TABLE_NAME as TableName,
                               atc.COLUMN_NAME as ColumnName,
                               atc.DATA_TYPE as DataType,
                               CASE
                                   WHEN UPPER(atc.COLUMN_NAME) LIKE '%EMAIL%' THEN 'email'
                                   WHEN UPPER(atc.COLUMN_NAME) LIKE '%CPF%' THEN 'cpf'
                                   WHEN UPPER(atc.COLUMN_NAME) LIKE '%TELEFONE%' THEN 'telefone'
                                   ELSE 'desconhecido'
                               END AS SensitiveType
                           FROM ALL_TAB_COLUMNS atc
                           INNER JOIN ALL_TABLES at ON atc.OWNER = at.OWNER AND atc.TABLE_NAME = at.TABLE_NAME
                           WHERE atc.OWNER NOT IN ('SYS', 'SYSTEM', 'OUTLN', 'DBSNMP', 'WMSYS', 'XDB', 'CTXSYS', 'MDSYS', 'OLAPSYS', 'ORDSYS')
                               AND (
                                   UPPER(atc.COLUMN_NAME) LIKE '%EMAIL%'
                                   OR UPPER(atc.COLUMN_NAME) LIKE '%CPF%'
                                   OR UPPER(atc.COLUMN_NAME) LIKE '%TELEFONE%'
                               )
                           ORDER BY atc.TABLE_NAME, atc.COLUMN_NAME
               """;
    }

    public override string GetPrimaryKeysQuery()
    {
        return """

                           SELECT acc.COLUMN_NAME
                           FROM ALL_CONSTRAINTS ac
                           INNER JOIN ALL_CONS_COLUMNS acc 
                               ON ac.OWNER = acc.OWNER 
                               AND ac.CONSTRAINT_NAME = acc.CONSTRAINT_NAME
                           WHERE ac.CONSTRAINT_TYPE = 'P'
                               AND ac.OWNER = :Schema
                               AND ac.TABLE_NAME = :TableName
                           ORDER BY acc.POSITION
               """;
    }

    public override string GetTableRowCountQuery(SensitiveColumnDto column)
    {
        return $"SELECT COUNT(*) FROM {column.FullTableName("\"")}";
    }

    public override string BuildSelectQuery(SensitiveColumnDto column, List<string> primaryKeys, int offset, int batchSize)
    {
        var pkColumns = string.Join(", ", primaryKeys.Select(pk => QuoteIdentifier(pk)));
        var columnQuoted = QuoteIdentifier(column.ColumnName);
        var pkOrderBy = string.Join(", ", primaryKeys.Select(pk => QuoteIdentifier(pk)));

        return $"SELECT {pkColumns}, {columnQuoted} " +
               $"FROM (SELECT {pkColumns}, {columnQuoted}, ROW_NUMBER() OVER (ORDER BY {pkOrderBy}) AS rn " +
               $"FROM {column.FullTableName("\"")}) " +
               $"WHERE rn > {offset} AND rn <= {offset + batchSize}";
    }

    public override string BuildSelectWithTempColumnQuery(SensitiveColumnDto column, string tempColumnName, long offset, int batchSize)
    {
        var tempColQuoted = QuoteIdentifier(tempColumnName);
        var columnQuoted = QuoteIdentifier(column.ColumnName);

        return $"SELECT {tempColQuoted}, {columnQuoted} " +
               $"FROM {column.FullTableName("\"")} " +
               $"WHERE {tempColQuoted} BETWEEN {offset} AND {offset + batchSize - 1} " +
               $"AND {columnQuoted} IS NOT NULL";
    }

    public override string GetAddTempColumnQuery(SensitiveColumnDto column, string tempColumnName)
    {
        var sequenceName = $"SEQ_{tempColumnName}";
        return $@"
            CREATE SEQUENCE {sequenceName} START WITH 1;
            ALTER TABLE {column.FullTableName("\"")} ADD ""{tempColumnName}"" NUMBER DEFAULT {sequenceName}.NEXTVAL;";
    }

    public override string GetDropTempColumnQuery(SensitiveColumnDto column, string tempColumnName)
    {
        var sequenceName = $"SEQ_{tempColumnName}";
        return $@"
            ALTER TABLE {column.FullTableName("\"")} DROP COLUMN ""{tempColumnName}"";
            DROP SEQUENCE {sequenceName};";
    }

    public override string QuoteIdentifier(string identifier)
    {
        return $"\"{identifier}\"";
    }

    protected override string GenerateTempTableName()
    {
        var guid = Guid.NewGuid().ToString("N").Substring(0, 18);
        return $"GTT_ANON_{guid}";
    }

    protected override string BuildCreateTempTableQuery(string tempTableName, List<string> primaryKeys)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"CREATE GLOBAL TEMPORARY TABLE {tempTableName} (");

        var pkDefinitions = primaryKeys.Select(pk => $"    \"{pk}\" VARCHAR2(255) NOT NULL");
        sb.AppendLine(string.Join(",\n", pkDefinitions) + ",");

        sb.AppendLine("    \"AnonymizedValue\" CLOB NOT NULL,");

        var pkList = string.Join(", ", primaryKeys.Select(pk => $"\"{pk}\""));
        sb.AppendLine($"    CONSTRAINT PK_{tempTableName.Replace("GTT_ANON_", "PK_")} PRIMARY KEY ({pkList})");

        sb.AppendLine(") ON COMMIT PRESERVE ROWS");

        return sb.ToString();
    }

    protected override string BuildBatchInsertQuery(string tempTableName, List<string> primaryKeys, List<AnonymizationRow> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("INSERT ALL");

        foreach (var row in rows)
        {
            var pkValues = primaryKeys.Select(pk => $"'{EscapeString(row.PrimaryKeyValues[pk])}'");
            var anonymized = $"'{EscapeString(row.AnonymizedValue)}'";

            sb.AppendLine($"    INTO {tempTableName} ({string.Join(", ", primaryKeys.Select(pk => $"\"{pk}\""))}, \"AnonymizedValue\")");
            sb.AppendLine($"    VALUES ({string.Join(", ", pkValues)}, {anonymized})");
        }

        sb.AppendLine("SELECT 1 FROM DUAL");

        return sb.ToString();
    }

    protected override string BuildUpdateFromTempTableQuery(string tempTableName, SensitiveColumnDto column, List<string> primaryKeys)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"MERGE INTO {column.FullTableName("\"")} t");
        sb.AppendLine($"USING {tempTableName} temp");

        var joinConditions = primaryKeys.Select(pk => $"    t.\"{pk}\" = temp.\"{pk}\"");
        sb.AppendLine($"ON ({string.Join("\n   AND ", joinConditions)})");

        sb.AppendLine("WHEN MATCHED THEN");
        sb.AppendLine($"    UPDATE SET t.\"{column.ColumnName}\" = temp.\"AnonymizedValue\"");

        return sb.ToString();
    }

    protected override async Task DropTempTableSafelyAsync(IDbConnectionWrapper connection, string tempTableName, IDbTransactionWrapper transaction)
    {
        var deleteSql = $"DELETE FROM {tempTableName}";
        await connection.ExecuteAsync(deleteSql, transaction: transaction, commandTimeout: 60);
    }
}