# EventBus Persistence (Outbox Pattern)

## Overview

The EventBus now supports **three delivery modes** to handle different reliability requirements:

### 1. **InMemory Mode** (Default)
- ✅ Fast, immediate event delivery
- ❌ Events lost on crash/restart
- ❌ No delivery guarantee
- 🎯 **Use for**: Development, testing, non-critical events

### 2. **Persistent Mode** (Outbox Pattern)
- ✅ At-least-once delivery guarantee
- ✅ Events survive crashes
- ✅ Automatic retry with exponential backoff
- ⚠️ Small latency (background processing)
- 🎯 **Use for**: Critical events, production, financial transactions

### 3. **Hybrid Mode** (Best of Both) ⭐ **Recommended**
- ✅ Immediate in-memory delivery (fast)
- ✅ ALSO saved to outbox (reliable)
- ✅ Best of both worlds
- ⚠️ Slight overhead (double processing)
- 🎯 **Use for**: Most production scenarios

---

## Quick Start

### Step 1: Add Required NuGet Packages

For **EF Core** (recommended):
```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
```

For **PostgreSQL**:
```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

For **SQL Server**:
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

For **MySQL**:
```bash
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

### Step 2: Run Database Migration

Choose your database and run the appropriate SQL script:

**PostgreSQL**:
```bash
psql -U postgres -d your_database -f Migrations/001_CreateOutboxEventsTable.sql
```

**SQL Server**:
```sql
-- Uncomment SQL Server section in the migration file and run
sqlcmd -S localhost -d YourDatabase -i Migrations/001_CreateOutboxEventsTable.sql
```

**MySQL**:
```bash
# Uncomment MySQL section in the migration file and run
mysql -u root -p your_database < Migrations/001_CreateOutboxEventsTable.sql
```

### Step 3: Configure DbContext (EF Core)

Add `OutboxEvents` to your `DbContext`:

```csharp
using Microsoft.EntityFrameworkCore;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Your domain entities
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }

    // Outbox events table
    public DbSet<StoredEvent> OutboxEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure OutboxEvents entity
        modelBuilder.Entity<StoredEvent>(entity =>
        {
            entity.ToTable("OutboxEvents");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.IsPublished, e.IsFailed, e.CreatedAt });
            entity.HasIndex(e => e.AggregateId);
            entity.Property(e => e.EventType).HasMaxLength(500).IsRequired();
            entity.Property(e => e.EventData).IsRequired();
        });
    }
}
```

### Step 4: Register EventBus in Program.cs

Choose your configuration:

#### Option A: Hybrid Mode with MediatR + EF Core (Most Common) ⭐

```csharp
using TaskFlow.BuildingBlocks.EventBus.Extensions;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;
using TaskFlow.BuildingBlocks.EventBus.Adapters.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Register EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register EventBus with Hybrid mode (immediate + persistent)
builder.Services.AddPersistentEventBusWithMediatRAndEfCore(
    mode: EventBusMode.Hybrid,
    processorOptions: new OutboxProcessorOptions
    {
        ProcessingIntervalSeconds = 10,  // Check every 10 seconds
        BatchSize = 100,                 // Process 100 events at a time
        MaxRetryAttempts = 5             // Retry 5 times before marking as failed
    });
```

#### Option B: Persistent Mode Only (Background Processing)

```csharp
// Events are saved to outbox only, published by background worker
builder.Services.AddPersistentEventBusWithMediatRAndEfCore(
    mode: EventBusMode.Persistent);
```

#### Option C: InMemory Mode (Development/Testing)

```csharp
// No persistence, fast in-memory only
builder.Services.AddEventBusWithMediatR();  // No persistence needed
```

#### Option D: Zero Framework Dependencies

```csharp
// Use InMemoryEventPublisher (no MediatR) + EF Core persistence
builder.Services.AddPersistentEventBusWithInMemoryAndEfCore(
    mode: EventBusMode.Hybrid);
```

#### Option E: Custom Configuration

```csharp
// Full control over all components
builder.Services.AddPersistentEventBus<MediatREventPublisher, EfCoreEventStore>(
    mode: EventBusMode.Hybrid,
    processorOptions: new OutboxProcessorOptions
    {
        ProcessingIntervalSeconds = 5,
        BatchSize = 50,
        MaxRetryAttempts = 10
    });
```

---

## How It Works

### Hybrid Mode Flow (Recommended)

