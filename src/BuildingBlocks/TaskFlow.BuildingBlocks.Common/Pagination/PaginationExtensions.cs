using Microsoft.EntityFrameworkCore;

namespace TaskFlow.BuildingBlocks.Common.Pagination;

/// <summary>
/// Extension methods for pagination
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Converts an IQueryable to a paginated list asynchronously
    /// </summary>
    public static async Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1)
            pageSize = 10;

        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
            return PagedList<T>.Empty(pageNumber, pageSize);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedList<T>.Create(items, pageNumber, pageSize, totalCount);
    }

    /// <summary>
    /// Converts an IEnumerable to a paginated list synchronously
    /// </summary>
    public static PagedList<T> ToPagedList<T>(
        this IEnumerable<T> source,
        int pageNumber,
        int pageSize)
    {
        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1)
            pageSize = 10;

        var enumerable = source as T[] ?? source.ToArray();
        var totalCount = enumerable.Length;

        if (totalCount == 0)
            return PagedList<T>.Empty(pageNumber, pageSize);

        var items = enumerable
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return PagedList<T>.Create(items, pageNumber, pageSize, totalCount);
    }
}
