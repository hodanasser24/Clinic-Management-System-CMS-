using System.Linq.Expressions;

namespace DCMS.Domain.Interfaces.Repositories;

/// <summary>
/// Generic repository contract.
///
/// FIX-4: IQueryable&lt;T&gt; Query() has been intentionally omitted.
///   Returning IQueryable leaks EF Core abstractions into the Application layer
///   and breaks dependency inversion. Any complex query need must be expressed
///   as an explicit named method on the appropriate specialized repository.
/// </summary>
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// Paginated query with optional predicate filter.
    /// Phase 4 (RISK-5): prevents unbounded result sets on list endpoints.
    /// </summary>
    Task<PagedResult<T>> GetPagedAsync(
        int page,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default);

    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
}

/// <summary>Lightweight paged result carrier used by GetPagedAsync.</summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public PagedResult(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}
