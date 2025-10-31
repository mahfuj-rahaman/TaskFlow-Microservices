# 🔄 Caching Provider Switching Guide

**Version**: 1.0.0
**Last Updated**: 2025-11-01
**Purpose**: Guide for switching between caching providers without vendor lock-in

---

## 🎯 Overview

The TaskFlow Caching BuildingBlock supports **3 caching strategies** with **zero code changes** required to switch between them:

1. **Memory** - In-memory caching (fast, single instance)
2. **Redis** - Distributed caching (shared across instances)
3. **Hybrid** - L1 (Memory) + L2 (Redis) for best performance

**Cloud Support:**
- ✅ **Azure Redis Cache**
- ✅ **AWS ElastiCache for Redis**
- ✅ **Google Cloud Memorystore**
- ✅ **Self-hosted Redis**

---

## 🔄 How to Switch Caching Providers

### 1. Memory Cache (In-Memory)

**Use Case**: Single-instance apps, development, testing

**appsettings.json**:
```json
{
  "Caching": {
    "Provider": "Memory",
    "Memory": {
      "SizeLimit": 1024,
      "CompactionPercentage": 0.25
    }
  }
}
```

**Benefits**:
- ✅ No external dependencies
- ✅ Fastest performance
- ✅ Zero setup

**Limitations**:
- ❌ Not shared across instances
- ❌ Lost on restart

---

### 2. Redis Cache (Distributed)

**Use Case**: Multi-instance deployments, production, cloud

**appsettings.json**:
```json
{
  "Caching": {
    "Provider": "Redis",
    "Redis": {
      "ConnectionString": "localhost:6379",
      "InstanceName": "TaskFlow:",
      "DefaultExpiration": "00:10:00"
    }
  }
}
```

**Benefits**:
- ✅ Shared across all instances
- ✅ Persistent (optional)
- ✅ Scalable

**Limitations**:
- ❌ Network latency
- ❌ Requires Redis server

---

### 3. Hybrid Cache (L1 + L2)

**Use Case**: High-performance production environments

**appsettings.json**:
```json
{
  "Caching": {
    "Provider": "Hybrid",
    "Memory": {
      "SizeLimit": 512,
      "CompactionPercentage": 0.25
    },
    "Redis": {
      "ConnectionString": "localhost:6379",
      "InstanceName": "TaskFlow:",
      "DefaultExpiration": "00:10:00"
    }
  }
}
```

**How it Works**:
1. Check L1 (memory) first → **fastest**
2. If miss, check L2 (Redis) → **shared**
3. If found in L2, populate L1 → **future hits are fast**

**Benefits**:
- ✅ Best of both worlds
- ✅ Fastest performance for hot data
- ✅ Shared cache for consistency

---

## ☁️ Cloud Provider Examples

### Azure Redis Cache

**appsettings.Production.json**:
```json
{
  "Caching": {
    "Provider": "Redis",
    "Redis": {
      "ConnectionString": "taskflow.redis.cache.windows.net:6380,password=${AZURE_REDIS_KEY},ssl=True,abortConnect=False",
      "InstanceName": "TaskFlow:Production:",
      "DefaultExpiration": "00:15:00"
    }
  }
}
```

**Azure Setup**:
```bash
# Create Azure Redis Cache
az redis create \
  --name taskflow-cache \
  --resource-group taskflow-rg \
  --location eastus \
  --sku Basic \
  --vm-size c0

# Get connection string
az redis list-keys \
  --name taskflow-cache \
  --resource-group taskflow-rg
```

---

### AWS ElastiCache for Redis

**appsettings.Production.json**:
```json
{
  "Caching": {
    "Provider": "Redis",
    "Redis": {
      "ConnectionString": "taskflow-cache.abc123.0001.use1.cache.amazonaws.com:6379",
      "InstanceName": "TaskFlow:Production:",
      "DefaultExpiration": "00:15:00"
    }
  }
}
```

**AWS Setup**:
```bash
# Create ElastiCache cluster
aws elasticache create-cache-cluster \
  --cache-cluster-id taskflow-cache \
  --engine redis \
  --cache-node-type cache.t3.micro \
  --num-cache-nodes 1

# Get endpoint
aws elasticache describe-cache-clusters \
  --cache-cluster-id taskflow-cache \
  --show-cache-node-info
```

---

### Google Cloud Memorystore

**appsettings.Production.json**:
```json
{
  "Caching": {
    "Provider": "Redis",
    "Redis": {
      "ConnectionString": "10.0.0.3:6379",
      "InstanceName": "TaskFlow:Production:",
      "DefaultExpiration": "00:15:00"
    }
  }
}
```

**GCP Setup**:
```bash
# Create Memorystore instance
gcloud redis instances create taskflow-cache \
  --size=1 \
  --region=us-central1 \
  --redis-version=redis_6_x

# Get connection info
gcloud redis instances describe taskflow-cache \
  --region=us-central1
```

---

## 🔄 Switching Examples

### Example 1: Development → Production (Memory → Redis)

