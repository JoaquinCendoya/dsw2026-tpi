namespace Dsw2026Tpi.Domain.Interfaces;

public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync();
    Task RollbackAsync();
}