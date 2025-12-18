using SqlDataAnonymizer.Domain.Enums;
using SqlDataAnonymizer.Infrastructure.Configuration;

namespace SqlDataAnonymizer.Infrastructure.Factories;

public sealed class ConnectionStringFactory
{
    private readonly DatabaseSettings _settings;

    public ConnectionStringFactory(DatabaseSettings settings)
    {
        _settings = settings;
    }

    public string Create(string server, string database, DatabaseType dbType)
    {
        return dbType switch
        {
            DatabaseType.SqlServer => CreateSqlServer(server, database),
            DatabaseType.Oracle => CreateOracle(server, database),
            DatabaseType.MySql => CreateMySql(server, database),
            _ => throw new NotSupportedException($"Database type {dbType} is not supported")
        };
    }

    private string CreateSqlServer(string server, string database)
    {
        return $"Server={server};" +
               $"Database={database};" +
               $"User Id={_settings.UserId};" +
               $"Password={_settings.Password};" +
               $"Connection Timeout={_settings.ConnectionTimeout};" +
               $"TrustServerCertificate={_settings.TrustServerCertificate};";
    }

    private string CreateOracle(string server, string database)
    {
        var port = _settings.Port > 0 ? _settings.Port : 1521;
        return $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={server})(PORT={port}))(CONNECT_DATA=(SERVICE_NAME={database})));" +
               $"User Id={_settings.UserId};" +
               $"Password={_settings.Password};";
    }

    private string CreateMySql(string server, string database)
    {
        var port = _settings.Port > 0 ? _settings.Port : 3306;
        return $"Server={server};" +
               $"Port={port};" +
               $"Database={database};" +
               $"Uid={_settings.UserId};" +
               $"Pwd={_settings.Password};" +
               $"Connection Timeout={_settings.ConnectionTimeout};";
    }

    private string CreatePostgreSql(string server, string database)
    {
        var port = _settings.Port > 0 ? _settings. Port : 5432;
        return $"Host={server};" +
               $"Port={port};" +
               $"Database={database};" +
               $"Username={_settings. UserId};" +
               $"Password={_settings.Password};" +
               $"Timeout={_settings.ConnectionTimeout};";
    }
}