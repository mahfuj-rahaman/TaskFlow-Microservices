namespace TaskFlow.BuildingBlocks.Caching.Abstractions;

/// <summary>
/// Marker interface for queries that should be cached by the CachingBehavior pipeline
/// </summary>
public interface ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for this query
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Gets the cache expiration time (null for default expiration)
    /// </summary>
    TimeSpan? CacheExpiration { get; }
}
