namespace SqlDataAnonymizer.Domain.Interfaces;

public interface IDbTransactionWrapper : IAsyncDisposable
{
    Task CommitAsync();
    
    Task RollbackAsync();
}