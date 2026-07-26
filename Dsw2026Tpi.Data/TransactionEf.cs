using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dsw2026Tpi.Data;

public class TransactionEf : ITransaction
{
    private readonly IDbContextTransaction _transaction;

    public TransactionEf(IDbContextTransaction transaction)
    {
        _transaction = transaction;
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
        await _transaction.DisposeAsync();
    }
}