namespace Market.Application.Common;

public sealed class PageResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int PageSize { get; init; }
    public required int CurrentPage { get; init; }
    public required bool IncludedTotal { get; init; }
    public required int TotalItems { get; init; }
    public required int TotalPages { get; init; }

    /// <summary>
    /// Creates a PageResult from an IQueryable using EF Core asynchronous methods.
    /// </summary>
    public static async Task<PageResult<T>> FromQueryableAsync(
        IQueryable<T> query,
        PageRequest paging,
        CancellationToken ct = default,
        bool includeTotal = true)
    {
        int total = 0;
        if (includeTotal)
            total = await query.CountAsync(ct);

        var items = await query
            .Skip(paging.SkipCount)
            .Take(paging.PageSize)
            .ToListAsync(ct);

        return new PageResult<T> {
            Items = items,
            PageSize = paging.PageSize,
            CurrentPage = paging.Page,
            IncludedTotal = includeTotal,
            TotalItems = total,
            TotalPages = includeTotal ? (int)Math.Ceiling(total / (double)paging.PageSize) : 0
        };
    }
}