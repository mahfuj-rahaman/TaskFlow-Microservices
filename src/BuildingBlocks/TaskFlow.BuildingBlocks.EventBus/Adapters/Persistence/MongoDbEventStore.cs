using System.Text.Json;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;

namespace TaskFlow.BuildingBlocks.EventBus.Adapters.Persistence;

/// <summary>
/// MongoDB-based event store implementation for Outbox pattern
/// Works with MongoDB and DocumentDB
///
/// SETUP INSTRUCTIONS:
/// 1. Install NuGet package:
///    dotnet add package MongoDB.Driver
///
/// 2. Create indexes in MongoDB shell or C#:
///    db.outbox_events.createIndex({ "isPublished": 1, "isFailed": 1, "createdAt": 1 })
///    db.outbox_events.createIndex({ "aggregateId": 1 })
///    db.outbox_events.createIndex({ "eventType": 1 })
///    db.outbox_events.createIndex({ "occurredAt": 1 })
///
/// 3. Register in DI:
///    builder.Services.AddSingleton&lt;IMongoClient&gt;(sp =>
///        new MongoClient("mongodb://localhost:27017"));
///
///    builder.Services.AddSingleton&lt;IMongoDatabase&gt;(sp =>
///        sp.GetRequiredService&lt;IMongoClient&gt;().GetDatabase("taskflow"));
///
///    builder.Services.AddPersistentEventBus&lt;MediatREventPublisher, MongoDbEventStore&gt;(
///        mode: EventBusMode.Hybrid);
/// </summary>
public sealed class MongoDbEventStore : IEventStore
{
    private readonly IMongoCollection<MongoStoredEvent> _collection;
    private readonly ILogger<MongoDbEventStore> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public MongoDbEventStore(
        IMongoDatabase database,
        ILogger<MongoDbEventStore> logger)
    {
        if (database == null) throw new ArgumentNullException(nameof(database));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _collection = database.GetCollection<MongoStoredEvent>("outbox_events");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Create indexes if they don't exist
        CreateIndexesAsync().GetAwaiter().GetResult();
    }

