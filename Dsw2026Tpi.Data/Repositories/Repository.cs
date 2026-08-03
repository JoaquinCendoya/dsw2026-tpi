using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dsw2026Tpi.Data.Repositories;

public class Repository<T> : IRepository<T> where T : EntityBase
{
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.Deleted);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.Where(e => !e.Deleted).ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(e => !e.Deleted).Where(predicate).ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params string[] includes)
    {
        var query = _dbSet.Where(e => !e.Deleted).AsQueryable();

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.Where(predicate).ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(T entity)
    {
        entity.Delete();
        _dbSet.Update(entity);
    }
    public async Task<Pagination<T>> PaginateAsync<TKey>(
    int pageSize,
    int pageIndex,
    Expression<Func<T, bool>> predicate,
    Expression<Func<T, TKey>> sortOrder,
    bool descending = false,
    params string[] includes)
    {
        pageSize = Math.Abs(pageSize);
        pageIndex = Math.Abs(pageIndex) == 0 ? 0 : Math.Abs(pageIndex) - 1;

        var query = _dbSet.Where(e => !e.Deleted);

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        var sorted = query.Where(predicate);
        var filtered = descending ? sorted.OrderByDescending(sortOrder) : sorted.OrderBy(sortOrder);
        var total = await filtered.CountAsync();

        async Task<Pagination<T>> GetPage(int skip, int take)
        {
            var data = await filtered.Skip(skip).Take(take).ToListAsync();
            return new Pagination<T>(pageSize, pageIndex, total, data);
        }

        if (total > pageSize * pageIndex)
            return await GetPage(pageIndex * pageSize, pageSize);

        if (total < pageSize)
            return new Pagination<T>(pageSize, pageIndex, total, await filtered.ToListAsync());

        var targetPageIndex = pageIndex - 1;
        while (true)
        {
            if (total > targetPageIndex * pageSize)
                return await GetPage(targetPageIndex * pageSize, pageSize);

            targetPageIndex--;
            if (targetPageIndex < 0) return new Pagination<T>(pageSize, 0, 0, []);
        }
    }
}