1. **Domain event occurs** (e.g., `UserCreatedDomainEvent`)
2. **EventBus.PublishAsync()** is called
3. **Immediate publishing**: Event published in-memory via MediatR (fast!)
4. **Outbox persistence**: Event also saved to database (reliable!)
5. **Background worker**: Periodically checks for unpublished events
6. **Retry logic**: Failed events are retried with exponential backoff
7. **Cleanup**: Successfully published events are marked as processed

### Persistent Mode Flow

1. **Domain event occurs**
2. **EventBus.PublishAsync()** saves event to database
3. **Background worker** picks up event and publishes
4. **On success**: Event marked as published
5. **On failure**: Event retried up to MaxRetryAttempts
6. **Permanent failure**: Event marked as failed for manual intervention

---

## Usage Examples

### Basic Usage (Hybrid Mode)

```csharp
public class UserService
{
    private readonly IEventBus _eventBus;

    public UserService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task CreateUserAsync(CreateUserRequest request)
    {
        // Create user
        var user = new User(request.Email, request.Name);

        // Save to database
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Publish event (immediate + persisted)
        var domainEvent = new UserCreatedDomainEvent(user.Id, user.Email);
        await _eventBus.PublishAsync(domainEvent);

        // Event is:
        // 1. Immediately published to in-process handlers (fast!)
        // 2. Saved to OutboxEvents table (reliable!)
        // 3. Will be re-published by background worker if needed
    }
}
```

### Transaction Safety Pattern

```csharp
public async Task CreateUserAsync(CreateUserRequest request)
{
    using var transaction = await _dbContext.Database.BeginTransactionAsync();

    try
    {
        // 1. Create user
        var user = new User(request.Email, request.Name);
        await _dbContext.Users.AddAsync(user);

        // 2. Save event to outbox (same transaction!)
        var domainEvent = new UserCreatedDomainEvent(user.Id, user.Email);
        await _eventBus.PublishAsync(domainEvent); // In Persistent mode, saves to outbox

        // 3. Commit transaction
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        // If commit fails, event is not saved
        // If commit succeeds, event is guaranteed to be published
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

### Query Outbox Events

```csharp
public class OutboxMonitoringService
{
    private readonly IEventStore _eventStore;

    public async Task<OutboxStats> GetStatsAsync()
    {
        var allEvents = await _eventStore.GetEventsByTimeRangeAsync(
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow);

        return new OutboxStats
        {
            Total = allEvents.Count,
            Published = allEvents.Count(e => e.IsPublished),
            Failed = allEvents.Count(e => e.IsFailed),
            Pending = allEvents.Count(e => !e.IsPublished && !e.IsFailed)
        };
    }

    public async Task<List<StoredEvent>> GetFailedEventsAsync()
    {
        var recentEvents = await _eventStore.GetEventsByTimeRangeAsync(
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow);

        return recentEvents.Where(e => e.IsFailed).ToList();
    }
}
```

---

## Configuration Options

### OutboxProcessorOptions

```csharp
public sealed class OutboxProcessorOptions
{
    /// <summary>
    /// How often to check for unpublished events (in seconds)
    /// Default: 10 seconds
    /// Recommendation: 5-30 seconds depending on load
    /// </summary>
    public int ProcessingIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Maximum number of events to process in one batch
    /// Default: 100
    /// Recommendation: 50-500 depending on event size
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Maximum number of retry attempts before marking event as permanently failed
    /// Default: 5
    /// Recommendation: 3-10 depending on criticality
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 5;
}
```

---

## Database Maintenance

### Clean Up Old Events

```sql
-- PostgreSQL: Delete published events older than 30 days
DELETE FROM "OutboxEvents"
WHERE "IsPublished" = TRUE
  AND "PublishedAt" < (NOW() - INTERVAL '30 days');
```

### Monitor Failed Events

```sql
-- Get failed events summary
SELECT
    "EventType",
    COUNT(*) as FailedCount,
    MAX("RetryCount") as MaxRetries,
    MAX("CreatedAt") as LatestFailure
FROM "OutboxEvents"
WHERE "IsFailed" = TRUE
GROUP BY "EventType"
ORDER BY FailedCount DESC;
```

### Reset Failed Events for Retry

```sql
-- CAUTION: Only reset events you've investigated
UPDATE "OutboxEvents"
SET
    "IsFailed" = FALSE,
    "RetryCount" = 0,
    "ErrorMessage" = NULL
WHERE "Id" = 'specific-event-id';
```

---

## Event Sourcing Support

The outbox table doubles as an event store for event sourcing:

```csharp
public class UserAggregateRepository
{
    private readonly IEventStore _eventStore;

