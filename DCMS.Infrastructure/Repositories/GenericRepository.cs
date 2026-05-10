using System.Linq.Expressions;
using DCMS.Domain.Interfaces.Repositories;
using DCMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DCMS.Infrastructure.Repositories;

/// <summary>
/// Generic EF Core repository implementation.
///
/// FIX-4: IQueryable&lt;T&gt; Query() removed — see IGenericRepository.
///        GetAllAsync replaces ListAllAsync.
///        GetPagedAsync added for bounded result sets (RISK-5).
///
/// Constraint changed from BaseEntity to class to support all entity types
/// registered in ApplicationDbContext (e.g. Branch, FAQ, Service).
/// </summary>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _dbSet.FindAsync([id], ct);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet.AsNoTracking().ToListAsync(ct);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.AsNoTracking().Where(predicate).ToListAsync(ct);

    public async Task<PagedResult<T>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking();
        if (predicate is not null)
            query = query.Where(predicate);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>(items, totalCount, page, pageSize);
    }

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _dbSet.AddAsync(entity, ct);

    public void Update(T entity)
        => _dbSet.Update(entity);

    public void Remove(T entity)
        => _dbSet.Remove(entity);

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.AnyAsync(predicate, ct);

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
        => predicate is null
            ? await _dbSet.CountAsync(ct)
            : await _dbSet.CountAsync(predicate, ct);
}