**Before (Development)**:
```json
{
  "Caching": {
    "Provider": "Memory"
  }
}
```

**After (Production)**:
```json
{
  "Caching": {
    "Provider": "Redis",
    "Redis": {
      "ConnectionString": "${REDIS_CONNECTION_STRING}"
    }
  }
}
```

**Code changes needed**: **ZERO!** ✅

---

### Example 2: Self-Hosted → Azure Cloud (Redis → Azure Redis)

**Before (Self-Hosted)**:
```json
{
  "Caching": {
    "Provider": "Redis",
    "Redis": {
      "ConnectionString": "redis-server.company.com:6379"
    }
  }
}
```

**After (Azure)**:
```json
{
  "Caching": {
    "Provider": "Redis",
    "Redis": {
      "ConnectionString": "taskflow.redis.cache.windows.net:6380,password=${AZURE_REDIS_KEY},ssl=True"
    }
  }
}
```

**Code changes needed**: **ZERO!** ✅

---

### Example 3: Performance Optimization (Redis → Hybrid)

**Before (Redis Only)**:
```json
{
  "Caching": {
    "Provider": "Redis",
    "Redis": {
      "ConnectionString": "localhost:6379"
    }
  }
}
```

**After (Hybrid - Faster!)**:
```json
{
  "Caching": {
    "Provider": "Hybrid",
    "Memory": {
      "SizeLimit": 512
    },
    "Redis": {
      "ConnectionString": "localhost:6379"
    }
  }
}
```

**Performance Improvement**:
- L1 hits: **~0.1ms** (100x faster than Redis)
- L2 hits: **~1-2ms** (normal Redis latency)

---

## 📝 Code Usage (Same for All Providers)

### Program.cs

```csharp
// No code changes needed! Just change appsettings.json
builder.Services.AddCaching(builder.Configuration);

// Optional: Add MediatR caching behavior
builder.Services.AddCachingBehavior();
```

### Using Cache in Your Code

```csharp
public class ProductService
{
    private readonly ICacheService _cache;

    public ProductService(ICacheService cache)
    {
        _cache = cache;
    }

    // Same code works with Memory, Redis, or Hybrid!
    public async Task<Product?> GetProductAsync(Guid id, CancellationToken ct)
    {
        var cacheKey = $"product:{id}";

        return await _cache.GetOrSetAsync(
            cacheKey,
            async () => await _repository.GetByIdAsync(id, ct),
            TimeSpan.FromMinutes(10),
            ct);
    }

    public async Task InvalidateCacheAsync(Guid id, CancellationToken ct)
    {
        await _cache.RemoveAsync($"product:{id}", ct);
    }
}
```

### Using Cache with MediatR Queries

```csharp
// Query with automatic caching
public sealed record GetProductByIdQuery(Guid Id)
    : IRequest<Result<ProductDto>>, ICacheableQuery
{
    public string CacheKey => $"product:{Id}";
    public TimeSpan? CacheExpiration => TimeSpan.FromMinutes(10);
}

// Handler doesn't need cache logic - handled by CachingBehavior!
public sealed class GetProductByIdQueryHandler
    : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(
        GetProductByIdQuery request,
        CancellationToken ct)
    {
        // This result will be automatically cached!
        var product = await _repository.GetByIdAsync(request.Id, ct);
        return Result.Success(product.Adapt<ProductDto>());
    }
}
```

---

## 🔧 Environment-Specific Configuration

### Using .env File

**.env.development**:
```bash
# Development - Use in-memory for speed
CACHING_PROVIDER="Memory"
```

**.env.staging**:
```bash
# Staging - Use Redis
CACHING_PROVIDER="Redis"
REDIS_CONNECTION_STRING="staging-redis.company.com:6379"
```

**.env.production**:
```bash
# Production - Use Hybrid for best performance
CACHING_PROVIDER="Hybrid"
REDIS_CONNECTION_STRING="taskflow.redis.cache.windows.net:6380,password=${AZURE_REDIS_KEY},ssl=True"
```

### appsettings.json with Environment Variables

```json
{
  "Caching": {
    "Provider": "${CACHING_PROVIDER:Memory}",
    "Redis": {
      "ConnectionString": "${REDIS_CONNECTION_STRING:localhost:6379}",
      "InstanceName": "${REDIS_INSTANCE_NAME:TaskFlow:}"
    }
  }
}
```

---

## 🐳 Docker Compose Examples

### Example 1: Development with Memory

**docker-compose.dev.yml**:
```yaml
version: '3.8'

services:
  order-service:
    image: taskflow/order-service
    environment:
      Caching__Provider: "Memory"
```

---

### Example 2: Production with Redis

**docker-compose.prod.yml**:
```yaml
version: '3.8'

services:
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    command: redis-server --appendonly yes

  order-service:
    image: taskflow/order-service
    environment:
      Caching__Provider: "Redis"
      Caching__Redis__ConnectionString: "redis:6379"
    depends_on:
      - redis

volumes:
  redis-data:
```

