# EventBus with NoSQL Databases

## Overview

The Outbox pattern now supports **ANY database** - SQL or NoSQL! Each service can choose its own database technology while still benefiting from reliable event delivery.

## Supported Databases

| Database | Adapter | Best For | Setup Complexity |
|----------|---------|----------|------------------|
| **PostgreSQL** | `EfCoreEventStore` | Most services, ACID transactions | ⭐ Easy |
| **SQL Server** | `EfCoreEventStore` | Enterprise .NET apps | ⭐ Easy |
| **MySQL** | `EfCoreEventStore` | Web applications | ⭐ Easy |
| **MongoDB** | `MongoDbEventStore` | Document-oriented data | ⭐⭐ Medium |
| **Cassandra** | `CassandraEventStore` | High-throughput, distributed | ⭐⭐⭐ Complex |
| **Redis** | `RedisEventStore` | Ultra-fast, cache-like | ⭐⭐ Medium |
| **Raw SQL** | `SqlEventStore` | When EF Core not wanted | ⭐⭐ Medium |

---

## MongoDB Setup

### Use Case
Services that use MongoDB as their primary database (e.g., Analytics Service, Logging Service)

### Installation

```bash
dotnet add package MongoDB.Driver
```

### Configuration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register MongoDB client
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient("mongodb://localhost:27017"));

// Register database
builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("analytics_service"));

// Register MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Register EventBus with MongoDB
builder.Services.AddPersistentEventBusWithMediatRAndMongoDB(
    mode: EventBusMode.Hybrid);

var app = builder.Build();
app.Run();
```

### MongoDB Schema

The adapter automatically creates indexes. No manual schema setup needed!

```javascript
// Collection: outbox_events
{
  "_id": "a1b2c3d4-...",  // GUID
  "eventType": "AnalyticsEventRecorded",
  "eventData": "{...}",   // JSON
  "aggregateId": "user-123",
  "occurredAt": ISODate("2025-11-03T..."),
  "isPublished": false,
  "retryCount": 0
}

// Indexes (auto-created):
db.outbox_events.createIndex({ "isPublished": 1, "isFailed": 1, "createdAt": 1 })
db.outbox_events.createIndex({ "aggregateId": 1 })
db.outbox_events.createIndex({ "eventType": 1 })
```

### Benefits
✅ Perfect for document-oriented services
✅ Flexible schema
✅ Automatic index creation
✅ Works with MongoDB Atlas (cloud)

---

## Cassandra Setup

### Use Case
High-throughput services with massive scale (e.g., IoT data, time-series analytics)

### Installation

```bash
dotnet add package CassandraCSharpDriver
```

### Configuration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register Cassandra cluster
builder.Services.AddSingleton<ICluster>(sp =>
    Cluster.Builder()
        .AddContactPoint("localhost")
        .WithPort(9042)
        .Build());

// Register session
builder.Services.AddSingleton<ISession>(sp =>
    sp.GetRequiredService<ICluster>().Connect("iot_service"));

// Register MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Register EventBus with Cassandra
builder.Services.AddPersistentEventBusWithMediatRAndCassandra(
    mode: EventBusMode.Hybrid);

var app = builder.Build();
app.Run();
```

### Cassandra Schema

```sql
-- Create keyspace (run once)
CREATE KEYSPACE IF NOT EXISTS iot_service
WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 3};

USE iot_service;

-- Create table
CREATE TABLE IF NOT EXISTS outbox_events (
    id uuid PRIMARY KEY,
    event_type text,
    event_data text,
    aggregate_id uuid,
    aggregate_type text,
    occurred_at timestamp,
    created_at timestamp,
    is_published boolean,
    published_at timestamp,
    retry_count int,
    error_message text,
    is_failed boolean
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_outbox_unpublished ON outbox_events (is_published);
CREATE INDEX IF NOT EXISTS idx_outbox_failed ON outbox_events (is_failed);
CREATE INDEX IF NOT EXISTS idx_outbox_aggregate ON outbox_events (aggregate_id);
```

