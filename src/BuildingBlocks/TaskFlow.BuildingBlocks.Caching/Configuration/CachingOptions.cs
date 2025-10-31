namespace TaskFlow.BuildingBlocks.Caching.Configuration;

/// <summary>
/// Configuration options for caching
/// </summary>
public sealed class CachingOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Caching";

    /// <summary>
    /// Cache provider type (Memory, Redis, Hybrid)
    /// </summary>
    public CacheProvider Provider { get; set; } = CacheProvider.Memory;

    /// <summary>
    /// Redis configuration options
    /// </summary>
    public RedisOptions Redis { get; set; } = new();

    /// <summary>
    /// Memory cache configuration options
    /// </summary>
    public MemoryOptions Memory { get; set; } = new();
}

/// <summary>
/// Cache provider types
/// </summary>
public enum CacheProvider
{
    /// <summary>
    /// In-memory caching only
    /// </summary>
    Memory,

    /// <summary>
    /// Distributed Redis caching only
    /// </summary>
    Redis,

    /// <summary>
    /// Hybrid (L1 memory + L2 Redis)
    /// </summary>
    Hybrid
}

/// <summary>
/// Redis cache configuration
/// </summary>
public sealed class RedisOptions
{
    /// <summary>
    /// Redis connection string
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Redis instance name (prefix for all keys)
    /// </summary>
    public string InstanceName { get; set; } = "TaskFlow:";

    /// <summary>
    /// Default expiration time
    /// </summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(10);
}

/// <summary>
/// Memory cache configuration
/// </summary>
public sealed class MemoryOptions
{
    /// <summary>
    /// Maximum cache size in MB (null for unlimited)
    /// </summary>
    public long? SizeLimit { get; set; } = 1024;

    /// <summary>
    /// Compaction percentage (0.0 - 1.0)
    /// </summary>
    public double CompactionPercentage { get; set; } = 0.25;
}
