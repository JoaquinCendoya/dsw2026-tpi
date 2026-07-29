using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : EntityBase;
    Task<int> SaveChangesAsync();
    Task<ITransaction> BeginTransactionAsync();
}