    public async Task<User> RehydrateAsync(Guid userId)
    {
        // Get all events for this aggregate
        var events = await _eventStore.GetEventsByAggregateIdAsync(userId);

        // Replay events to rebuild state
        var user = new User();
        foreach (var storedEvent in events.OrderBy(e => e.OccurredAt))
        {
            var eventType = Type.GetType(storedEvent.EventType);
            var @event = JsonSerializer.Deserialize(storedEvent.EventData, eventType);
            user.Apply(@event);
        }

        return user;
    }
}
```

---

## Monitoring & Observability

### Health Check

```csharp
public class OutboxHealthCheck : IHealthCheck
{
    private readonly IEventStore _eventStore;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var unpublishedEvents = await _eventStore.GetUnpublishedEventsAsync(1000);

        var failedCount = unpublishedEvents.Count(e => e.IsFailed);
        var oldestUnpublished = unpublishedEvents
            .Where(e => !e.IsFailed)
            .OrderBy(e => e.CreatedAt)
            .FirstOrDefault();

        if (failedCount > 100)
        {
            return HealthCheckResult.Unhealthy($"Too many failed events: {failedCount}");
        }

        if (oldestUnpublished != null &&
            (DateTime.UtcNow - oldestUnpublished.CreatedAt).TotalMinutes > 30)
        {
            return HealthCheckResult.Degraded(
                $"Oldest unpublished event is {(DateTime.UtcNow - oldestUnpublished.CreatedAt).TotalMinutes:F0} minutes old");
        }

        return HealthCheckResult.Healthy($"{unpublishedEvents.Count} events pending");
    }
}
```

---

## Troubleshooting

### Problem: Events not being published

**Check**:
1. Is the OutboxProcessor running? Check logs for "Outbox Processor started"
2. Are there events in the OutboxEvents table with `IsPublished = false`?
3. Check for errors in logs: "Failed to publish event"
4. Verify database connection

### Problem: Too many failed events

**Check**:
1. Look at error messages in `ErrorMessage` column
2. Check if event handlers are throwing exceptions
3. Verify event types can be deserialized
4. Check if dependent services are down

### Problem: Duplicate event processing

**Expected**: Outbox pattern guarantees at-least-once delivery, duplicates are possible
**Solution**: Make event handlers idempotent:

```csharp
public class UserCreatedEventHandler
{
    public async Task HandleAsync(UserCreatedDomainEvent @event)
    {
        // Check if already processed (idempotency)
        if (await _cache.ExistsAsync($"event:{@event.EventId}"))
        {
            return; // Already processed, skip
        }

        // Process event
        await SendWelcomeEmail(@event.UserId);

        // Mark as processed
        await _cache.SetAsync($"event:{@event.EventId}", true, TimeSpan.FromDays(30));
    }
}
```

---

## Performance Considerations

### Hybrid Mode Overhead
- Events are processed twice (immediate + background)
- Slight performance impact (~5-10ms per event)
- Worth it for reliability in production

### Optimization Tips
1. **Batch size**: Increase for high-throughput scenarios (500+)
2. **Processing interval**: Decrease for low-latency requirements (5s)
3. **Indexes**: Ensure indexes on `IsPublished`, `IsFailed`, `CreatedAt`
4. **Partitioning**: Consider table partitioning for millions of events
5. **Cleanup**: Run cleanup jobs regularly to prevent table bloat

---

## Migration from InMemory to Persistent

```csharp
// BEFORE (In-Memory)
builder.Services.AddEventBusWithMediatR();

// AFTER (Persistent)
builder.Services.AddPersistentEventBusWithMediatRAndEfCore(
    mode: EventBusMode.Hybrid);
```

**Zero code changes required!** The IEventBus interface remains the same.

---

## Summary

| Mode | Speed | Reliability | Use Case |
|------|-------|-------------|----------|
| **InMemory** | ⚡ Fastest | ❌ None | Dev/Test |
| **Persistent** | 🐢 Slowest | ✅ Guaranteed | Critical events |
| **Hybrid** ⭐ | ⚡ Fast | ✅ Guaranteed | Production (recommended) |

**Recommendation**: Use **Hybrid mode** for production - you get immediate processing AND reliability!

---

## Need Help?

- Check logs for "EventBus" and "OutboxProcessor"
- Query OutboxEvents table for pending/failed events
- Review error messages in `ErrorMessage` column
- Ensure event handlers are idempotent
- Consider using health checks for monitoring
