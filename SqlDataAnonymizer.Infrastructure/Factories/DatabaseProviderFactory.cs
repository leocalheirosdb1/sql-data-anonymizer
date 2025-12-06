using SqlDataAnonymizer.Domain.Enums;
using SqlDataAnonymizer.Domain.Interfaces;
using SqlDataAnonymizer.Infrastructure.Database;
using SqlDataAnonymizer.Infrastructure.Database.Mysql;
using SqlDataAnonymizer.Infrastructure.Database.Oracle;

namespace SqlDataAnonymizer.Infrastructure.Factories;

public sealed class DatabaseProviderFactory
{
    private readonly Dictionary<DatabaseType, IDatabaseProvider> _providers;

    public DatabaseProviderFactory()
    {
        _providers = new Dictionary<DatabaseType, IDatabaseProvider>
        {
            { DatabaseType.SqlServer, new SqlServerProvider() },
            { DatabaseType.Oracle, new OracleProvider() },
            { DatabaseType.MySql, new MySqlProvider() }
        };
    }

    public IDatabaseProvider GetProvider(DatabaseType dbType)
    {
        if (! _providers.TryGetValue(dbType, out var provider))
        {
            throw new NotSupportedException($"Database type {dbType} is not supported");
        }

        return provider;
    }
}