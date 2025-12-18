using System.Data.Common;
using SqlDataAnonymizer.Domain.Interfaces;

namespace SqlDataAnonymizer.Infrastructure.Database;

public sealed class DbTransactionWrapper : IDbTransactionWrapper
{
    private readonly DbTransaction _transaction;
    private bool _disposed;

    public DbTransactionWrapper(DbTransaction transaction)
    {
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
    }

    public async Task CommitAsync()
    {
        await _transaction.CommitAsync();
    }

    public async Task RollbackAsync()
    {
        await _transaction.RollbackAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await _transaction.DisposeAsync();
            _disposed = true;
        }
    }
    
    internal DbTransaction GetInternalTransaction() => _transaction;
}