    private async Task CreateIndexesAsync()
    {
        try
        {
            var indexKeys = Builders<MongoStoredEvent>.IndexKeys
                .Ascending(e => e.IsPublished)
                .Ascending(e => e.IsFailed)
                .Ascending(e => e.CreatedAt);

            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoStoredEvent>(indexKeys));

            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoStoredEvent>(
                    Builders<MongoStoredEvent>.IndexKeys.Ascending(e => e.AggregateId)));

            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoStoredEvent>(
                    Builders<MongoStoredEvent>.IndexKeys.Ascending(e => e.EventType)));

            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoStoredEvent>(
                    Builders<MongoStoredEvent>.IndexKeys.Ascending(e => e.OccurredAt)));

            _logger.LogDebug("MongoDB outbox indexes created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create MongoDB indexes (they may already exist)");
        }
    }

    public async Task SaveEventAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var storedEvent = CreateMongoStoredEvent(@event);

        try
        {
            await _collection.InsertOneAsync(storedEvent, cancellationToken: cancellationToken);

            _logger.LogInformation("Event {EventType} saved to MongoDB outbox with ID {EventId}",
                storedEvent.EventType, storedEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save event {EventType} to MongoDB outbox", storedEvent.EventType);
            throw;
        }
    }

    public async Task SaveEventsAsync(IEnumerable<object> events, CancellationToken cancellationToken = default)
    {
        var storedEvents = events.Select(CreateMongoStoredEvent).ToList();

        try
        {
            await _collection.InsertManyAsync(storedEvents, cancellationToken: cancellationToken);

            _logger.LogInformation("Saved {Count} events to MongoDB outbox", storedEvents.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save {Count} events to MongoDB outbox", storedEvents.Count);
            throw;
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> GetUnpublishedEventsAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<MongoStoredEvent>.Filter.And(
                Builders<MongoStoredEvent>.Filter.Eq(e => e.IsPublished, false),
                Builders<MongoStoredEvent>.Filter.Eq(e => e.IsFailed, false));

            var mongoEvents = await _collection
                .Find(filter)
                .SortBy(e => e.CreatedAt)
                .Limit(batchSize)
                .ToListAsync(cancellationToken);

            var events = mongoEvents.Select(MapToStoredEvent).ToList();

            _logger.LogDebug("Retrieved {Count} unpublished events from MongoDB outbox", events.Count);
            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve unpublished events from MongoDB outbox");
            throw;
        }
    }

    public async Task MarkAsPublishedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<MongoStoredEvent>.Filter.Eq(e => e.Id, eventId);
            var update = Builders<MongoStoredEvent>.Update
                .Set(e => e.IsPublished, true)
                .Set(e => e.PublishedAt, DateTime.UtcNow);

            await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

            _logger.LogDebug("Event {EventId} marked as published in MongoDB", eventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark event {EventId} as published in MongoDB", eventId);
            throw;
        }
    }

    public async Task MarkAsFailedAsync(Guid eventId, string errorMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<MongoStoredEvent>.Filter.Eq(e => e.Id, eventId);
            var update = Builders<MongoStoredEvent>.Update
                .Set(e => e.IsFailed, true)
                .Set(e => e.ErrorMessage, errorMessage)
                .Inc(e => e.RetryCount, 1);

            await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

            _logger.LogWarning("Event {EventId} marked as failed in MongoDB: {ErrorMessage}", eventId, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark event {EventId} as failed in MongoDB", eventId);
            throw;
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> GetEventsByAggregateIdAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<MongoStoredEvent>.Filter.Eq(e => e.AggregateId, aggregateId);

            var mongoEvents = await _collection
                .Find(filter)
                .SortBy(e => e.OccurredAt)
                .ToListAsync(cancellationToken);

            return mongoEvents.Select(MapToStoredEvent).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve events for aggregate {AggregateId} from MongoDB", aggregateId);
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
            var filter = Builders<MongoStoredEvent>.Filter.And(
                Builders<MongoStoredEvent>.Filter.Gte(e => e.OccurredAt, startTime),
                Builders<MongoStoredEvent>.Filter.Lte(e => e.OccurredAt, endTime));

            var mongoEvents = await _collection
                .Find(filter)
                .SortBy(e => e.OccurredAt)
                .ToListAsync(cancellationToken);

            return mongoEvents.Select(MapToStoredEvent).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve events for time range {Start} - {End} from MongoDB",
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
            var filter = Builders<MongoStoredEvent>.Filter.Eq(e => e.EventType, eventType);

            var mongoEvents = await _collection
                .Find(filter)
                .SortBy(e => e.OccurredAt)
                .ToListAsync(cancellationToken);

            return mongoEvents.Select(MapToStoredEvent).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve events of type {EventType} from MongoDB", eventType);
            throw;
        }
    }

    private MongoStoredEvent CreateMongoStoredEvent<TEvent>(TEvent @event) where TEvent : class
    {
        var eventType = @event.GetType();

        return new MongoStoredEvent
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

    private static StoredEvent MapToStoredEvent(MongoStoredEvent mongoEvent)
    {
        return new StoredEvent
        {
            Id = mongoEvent.Id,
            EventType = mongoEvent.EventType,
            EventData = mongoEvent.EventData,
            AggregateId = mongoEvent.AggregateId,
            AggregateType = mongoEvent.AggregateType,
            OccurredAt = mongoEvent.OccurredAt,
            CreatedAt = mongoEvent.CreatedAt,
            IsPublished = mongoEvent.IsPublished,
            PublishedAt = mongoEvent.PublishedAt,
            RetryCount = mongoEvent.RetryCount,
            ErrorMessage = mongoEvent.ErrorMessage,
            IsFailed = mongoEvent.IsFailed
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

/// <summary>
/// MongoDB-specific stored event model with BSON attributes
/// </summary>
internal sealed class MongoStoredEvent
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("eventType")]
    public string EventType { get; set; } = string.Empty;

    [BsonElement("eventData")]
    public string EventData { get; set; } = string.Empty;

    [BsonElement("aggregateId")]
    [BsonRepresentation(BsonType.String)]
    public Guid? AggregateId { get; set; }

    [BsonElement("aggregateType")]
    public string? AggregateType { get; set; }

    [BsonElement("occurredAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime OccurredAt { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    [BsonElement("isPublished")]
    public bool IsPublished { get; set; }

    [BsonElement("publishedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? PublishedAt { get; set; }

    [BsonElement("retryCount")]
    public int RetryCount { get; set; }

    [BsonElement("errorMessage")]
    public string? ErrorMessage { get; set; }

    [BsonElement("isFailed")]
    public bool IsFailed { get; set; }
}
