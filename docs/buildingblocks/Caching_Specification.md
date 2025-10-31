# Caching BuildingBlock Specification

**Project**: TaskFlow.BuildingBlocks.Caching
**Version**: 1.0.0
**Last Updated**: 2025-11-01
**Status**: Specification

---

## 📋 Overview

The Caching BuildingBlock provides distributed caching with Redis and in-memory caching with automatic cache invalidation and MediatR pipeline behavior.

## 🎯 Purpose

- Reduce database load
- Improve response times
- Provide distributed caching across instances
- Support cache-aside pattern
- Automatic cache invalidation
- MediatR caching behavior for queries

## 📦 Core Components

### 1. ICacheService Interface

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);
}
```

### 2. Implementation Classes

#### RedisCacheService
```csharp
public sealed class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;

    // Uses StackExchange.Redis for distributed caching
}
```

#### MemoryCacheService
```csharp
public sealed class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    // Uses Microsoft.Extensions.Caching.Memory for in-process caching
}
```

#### HybridCacheService (L1 + L2)
```csharp
public sealed class HybridCacheService : ICacheService
{
    private readonly IMemoryCache _l1Cache; // Fast, in-process
    private readonly IConnectionMultiplexer _l2Cache; // Distributed, Redis

    // Check L1 first, fallback to L2, populate L1 on miss
}
```

### 3. MediatR Caching Behavior

```csharp
public sealed class CachingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery
{
    private readonly ICacheService _cacheService;

    public async Task<TResponse> Handle(TRequest request,
        RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var cacheKey = request.CacheKey;

        var cached = await _cacheService.GetAsync<TResponse>(cacheKey, ct);
        if (cached is not null)
            return cached;

        var response = await next();

        await _cacheService.SetAsync(cacheKey, response, request.CacheExpiration, ct);

        return response;
    }
}

public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan? CacheExpiration { get; }
}
```

## 📝 Usage Examples

### 1. Direct Cache Usage

```csharp
public sealed class ProductService
{
    private readonly ICacheService _cache;
    private readonly IProductRepository _repository;

    public async Task<Product?> GetProductAsync(Guid id, CancellationToken ct)
    {
        var cacheKey = $"product:{id}";

        return await _cache.GetOrSetAsync(
            cacheKey,
            () => _repository.GetByIdAsync(id, ct),
            TimeSpan.FromMinutes(10),
            ct);
    }
}
```

### 2. Query Caching with MediatR

```csharp
public sealed record GetProductByIdQuery(Guid Id)
    : IRequest<Result<ProductDto>>, ICacheableQuery
{
    public string CacheKey => $"product:{Id}";
    public TimeSpan? CacheExpiration => TimeSpan.FromMinutes(10);
}

// Handler doesn't need cache logic - it's handled by CachingBehavior
public sealed class GetProductByIdQueryHandler
    : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(request.Id, ct);
        return product is null
            ? Result.Failure<ProductDto>("Product not found")
            : Result.Success(product.Adapt<ProductDto>());
    }
}
```

### 3. Cache Invalidation

```csharp
public sealed class UpdateProductCommandHandler
{
    private readonly ICacheService _cache;
    private readonly IProductRepository _repository;

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(request.Id, ct);
        product.Update(request.Name, request.Price);

        await _repository.UpdateAsync(product, ct);

        // Invalidate cache
        await _cache.RemoveAsync($"product:{request.Id}", ct);

        return Result.Success();
    }
}
```

## ⚙️ Configuration

### appsettings.json
```json
{
  "Caching": {
    "Provider": "Redis",
    "Redis": {
      "ConnectionString": "localhost:6379",
      "InstanceName": "TaskFlow:",
      "DefaultExpiration": "00:10:00"
    },
    "Memory": {
      "SizeLimit": 1024,
      "CompactionPercentage": 0.25
    }
  }
}
```

### DependencyInjection
```csharp
public static IServiceCollection AddCaching(
    this IServiceCollection services,
    IConfiguration configuration)
{
    var options = configuration.GetSection("Caching").Get<CachingOptions>();

    services.AddStackExchangeRedisCache(opt =>
    {
        opt.Configuration = options.Redis.ConnectionString;
        opt.InstanceName = options.Redis.InstanceName;
    });

    services.AddMemoryCache();
    services.AddScoped<ICacheService, HybridCacheService>();

    return services;
}
```

## 🔧 Dependencies

- `StackExchange.Redis` (2.8.16)
- `Microsoft.Extensions.Caching.Memory` (8.0.0)
- `Microsoft.Extensions.Caching.StackExchangeRedis` (8.0.10)

## ✅ Features

- [x] Distributed caching (Redis)
- [x] In-memory caching
- [x] Hybrid L1/L2 caching
- [x] Automatic serialization (JSON)
- [x] Cache-aside pattern
- [x] MediatR pipeline behavior
- [x] Cache key patterns
- [x] TTL/Expiration support
- [x] Cache invalidation (single + prefix)

## 🎯 Success Criteria

1. ✅ Cache hit rate > 80% for frequently accessed data
2. ✅ Response time improvement > 50% for cached queries
3. ✅ Zero stale data issues
4. ✅ Graceful degradation when cache unavailable

---

**Next**: See `Common_Specification.md` for additional BuildingBlocks
