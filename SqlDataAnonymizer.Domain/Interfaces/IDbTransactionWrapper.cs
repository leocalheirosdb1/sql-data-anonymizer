namespace SqlDataAnonymizer.Domain.Interfaces;

/// <summary>
/// Wrapper para transa��o de banco que permite testabilidade
/// </summary>
public interface IDbTransactionWrapper : IAsyncDisposable
{
    /// <summary>
    /// Confirma a transacao
    /// </summary>
    Task CommitAsync();
    
    /// <summary>
    /// Reverte a transacao
    /// </summary>
    Task RollbackAsync();
}