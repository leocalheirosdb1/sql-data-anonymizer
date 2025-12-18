using System.Data;

namespace SqlDataAnonymizer.Domain.Interfaces;

public interface IDbConnectionWrapper : IAsyncDisposable
{
    Task OpenAsync(CancellationToken cancellationToken = default);
    
    Task CloseAsync();
    
    ConnectionState State { get; }
    
    Task<IDbTransactionWrapper> BeginTransactionAsync();
    
    Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null, IDbTransactionWrapper? transaction = null, int? commandTimeout = null);
    
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, IDbTransactionWrapper? transaction = null, int? commandTimeout = null);
    
    Task<IEnumerable<dynamic>> QueryAsync(string sql, object? parameters = null, IDbTransactionWrapper? transaction = null, int? commandTimeout = null);
    
    Task<int> ExecuteAsync(string sql, object? parameters = null, IDbTransactionWrapper? transaction = null, int? commandTimeout = null);
}