### Benefits
✅ Handles millions of events/second
✅ Distributed, no single point of failure
✅ Works with ScyllaDB (Cassandra-compatible)
✅ Linear scalability

### Trade-offs
⚠️ More complex setup
⚠️ Eventual consistency model
⚠️ No ACID transactions across rows

---

## Redis Setup

### Use Case
Ultra-fast event processing with short-lived events (e.g., real-time notifications, caching layer)

### Installation

```bash
dotnet add package StackExchange.Redis
```

### Configuration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect("localhost:6379"));

// Register MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Register EventBus with Redis
builder.Services.AddPersistentEventBusWithMediatRAndRedis(
    mode: EventBusMode.Hybrid);

var app = builder.Build();
app.Run();
```

### Redis Persistence Configuration

**CRITICAL**: Redis is volatile by default! Enable persistence:

```bash
# Edit redis.conf

# Enable AOF (append-only file)
appendonly yes
appendfsync everysec  # fsync every second

# Also enable RDB snapshots
save 900 1      # save after 15 min if 1 key changed
save 300 10     # save after 5 min if 10 keys changed
save 60 10000   # save after 1 min if 10000 keys changed
```

### Benefits
✅ Blazing fast (millions of ops/second)
✅ Built-in TTL (auto-expire old events)
✅ Simple key-value model
✅ Works as cache + outbox

### Trade-offs
⚠️ Volatile by default (MUST enable AOF+RDB)
⚠️ Limited query capabilities vs SQL
⚠️ Memory-based (more expensive at scale)

### Best Practice

Use Redis as **secondary cache** for recent events:

```csharp
// Hybrid approach: PostgreSQL primary + Redis cache
builder.Services.AddSingleton<IConnectionMultiplexer>(...);
builder.Services.AddDbContext<AppDbContext>(...);

// Register both stores
builder.Services.AddScoped<IEventStore, EfCoreEventStore>(); // Primary
builder.Services.AddSingleton<RedisEventStore>(); // Cache (register separately)
```

---

## Real-World Architecture Examples

### Example 1: Polyglot Microservices

```
┌─────────────────────────────┐
│ Identity Service            │
│ PostgreSQL + EfCore         │  ← Most services use this
│                             │
│ builder.Services            │
│   .AddPersistentEventBus    │
│   WithMediatRAndEfCore()    │
└─────────────────────────────┘

┌─────────────────────────────┐
│ Analytics Service           │
│ MongoDB                     │  ← Document-oriented data
│                             │
│ builder.Services            │
│   .AddPersistentEventBus    │
│   WithMediatRAndMongoDB()   │
└─────────────────────────────┘

┌─────────────────────────────┐
│ IoT Telemetry Service       │
│ Cassandra                   │  ← Massive scale
│                             │
│ builder.Services            │
│   .AddPersistentEventBus    │
│   WithMediatRAndCassandra() │
└─────────────────────────────┘

┌─────────────────────────────┐
│ Notification Service        │
│ PostgreSQL + Redis Cache    │  ← Hybrid approach
│                             │
│ builder.Services            │
│   .AddPersistentEventBus    │
│   WithMediatRAndEfCore()    │
│ // Also register Redis      │
│ // for caching recent       │
└─────────────────────────────┘
```

### Example 2: Event Flow Across Different Databases

```
1. User registers in Identity Service (PostgreSQL)
   ↓
   UserCreatedEvent saved to PostgreSQL outbox
   ↓
2. OutboxProcessor publishes to RabbitMQ
   ↓
3. Analytics Service receives event (MongoDB)
   ↓
   AnalyticsRecordedEvent saved to MongoDB outbox
   ↓
4. IoT Service logs telemetry (Cassandra)
   ↓
   TelemetryLoggedEvent saved to Cassandra outbox
   ↓
5. Notification Service sends email (PostgreSQL)
   ↓
   NotificationSentEvent saved to PostgreSQL outbox

