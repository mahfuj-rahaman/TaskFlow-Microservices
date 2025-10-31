namespace TaskFlow.BuildingBlocks.Common.Pagination;

/// <summary>
/// Represents a paginated list of items
/// </summary>
/// <typeparam name="T">The type of items in the list</typeparam>
public sealed class PagedList<T>
{
    /// <summary>
    /// The items in the current page
    /// </summary>
    public List<T> Items { get; init; } = new();

    /// <summary>
    /// The current page number (1-based)
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// The number of items per page
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// The total number of items across all pages
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// The total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Creates a new paginated list
    /// </summary>
    public static PagedList<T> Create(
        IEnumerable<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        return new PagedList<T>
        {
            Items = items.ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Creates an empty paginated list
    /// </summary>
    public static PagedList<T> Empty(int pageNumber, int pageSize)
    {
        return new PagedList<T>
        {
            Items = new List<T>(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = 0
        };
    }
}
