using System.Data;

namespace SqlDataAnonymizer.Domain.Interfaces;

/// <summary>
/// Wrapper para conexão de banco que permite testabilidade
/// </summary>
public interface IDbConnectionWrapper : IAsyncDisposable
{
    /// <summary>
    /// Abre a conexão de forma assíncrona
    /// </summary>
    Task OpenAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Fecha a conexão
    /// </summary>
    Task CloseAsync();
    
    /// <summary>
    /// Estado atual da conexão
    /// </summary>
    ConnectionState State { get; }
    
    /// <summary>
    /// Inicia uma transação
    /// </summary>
    Task<IDbTransactionWrapper> BeginTransactionAsync();
    
    /// <summary>
    /// Executa uma query que retorna um único valor escalar
    /// </summary>
    Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null, IDbTransactionWrapper? transaction = null, int? commandTimeout = null);
    
    /// <summary>
    /// Executa uma query que retorna múltiplos registros
    /// </summary>
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, IDbTransactionWrapper? transaction = null, int? commandTimeout = null);
    
    /// <summary>
    /// Executa uma query que retorna objetos dinâmicos
    /// </summary>
    Task<IEnumerable<dynamic>> QueryAsync(string sql, object? parameters = null, IDbTransactionWrapper? transaction = null, int? commandTimeout = null);
    
    /// <summary>
    /// Executa um comando que não retorna dados (INSERT, UPDATE, DELETE)
    /// </summary>
    Task<int> ExecuteAsync(string sql, object? parameters = null, IDbTransactionWrapper? transaction = null, int? commandTimeout = null);
}