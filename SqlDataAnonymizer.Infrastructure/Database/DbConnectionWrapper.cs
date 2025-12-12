using System.Data;
using System.Data.Common;
using Dapper;
using SqlDataAnonymizer.Domain.Interfaces;

namespace SqlDataAnonymizer.Infrastructure.Database;

/// <summary>
/// Implementação concreta do wrapper usando DbConnection do ADO.NET
/// </summary>
public sealed class DbConnectionWrapper : IDbConnectionWrapper
{
    private readonly DbConnection _connection;
    private bool _disposed;

    public DbConnectionWrapper(DbConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public ConnectionState State => _connection.State;

    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync(cancellationToken);
        }
    }

    public Task CloseAsync()
    {
        if (_connection.State != ConnectionState.Closed)
        {
            _connection.Close();
        }
        return Task.CompletedTask;
    }
    
    public async Task<IDbTransactionWrapper> BeginTransactionAsync()
    {
        var dbTransaction = await _connection.BeginTransactionAsync();
        return new DbTransactionWrapper(dbTransaction);
    }

    public async Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null, IDbTransactionWrapper? transaction = null, int? commandTimeout = null)
    {
        var dbTransaction = GetDbTransaction(transaction);
        return await _connection.ExecuteScalarAsync<T>(sql, parameters, dbTransaction, commandTimeout: commandTimeout);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, IDbTransactionWrapper? transaction = null, int? commandTimeout = null)
    {
        var dbTransaction = GetDbTransaction(transaction);
        return await _connection.QueryAsync<T>(sql, parameters, dbTransaction, commandTimeout: commandTimeout);
    }

    public async Task<IEnumerable<dynamic>> QueryAsync(string sql, object? parameters = null, IDbTransactionWrapper? transaction = null, int? commandTimeout = null)
    {
        var dbTransaction = GetDbTransaction(transaction);
        return await _connection.QueryAsync(sql, parameters, dbTransaction, commandTimeout: commandTimeout);
    }

    public async Task<int> ExecuteAsync(string sql, object? parameters = null, IDbTransactionWrapper? transaction = null, int? commandTimeout = null)
    {
        var dbTransaction = GetDbTransaction(transaction);
        return await _connection.ExecuteAsync(sql, parameters, dbTransaction, commandTimeout: commandTimeout);
    }
    
    private DbTransaction? GetDbTransaction(IDbTransactionWrapper? wrapper)
    {
        return (wrapper as DbTransactionWrapper)?.GetInternalTransaction();
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await _connection.DisposeAsync();
            _disposed = true;
        }
    }
}