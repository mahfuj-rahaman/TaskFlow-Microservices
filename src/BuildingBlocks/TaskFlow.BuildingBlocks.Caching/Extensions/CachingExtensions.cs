using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TaskFlow.BuildingBlocks.Caching.Abstractions;
using TaskFlow.BuildingBlocks.Caching.Behaviors;
using TaskFlow.BuildingBlocks.Caching.Configuration;
using TaskFlow.BuildingBlocks.Caching.Services;

namespace TaskFlow.BuildingBlocks.Caching.Extensions;

/// <summary>
/// Extension methods for registering caching services
/// </summary>
public static class CachingExtensions
{
    /// <summary>
    /// Adds caching services based on configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(CachingOptions.SectionName)
            .Get<CachingOptions>() ?? new CachingOptions();

        return services.AddCaching(options);
    }

    /// <summary>
    /// Adds caching services with custom options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="options">The caching options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        CachingOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        // Register based on provider type
        switch (options.Provider)
        {
            case CacheProvider.Memory:
                services.AddMemoryCaching(options.Memory);
                break;

            case CacheProvider.Redis:
                services.AddRedisCaching(options.Redis);
                break;

            case CacheProvider.Hybrid:
                services.AddHybridCaching(options.Memory, options.Redis);
                break;

            default:
                throw new ArgumentException($"Unsupported cache provider: {options.Provider}");
        }

        return services;
    }

    /// <summary>
    /// Adds memory cache service
    /// </summary>
    public static IServiceCollection AddMemoryCaching(
        this IServiceCollection services,
        MemoryOptions? options = null)
    {
        options ??= new MemoryOptions();

        services.AddMemoryCache(opt =>
        {
            if (options.SizeLimit.HasValue)
            {
                opt.SizeLimit = options.SizeLimit.Value;
            }
            opt.CompactionPercentage = options.CompactionPercentage;
        });

        services.AddScoped<ICacheService, MemoryCacheService>();

        return services;
    }

    /// <summary>
    /// Adds Redis cache service
    /// </summary>
    public static IServiceCollection AddRedisCaching(
        this IServiceCollection services,
        RedisOptions? options = null)
    {
        options ??= new RedisOptions();

        // Register Redis connection
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(options.ConnectionString));

        // Register StackExchangeRedisCache
        services.AddStackExchangeRedisCache(opt =>
        {
            opt.Configuration = options.ConnectionString;
            opt.InstanceName = options.InstanceName;
        });

        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }

    /// <summary>
    /// Adds hybrid cache service (L1 memory + L2 Redis)
    /// </summary>
    public static IServiceCollection AddHybridCaching(
        this IServiceCollection services,
        MemoryOptions? memoryOptions = null,
        RedisOptions? redisOptions = null)
    {
        memoryOptions ??= new MemoryOptions();
        redisOptions ??= new RedisOptions();

        // Register memory cache (L1)
        services.AddMemoryCache(opt =>
        {
            if (memoryOptions.SizeLimit.HasValue)
            {
                opt.SizeLimit = memoryOptions.SizeLimit.Value;
            }
            opt.CompactionPercentage = memoryOptions.CompactionPercentage;
        });

        // Register Redis connection (L2)
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisOptions.ConnectionString));

        // Register StackExchangeRedisCache
        services.AddStackExchangeRedisCache(opt =>
        {
            opt.Configuration = redisOptions.ConnectionString;
            opt.InstanceName = redisOptions.InstanceName;
        });

        services.AddScoped<ICacheService, HybridCacheService>();

        return services;
    }

    /// <summary>
    /// Adds MediatR caching behavior to the pipeline
    /// </summary>
    public static IServiceCollection AddCachingBehavior(this IServiceCollection services)
    {
        services.AddScoped(typeof(MediatR.IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        return services;
    }
}
