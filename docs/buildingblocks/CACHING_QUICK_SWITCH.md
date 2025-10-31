# ⚡ Caching Quick Switch Guide - 30 Seconds

---

## 🎯 How to Switch Caching Providers

### Method 1: Using .env File (EASIEST!) ⭐

#### Switch to Memory Cache (Development)
```bash
# .env
CACHING_PROVIDER="Memory"
```
**Restart service** → Done! ✅

#### Switch to Redis Cache (Production)
```bash
# .env
CACHING_PROVIDER="Redis"
REDIS_CONNECTION_STRING="localhost:6379"
```
**Restart service** → Done! ✅

#### Switch to Hybrid Cache (Best Performance)
```bash
# .env
CACHING_PROVIDER="Hybrid"
REDIS_CONNECTION_STRING="localhost:6379"
MEMORY_CACHE_SIZE_LIMIT_MB="512"
```
**Restart service** → Done! ✅

---

### Method 2: Using appsettings.json

#### Memory Cache
```json
{
  "Caching": {
    "Provider": "Memory",
    "Memory": {
      "SizeLimit": 1024
    }
  }
}
```

#### Redis Cache
```json
{
  "Caching": {
    "Provider": "Redis",
    "Redis": {
      "ConnectionString": "localhost:6379",
      "InstanceName": "TaskFlow:"
    }
  }
}
```

#### Hybrid Cache (L1 + L2)
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

---

## ☁️ Cloud Provider Examples

### Azure Redis Cache
```bash
# .env
CACHING_PROVIDER="Redis"
REDIS_CONNECTION_STRING="taskflow.redis.cache.windows.net:6380,password=YOUR_KEY,ssl=True"
```

### AWS ElastiCache
```bash
# .env
CACHING_PROVIDER="Redis"
REDIS_CONNECTION_STRING="taskflow-cache.abc123.0001.use1.cache.amazonaws.com:6379"
```

### Google Cloud Memorystore
```bash
# .env
CACHING_PROVIDER="Redis"
REDIS_CONNECTION_STRING="10.0.0.3:6379"
```

---

## 📊 Quick Comparison

| Provider | Speed | Shared? | Setup | Best For |
|----------|-------|---------|-------|----------|
| **Memory** | ⚡⚡⚡ Very Fast | ❌ No | ✅ None | Development, Single instance |
| **Redis** | ⚡⚡ Fast | ✅ Yes | 🔧 Easy | Production, Multi-instance |
| **Hybrid** | ⚡⚡⚡ Very Fast + ⚡⚡ Fast | ✅ Yes | 🔧 Medium | High-traffic production |

---

## 🔄 Complete .env Example

```bash
# ======================
# Caching Configuration
# ======================
# Choose ONE provider: Memory, Redis, Hybrid

# Option 1: Memory (Uncomment to use)
CACHING_PROVIDER="Memory"
MEMORY_CACHE_SIZE_LIMIT_MB="1024"

# Option 2: Redis (Uncomment to use)
# CACHING_PROVIDER="Redis"
# REDIS_CONNECTION_STRING="localhost:6379"
# REDIS_INSTANCE_NAME="TaskFlow:"

# Option 3: Hybrid - Best Performance (Uncomment to use)
# CACHING_PROVIDER="Hybrid"
# MEMORY_CACHE_SIZE_LIMIT_MB="512"
# REDIS_CONNECTION_STRING="localhost:6379"
# REDIS_INSTANCE_NAME="TaskFlow:"
```

---

## 🧪 Quick Testing

### Test with Memory (No dependencies!)
```bash
CACHING_PROVIDER="Memory"
dotnet run
```

### Test with Redis (Requires Redis)
```bash
# Start Redis
docker run -d -p 6379:6379 redis:7-alpine

# Configure
CACHING_PROVIDER="Redis"
REDIS_CONNECTION_STRING="localhost:6379"

# Run
dotnet run
```

### Test with Hybrid (Best performance!)
```bash
# Start Redis
docker run -d -p 6379:6379 redis:7-alpine

# Configure
CACHING_PROVIDER="Hybrid"
REDIS_CONNECTION_STRING="localhost:6379"

# Run
dotnet run
```

---

## ❓ FAQ

**Q: Do I need to change code?**
A: ❌ NO! Just change configuration.

**Q: Can I use different providers for different services?**
A: ✅ YES! Each service has its own config.

**Q: Which is fastest?**
A:
- Memory: ~0.1ms
- Redis: ~1-2ms
- Hybrid: ~0.1ms (L1 hit) or ~1-2ms (L2 hit)

**Q: Which should I use in production?**
A:
- Single instance → **Memory**
- Multiple instances → **Redis**
- High traffic → **Hybrid**

---

## 🎯 Summary

**Three simple steps:**
1️⃣ Edit `.env` or `appsettings.json`
2️⃣ Change `Provider` value
3️⃣ Restart service

**No code changes required!** ✅
