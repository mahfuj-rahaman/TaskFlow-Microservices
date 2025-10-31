using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TaskFlow.BuildingBlocks.Caching.Abstractions;

namespace TaskFlow.BuildingBlocks.Caching.Services;

/// <summary>
/// Hybrid cache service with L1 (in-memory) and L2 (Redis) caching
/// Provides fast local cache with distributed cache fallback
/// </summary>
public sealed class HybridCacheService : ICacheService
{
    private readonly IMemoryCache _l1Cache;
    private readonly IConnectionMultiplexer _l2Cache;
    private readonly ILogger<HybridCacheService> _logger;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(10);
    private readonly TimeSpan _l1Expiration = TimeSpan.FromMinutes(5); // L1 expires faster

    // Track L1 cache keys for prefix-based removal
    private readonly HashSet<string> _l1CacheKeys = new();
    private readonly object _lock = new();

    public HybridCacheService(
        IMemoryCache l1Cache,
        IConnectionMultiplexer l2Cache,
        ILogger<HybridCacheService> logger)
    {
        _l1Cache = l1Cache ?? throw new ArgumentNullException(nameof(l1Cache));
        _l2Cache = l2Cache ?? throw new ArgumentNullException(nameof(l2Cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try L1 cache first (fast)
            if (_l1Cache.TryGetValue<T>(key, out var l1Value))
            {
                _logger.LogDebug("L1 cache hit for key: {CacheKey}", key);
                return l1Value;
            }

            // Try L2 cache (distributed)
            var db = _l2Cache.GetDatabase();
            var l2CachedValue = await db.StringGetAsync(key);

            if (!l2CachedValue.HasValue)
            {
                _logger.LogDebug("L1 and L2 cache miss for key: {CacheKey}", key);
                return default;
            }

            _logger.LogDebug("L2 cache hit for key: {CacheKey}", key);
            var value = JsonSerializer.Deserialize<T>(l2CachedValue.ToString());

            // Populate L1 cache for subsequent requests
            if (value is not null)
            {
                SetL1Cache(key, value, _l1Expiration);
            }

            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from hybrid cache for key: {CacheKey}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var expirationTime = expiration ?? _defaultExpiration;

            // Set in both L1 and L2
            SetL1Cache(key, value, _l1Expiration);

            var db = _l2Cache.GetDatabase();
            var serializedValue = JsonSerializer.Serialize(value);
            await db.StringSetAsync(key, serializedValue, expirationTime);

            _logger.LogDebug("Cached value in L1 and L2 for key: {CacheKey} with expiration: {Expiration}", key, expirationTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in hybrid cache for key: {CacheKey}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Remove from both L1 and L2
            _l1Cache.Remove(key);

            lock (_lock)
            {
                _l1CacheKeys.Remove(key);
            }

            var db = _l2Cache.GetDatabase();
            await db.KeyDeleteAsync(key);

            _logger.LogDebug("Removed cache entry from L1 and L2 for key: {CacheKey}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from hybrid cache for key: {CacheKey}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            // Remove from L1
            List<string> l1KeysToRemove;

            lock (_lock)
            {
                l1KeysToRemove = _l1CacheKeys.Where(k => k.StartsWith(prefix)).ToList();
            }

            foreach (var key in l1KeysToRemove)
            {
                _l1Cache.Remove(key);
            }

            lock (_lock)
            {
                foreach (var key in l1KeysToRemove)
                {
                    _l1CacheKeys.Remove(key);
                }
            }

            // Remove from L2
            var endpoints = _l2Cache.GetEndPoints();
            var server = _l2Cache.GetServer(endpoints.First());
            var db = _l2Cache.GetDatabase();

            var l2Keys = server.Keys(pattern: $"{prefix}*").ToArray();

            if (l2Keys.Length > 0)
            {
                await db.KeyDeleteAsync(l2Keys);
            }

            _logger.LogDebug("Removed {L1Count} L1 and {L2Count} L2 cache entries with prefix: {Prefix}",
                l1KeysToRemove.Count, l2Keys.Length, prefix);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing values from hybrid cache with prefix: {Prefix}", prefix);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check L1 first
            if (_l1Cache.TryGetValue(key, out _))
            {
                return true;
            }

            // Check L2
            var db = _l2Cache.GetDatabase();
            return await db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence in hybrid cache for key: {CacheKey}", key);
            return false;
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

    private void SetL1Cache<T>(string key, T value, TimeSpan expiration)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration,
            PostEvictionCallbacks =
            {
                new PostEvictionCallbackRegistration
                {
                    EvictionCallback = (key, value, reason, state) =>
                    {
                        lock (_lock)
                        {
                            _l1CacheKeys.Remove(key.ToString() ?? string.Empty);
                        }
                    }
                }
            }
        };

        _l1Cache.Set(key, value, cacheEntryOptions);

        lock (_lock)
        {
            _l1CacheKeys.Add(key);
        }
    }
}