All services use Outbox pattern, but with DIFFERENT databases!
```

---

## Performance Comparison

| Database | Write Latency | Read Latency | Throughput | Scalability |
|----------|---------------|--------------|------------|-------------|
| **PostgreSQL** | ~1ms | ~1ms | 10K ops/s | Vertical |
| **MongoDB** | ~2ms | ~1ms | 50K ops/s | Horizontal |
| **Cassandra** | ~2ms | ~2ms | 1M ops/s | Horizontal |
| **Redis** | ~0.1ms | ~0.1ms | 10M ops/s | Horizontal |

*Approximate numbers, vary by hardware/configuration*

---

## Choosing the Right Database

### Use EfCore (PostgreSQL/SQL Server/MySQL) if:
✅ You need ACID transactions
✅ You already use EF Core in the service
✅ Data has complex relationships
✅ Standard microservice (<100K events/day)

### Use MongoDB if:
✅ Service uses document-oriented data
✅ Flexible schema needed
✅ JSON-heavy workloads
✅ Read-heavy with moderate writes

### Use Cassandra if:
✅ Massive scale (millions of events/day)
✅ Time-series data
✅ IoT/telemetry workloads
✅ Multi-datacenter replication needed

### Use Redis if:
✅ Ultra-low latency required (<1ms)
✅ Events are short-lived (TTL)
✅ Used as cache layer
✅ Real-time scenarios

---

## Migration Between Databases

You can migrate between databases without changing application code!

```csharp
// BEFORE: Using MongoDB
builder.Services.AddPersistentEventBusWithMediatRAndMongoDB();

// AFTER: Migrating to Cassandra (zero code changes!)
builder.Services.AddPersistentEventBusWithMediatRAndCassandra();

// Your code stays the same:
await _eventBus.PublishAsync(new MyEvent());
```

---

## Custom Database Adapter

Need a database we don't support? Implement `IEventStore`:

```csharp
public class MyCustomDbEventStore : IEventStore
{
    public async Task SaveEventAsync<TEvent>(TEvent @event, ...)
    {
        // Your custom database logic
    }

    public async Task<IReadOnlyList<StoredEvent>> GetUnpublishedEventsAsync(...)
    {
        // Your custom query logic
    }

    // Implement other methods...
}

// Register it:
builder.Services.AddPersistentEventBus<MediatREventPublisher, MyCustomDbEventStore>();
```

Examples we can add:
- Azure CosmosDB
- Amazon DynamoDB
- Google Cloud Firestore
- CouchDB
- RavenDB
- Neo4j (graph DB)

---

## Best Practices

### 1. Match Event Store to Service Database
✅ **Good**: Identity uses PostgreSQL → Use `EfCoreEventStore`
✅ **Good**: Analytics uses MongoDB → Use `MongoDbEventStore`
❌ **Bad**: Service uses MongoDB, but events in PostgreSQL (extra DB to manage)

### 2. Enable Persistence
- **PostgreSQL/MySQL/SQL Server**: Built-in persistence ✅
- **MongoDB**: Enable journaling (default) ✅
- **Cassandra**: Enable commitlog (default) ✅
- **Redis**: Enable AOF+RDB ⚠️ **CRITICAL!**

### 3. Monitor Outbox Health
```csharp
// Add health check
builder.Services.AddHealthChecks()
    .AddCheck<OutboxHealthCheck>("outbox");
```

### 4. Clean Up Old Events
```javascript
// MongoDB: Delete old published events (run daily)
db.outbox_events.deleteMany({
  isPublished: true,
  publishedAt: { $lt: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000) }
})
```

---

## Summary

✅ **Outbox pattern works with ANY database**
✅ **Each service chooses its own database**
✅ **Same IEventStore interface for all**
✅ **No vendor lock-in**
✅ **True polyglot microservices**

```
Your Choice of Database + Outbox Pattern = Reliable Events! 🎉
```

Need help choosing? Ask yourself:
- What database does my service already use? → **Use that!**
- Do I need ACID transactions? → **PostgreSQL/SQL Server**
- Do I have massive scale? → **Cassandra**
- Do I need ultra-low latency? → **Redis**
- Do I have document data? → **MongoDB**
