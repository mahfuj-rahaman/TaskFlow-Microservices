using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;

namespace TaskFlow.BuildingBlocks.EventBus.Adapters.Persistence;

/// <summary>
/// Redis-based event store implementation for Outbox pattern
/// Ultra-fast but consider persistence settings carefully
///
/// IMPORTANT: Redis is volatile by default!
/// Enable Redis persistence (RDB or AOF) for production:
/// - RDB: Periodic snapshots (redis.conf: save 900 1)
/// - AOF: Append-only file (redis.conf: appendonly yes)
/// - Both: Maximum durability (recommended for events)
///
/// SETUP INSTRUCTIONS:
/// 1. Install NuGet package:
///    dotnet add package StackExchange.Redis
///
/// 2. Configure Redis for persistence (redis.conf):
///    # Enable AOF (append-only file) for durability
///    appendonly yes
///    appendfsync everysec  # fsync every second (good balance)
///
///    # Also enable RDB snapshots as backup
///    save 900 1      # save after 900 seconds if at least 1 key changed
///    save 300 10     # save after 300 seconds if at least 10 keys changed
///    save 60 10000   # save after 60 seconds if at least 10000 keys changed
///
/// 3. Register in DI:
///    builder.Services.AddSingleton&lt;IConnectionMultiplexer&gt;(sp =>
///        ConnectionMultiplexer.Connect("localhost:6379"));
///
///    builder.Services.AddPersistentEventBus&lt;MediatREventPublisher, RedisEventStore&gt;(
///        mode: EventBusMode.Hybrid);
///
/// USE CASES:
/// ✅ High-performance scenarios (millions of events/second)
/// ✅ Short-lived events (TTL-based expiration)
/// ✅ Read-heavy workloads (Redis is super fast)
/// ⚠️ Not recommended as primary store without AOF+RDB enabled
/// ✅ Great as secondary cache for recent events
/// </summary>
public sealed class RedisEventStore : IEventStore
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<RedisEventStore> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly TimeSpan? _eventTtl;

    // Redis key prefixes
    private const string EventPrefix = "outbox:event:";
    private const string UnpublishedSetKey = "outbox:unpublished";
    private const string FailedSetKey = "outbox:failed";
    private const string AggregatePrefix = "outbox:aggregate:";
    private const string EventTypePrefix = "outbox:type:";
    private const string TimeIndexKey = "outbox:timeline";

    public RedisEventStore(
        IConnectionMultiplexer redis,
        ILogger<RedisEventStore> logger,
        TimeSpan? eventTtl = null)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _database = _redis.GetDatabase();
        _eventTtl = eventTtl; // Optional TTL for auto-expiration (e.g., 30 days)

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        CheckRedisPersistence();
    }

    private void CheckRedisPersistence()
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var config = server.ConfigGet("appendonly");
            var aofEnabled = config.Any(c => c.Key == "appendonly" && c.Value == "yes");

            if (!aofEnabled)
            {
                _logger.LogWarning(
                    "⚠️ Redis AOF (append-only file) is NOT enabled! " +
                    "Events may be lost on Redis restart. " +
                    "Enable AOF in redis.conf: appendonly yes");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not check Redis persistence configuration");
        }
    }

    public async Task SaveEventAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var storedEvent = CreateStoredEvent(@event);
        var eventKey = $"{EventPrefix}{storedEvent.Id}";
        var eventJson = JsonSerializer.Serialize(storedEvent, _jsonOptions);

        try
        {
            var batch = _database.CreateBatch();

            // Store the event
            var setTask = batch.StringSetAsync(eventKey, eventJson, _eventTtl);

            // Add to unpublished set (sorted by created timestamp)
            var addUnpublishedTask = batch.SortedSetAddAsync(
                UnpublishedSetKey,
                storedEvent.Id.ToString(),
                storedEvent.CreatedAt.Ticks);

            // Add to timeline (for time-range queries)
            var addTimelineTask = batch.SortedSetAddAsync(
                TimeIndexKey,
                storedEvent.Id.ToString(),
                storedEvent.OccurredAt.Ticks);

            // Add to aggregate index if applicable
            Task? addAggregateTask = null;
            if (storedEvent.AggregateId.HasValue)
            {
                var aggregateKey = $"{AggregatePrefix}{storedEvent.AggregateId}";
                addAggregateTask = batch.SortedSetAddAsync(
                    aggregateKey,
                    storedEvent.Id.ToString(),
                    storedEvent.OccurredAt.Ticks);
            }

            // Add to event type index
            var eventTypeKey = $"{EventTypePrefix}{storedEvent.EventType}";
            var addTypeTask = batch.SortedSetAddAsync(
                eventTypeKey,
                storedEvent.Id.ToString(),
                storedEvent.OccurredAt.Ticks);

            batch.Execute();

            await setTask;
            await addUnpublishedTask;
            await addTimelineTask;
            if (addAggregateTask != null) await addAggregateTask;
            await addTypeTask;

            _logger.LogInformation("Event {EventType} saved to Redis outbox with ID {EventId}",
                storedEvent.EventType, storedEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save event {EventType} to Redis outbox", storedEvent.EventType);
            throw;
        }
    }

    public async Task SaveEventsAsync(IEnumerable<object> events, CancellationToken cancellationToken = default)
    {
        var batch = _database.CreateBatch();
        var tasks = new List<Task>();

        foreach (var @event in events)
        {
            var storedEvent = CreateStoredEvent(@event);
            var eventKey = $"{EventPrefix}{storedEvent.Id}";
            var eventJson = JsonSerializer.Serialize(storedEvent, _jsonOptions);

            tasks.Add(batch.StringSetAsync(eventKey, eventJson, _eventTtl));
            tasks.Add(batch.SortedSetAddAsync(UnpublishedSetKey, storedEvent.Id.ToString(), storedEvent.CreatedAt.Ticks));
            tasks.Add(batch.SortedSetAddAsync(TimeIndexKey, storedEvent.Id.ToString(), storedEvent.OccurredAt.Ticks));

            if (storedEvent.AggregateId.HasValue)
            {
                var aggregateKey = $"{AggregatePrefix}{storedEvent.AggregateId}";
                tasks.Add(batch.SortedSetAddAsync(aggregateKey, storedEvent.Id.ToString(), storedEvent.OccurredAt.Ticks));
            }

            var eventTypeKey = $"{EventTypePrefix}{storedEvent.EventType}";
            tasks.Add(batch.SortedSetAddAsync(eventTypeKey, storedEvent.Id.ToString(), storedEvent.OccurredAt.Ticks));
        }

        try
        {
            batch.Execute();
            await Task.WhenAll(tasks);
            _logger.LogInformation("Saved {Count} events to Redis outbox in batch", events.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save events to Redis outbox");
            throw;
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> GetUnpublishedEventsAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get event IDs from sorted set (oldest first)
            var eventIds = await _database.SortedSetRangeByRankAsync(
                UnpublishedSetKey,
                0,
                batchSize - 1);

            if (eventIds.Length == 0)
            {
                return Array.Empty<StoredEvent>();
            }

            var events = new List<StoredEvent>();

            foreach (var eventId in eventIds)
            {
                var eventKey = $"{EventPrefix}{eventId}";
                var eventJson = await _database.StringGetAsync(eventKey);

                if (eventJson.HasValue)
                {
                    var storedEvent = JsonSerializer.Deserialize<StoredEvent>(eventJson!, _jsonOptions);
                    if (storedEvent != null && !storedEvent.IsFailed)
                    {
                        events.Add(storedEvent);
                    }
                }
            }

            _logger.LogDebug("Retrieved {Count} unpublished events from Redis outbox", events.Count);
            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve unpublished events from Redis outbox");
            throw;
        }
    }

    public async Task MarkAsPublishedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            var eventKey = $"{EventPrefix}{eventId}";
            var eventJson = await _database.StringGetAsync(eventKey);

            if (!eventJson.HasValue)
            {
                _logger.LogWarning("Event {EventId} not found in Redis outbox", eventId);
                return;
            }

            var storedEvent = JsonSerializer.Deserialize<StoredEvent>(eventJson!, _jsonOptions);
            if (storedEvent == null) return;

            storedEvent.IsPublished = true;
            storedEvent.PublishedAt = DateTime.UtcNow;

            var batch = _database.CreateBatch();

            // Update event
            var updateTask = batch.StringSetAsync(eventKey, JsonSerializer.Serialize(storedEvent, _jsonOptions), _eventTtl);

            // Remove from unpublished set
            var removeTask = batch.SortedSetRemoveAsync(UnpublishedSetKey, eventId.ToString());

            batch.Execute();
            await updateTask;
            await removeTask;

            _logger.LogDebug("Event {EventId} marked as published in Redis", eventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark event {EventId} as published in Redis", eventId);
            throw;
        }
    }

    public async Task MarkAsFailedAsync(Guid eventId, string errorMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            var eventKey = $"{EventPrefix}{eventId}";
            var eventJson = await _database.StringGetAsync(eventKey);

            if (!eventJson.HasValue)
            {
                _logger.LogWarning("Event {EventId} not found in Redis outbox", eventId);
                return;
            }

            var storedEvent = JsonSerializer.Deserialize<StoredEvent>(eventJson!, _jsonOptions);
            if (storedEvent == null) return;

            storedEvent.IsFailed = true;
            storedEvent.ErrorMessage = errorMessage;
            storedEvent.RetryCount++;

            var batch = _database.CreateBatch();

            // Update event
            var updateTask = batch.StringSetAsync(eventKey, JsonSerializer.Serialize(storedEvent, _jsonOptions), _eventTtl);

            // Move to failed set
            var addFailedTask = batch.SortedSetAddAsync(FailedSetKey, eventId.ToString(), DateTime.UtcNow.Ticks);

            // Remove from unpublished set
            var removeTask = batch.SortedSetRemoveAsync(UnpublishedSetKey, eventId.ToString());

            batch.Execute();
            await updateTask;
            await addFailedTask;
            await removeTask;

            _logger.LogWarning("Event {EventId} marked as failed in Redis: {ErrorMessage}", eventId, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark event {EventId} as failed in Redis", eventId);
            throw;
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> GetEventsByAggregateIdAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var aggregateKey = $"{AggregatePrefix}{aggregateId}";
            var eventIds = await _database.SortedSetRangeByRankAsync(aggregateKey);

            var events = new List<StoredEvent>();

            foreach (var eventId in eventIds)
            {
                var eventKey = $"{EventPrefix}{eventId}";
                var eventJson = await _database.StringGetAsync(eventKey);

                if (eventJson.HasValue)
                {
                    var storedEvent = JsonSerializer.Deserialize<StoredEvent>(eventJson!, _jsonOptions);
                    if (storedEvent != null)
                    {
                        events.Add(storedEvent);
                    }
                }
            }

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve events for aggregate {AggregateId} from Redis", aggregateId);
            throw;
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> GetEventsByTimeRangeAsync(
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var eventIds = await _database.SortedSetRangeByScoreAsync(
                TimeIndexKey,
                startTime.Ticks,
                endTime.Ticks);

            var events = new List<StoredEvent>();

            foreach (var eventId in eventIds)
            {
                var eventKey = $"{EventPrefix}{eventId}";
                var eventJson = await _database.StringGetAsync(eventKey);

                if (eventJson.HasValue)
                {
                    var storedEvent = JsonSerializer.Deserialize<StoredEvent>(eventJson!, _jsonOptions);
                    if (storedEvent != null)
                    {
                        events.Add(storedEvent);
                    }
                }
            }

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve events for time range {Start} - {End} from Redis",
                startTime, endTime);
            throw;
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> GetEventsByTypeAsync(
        string eventType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var eventTypeKey = $"{EventTypePrefix}{eventType}";
            var eventIds = await _database.SortedSetRangeByRankAsync(eventTypeKey);

            var events = new List<StoredEvent>();

            foreach (var eventId in eventIds)
            {
                var eventKey = $"{EventPrefix}{eventId}";
                var eventJson = await _database.StringGetAsync(eventKey);

                if (eventJson.HasValue)
                {
                    var storedEvent = JsonSerializer.Deserialize<StoredEvent>(eventJson!, _jsonOptions);
                    if (storedEvent != null)
                    {
                        events.Add(storedEvent);
                    }
                }
            }

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve events of type {EventType} from Redis", eventType);
            throw;
        }
    }

    private StoredEvent CreateStoredEvent<TEvent>(TEvent @event) where TEvent : class
    {
        var eventType = @event.GetType();

        return new StoredEvent
        {
            Id = Guid.NewGuid(),
            EventType = eventType.FullName ?? eventType.Name,
            EventData = JsonSerializer.Serialize(@event, _jsonOptions),
            AggregateId = TryGetAggregateId(@event),
            AggregateType = TryGetAggregateType(@event),
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsPublished = false,
            RetryCount = 0,
            IsFailed = false
        };
    }

    private static Guid? TryGetAggregateId(object @event)
    {
        var property = @event.GetType().GetProperty("AggregateId");
        if (property?.PropertyType == typeof(Guid))
        {
            return (Guid?)property.GetValue(@event);
        }
        return null;
    }

    private static string? TryGetAggregateType(object @event)
    {
        var property = @event.GetType().GetProperty("AggregateType");
        if (property?.PropertyType == typeof(string))
        {
            return property.GetValue(@event) as string;
        }
        return null;
    }
}
