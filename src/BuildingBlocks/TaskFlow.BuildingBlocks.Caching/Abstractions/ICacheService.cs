namespace TaskFlow.BuildingBlocks.Caching.Abstractions;

/// <summary>
/// Abstraction for caching service supporting both distributed and in-memory caching
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from cache
    /// </summary>
    /// <typeparam name="T">The type of the cached value</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cached value or null if not found</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in cache
    /// </summary>
    /// <typeparam name="T">The type of the value to cache</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="value">The value to cache</param>
    /// <param name="expiration">Optional expiration time (default: 10 minutes)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a value from cache
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cache entries with keys starting with the specified prefix
    /// </summary>
    /// <param name="prefix">The key prefix</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a cache entry exists
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the entry exists, false otherwise</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value from cache or sets it using the factory function if not found (cache-aside pattern)
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">Function to generate the value if not cached</param>
    /// <param name="expiration">Optional expiration time (default: 10 minutes)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cached or newly generated value</returns>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
}
