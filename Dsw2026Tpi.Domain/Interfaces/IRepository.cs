using Dsw2026Tpi.Domain.Entities;
using System.Linq.Expressions;

namespace Dsw2026Tpi.Domain.Interfaces;

public interface IRepository<T> where T : EntityBase
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);

    // Nueva firma obligatoria para reemplazar IPersistence
    Task<Pagination<T>> PaginateAsync<TKey>(
        int pageSize,
        int pageIndex,
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TKey>> sortOrder,
        params string[] includes);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params string[] includes);
}