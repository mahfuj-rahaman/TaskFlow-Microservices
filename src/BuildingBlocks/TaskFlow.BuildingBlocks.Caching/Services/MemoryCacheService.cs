using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TaskFlow.BuildingBlocks.Caching.Abstractions;

namespace TaskFlow.BuildingBlocks.Caching.Services;

/// <summary>
/// In-memory cache service implementation using Microsoft.Extensions.Caching.Memory
/// </summary>
public sealed class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(10);

    // Track all cache keys for prefix-based removal
    private readonly HashSet<string> _cacheKeys = new();
    private readonly object _lock = new();

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_cache.TryGetValue<T>(key, out var value))
            {
                _logger.LogDebug("Cache hit for key: {CacheKey}", key);
                return Task.FromResult<T?>(value);
            }

            _logger.LogDebug("Cache miss for key: {CacheKey}", key);
            return Task.FromResult<T?>(default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from memory cache for key: {CacheKey}", key);
            return Task.FromResult<T?>(default);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var expirationTime = expiration ?? _defaultExpiration;
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expirationTime,
                PostEvictionCallbacks =
                {
                    new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = (key, value, reason, state) =>
                        {
                            // Remove from tracking set when evicted
                            lock (_lock)
                            {
                                _cacheKeys.Remove(key.ToString() ?? string.Empty);
                            }
                        }
                    }
                }
            };

            _cache.Set(key, value, cacheEntryOptions);

            // Track the key
            lock (_lock)
            {
                _cacheKeys.Add(key);
            }

            _logger.LogDebug("Cached value for key: {CacheKey} with expiration: {Expiration}", key, expirationTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in memory cache for key: {CacheKey}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            _cache.Remove(key);

            lock (_lock)
            {
                _cacheKeys.Remove(key);
            }

            _logger.LogDebug("Removed cache entry for key: {CacheKey}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from memory cache for key: {CacheKey}", key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            List<string> keysToRemove;

            lock (_lock)
            {
                keysToRemove = _cacheKeys.Where(k => k.StartsWith(prefix)).ToList();
            }

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }

            lock (_lock)
            {
                foreach (var key in keysToRemove)
                {
                    _cacheKeys.Remove(key);
                }
            }

            _logger.LogDebug("Removed {Count} cache entries with prefix: {Prefix}", keysToRemove.Count, prefix);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing values from memory cache with prefix: {Prefix}", prefix);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return Task.FromResult(_cache.TryGetValue(key, out _));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence in memory cache for key: {CacheKey}", key);
            return Task.FromResult(false);
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var cachedValue = await GetAsync<T>(key, cancellationToken);
        if (cachedValue is not null)
        {
            return cachedValue;
        }

        var value = await factory();
        await SetAsync(key, value, expiration, cancellationToken);

        return value;
    }
}