---

### Example 3: Production with Hybrid

**docker-compose.hybrid.yml**:
```yaml
version: '3.8'

services:
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  order-service:
    image: taskflow/order-service
    environment:
      Caching__Provider: "Hybrid"
      Caching__Memory__SizeLimit: 512
      Caching__Redis__ConnectionString: "redis:6379"
    depends_on:
      - redis
```

---

## 📊 Provider Comparison

| Feature | Memory | Redis | Hybrid |
|---------|--------|-------|--------|
| **Speed** | Very Fast (~0.1ms) | Fast (~1-2ms) | Very Fast (L1) + Fast (L2) |
| **Shared Cache** | ❌ No | ✅ Yes | ✅ Yes |
| **Persistence** | ❌ No | ✅ Optional | ✅ Optional (L2) |
| **Scalability** | ❌ Single instance | ✅ Distributed | ✅ Distributed |
| **Memory Usage** | High (all data in RAM) | Low (only metadata) | Medium |
| **Setup Complexity** | None | Low | Medium |
| **Best For** | Development, Testing | Production (multi-instance) | High-traffic production |
| **Cost** | Free | Infrastructure cost | Infrastructure cost |

---

## 🎯 Best Practices

### 1. Environment-Specific Providers

```bash
Development  → Memory    (fast, no dependencies)
Staging      → Redis     (test distributed caching)
Production   → Hybrid    (best performance)
```

### 2. Connection String Security

**❌ DON'T** hardcode connection strings:
```json
{
  "Caching": {
    "Redis": {
      "ConnectionString": "password=MySecretPassword123"  // BAD!
    }
  }
}
```

**✅ DO** use environment variables:
```json
{
  "Caching": {
    "Redis": {
      "ConnectionString": "${REDIS_CONNECTION_STRING}"  // GOOD!
    }
  }
}
```

### 3. Monitor Cache Performance

```csharp
// Log cache hits/misses
_logger.LogInformation("Cache {Status} for key {Key}",
    cachedValue != null ? "HIT" : "MISS",
    cacheKey);
```

### 4. Set Appropriate TTL

```csharp
// Frequently changing data
TimeSpan.FromMinutes(1)

// Semi-static data
TimeSpan.FromMinutes(10)

// Static data
TimeSpan.FromHours(1)
```

### 5. Cache Invalidation Strategy

```csharp
// Invalidate on update
public async Task UpdateProductAsync(Product product, CancellationToken ct)
{
    await _repository.UpdateAsync(product, ct);
    await _cache.RemoveAsync($"product:{product.Id}", ct);
    await _cache.RemoveByPrefixAsync("products:list", ct); // Invalidate lists
}
```

---

## ☁️ Cloud Provider Pricing (Estimates)

### Azure Redis Cache
- **Basic (C0)**: ~$16/month (250 MB)
- **Standard (C1)**: ~$55/month (1 GB)
- **Premium (P1)**: ~$219/month (6 GB)

### AWS ElastiCache
- **cache.t3.micro**: ~$12/month (0.5 GB)
- **cache.m5.large**: ~$120/month (6.38 GB)
- **cache.r5.large**: ~$150/month (13.07 GB)

### Google Cloud Memorystore
- **Basic (1 GB)**: ~$50/month
- **Standard (1 GB)**: ~$100/month (HA)

---

## 🧪 Testing Different Providers

### Unit Tests (Memory)

```csharp
[Fact]
public async Task GetProduct_ShouldCacheResult()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddCaching(new CachingOptions
    {
        Provider = CacheProvider.Memory
    });

    // Act & Assert
    // Test your caching logic...
}
```

### Integration Tests (Redis with Testcontainers)

```csharp
[Fact]
public async Task GetProduct_ShouldShareCacheAcrossInstances()
{
    // Start Redis container
    await using var container = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    await container.StartAsync();

    var options = new CachingOptions
    {
        Provider = CacheProvider.Redis,
        Redis = new RedisOptions
        {
            ConnectionString = $"{container.Hostname}:{container.GetMappedPublicPort(6379)}"
        }
    };

    // Test distributed caching...
}
```

---

## ✅ Migration Checklist

When switching cache providers:

- [ ] Update `appsettings.json` or `.env` with new provider
- [ ] Configure cloud provider (if using Azure/AWS/GCP)
- [ ] Test in staging environment first
- [ ] Monitor cache hit rates
- [ ] Deploy to production
- [ ] Warm up cache (optional)
- [ ] Decommission old cache infrastructure (optional)

---

## 🎯 Summary

**Three cache providers available:**
✅ **Memory** - Fast, local, no dependencies
✅ **Redis** - Distributed, shared, cloud-ready
✅ **Hybrid** - Best performance (L1 + L2)

**Cloud providers supported:**
✅ Azure Redis Cache
✅ AWS ElastiCache
✅ Google Cloud Memorystore
✅ Self-hosted Redis

**Switch by changing configuration only** - no code changes required! 🎉
