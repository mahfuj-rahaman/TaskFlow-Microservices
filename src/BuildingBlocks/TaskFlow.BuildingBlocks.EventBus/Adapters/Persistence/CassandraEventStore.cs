using System.Text.Json;
using Cassandra;
using Cassandra.Mapping;
using Microsoft.Extensions.Logging;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;

namespace TaskFlow.BuildingBlocks.EventBus.Adapters.Persistence;

/// <summary>
/// Cassandra-based event store implementation for Outbox pattern
/// Works with Apache Cassandra and ScyllaDB
///
/// SETUP INSTRUCTIONS:
/// 1. Install NuGet package:
///    dotnet add package CassandraCSharpDriver
///
/// 2. Create keyspace and table:
///    CREATE KEYSPACE IF NOT EXISTS taskflow
///    WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 3};
///
///    USE taskflow;
///
///    CREATE TABLE IF NOT EXISTS outbox_events (
///        id uuid PRIMARY KEY,
///        event_type text,
///        event_data text,
///        aggregate_id uuid,
///        aggregate_type text,
///        occurred_at timestamp,
///        created_at timestamp,
///        is_published boolean,
///        published_at timestamp,
///        retry_count int,
///        error_message text,
///        is_failed boolean
///    );
///
///    -- Index for outbox processor query
///    CREATE INDEX IF NOT EXISTS idx_outbox_unpublished
///    ON outbox_events (is_published);
///
///    CREATE INDEX IF NOT EXISTS idx_outbox_failed
///    ON outbox_events (is_failed);
///
///    CREATE INDEX IF NOT EXISTS idx_outbox_aggregate
///    ON outbox_events (aggregate_id);
///
/// 3. Register in DI:
///    builder.Services.AddSingleton&lt;ICluster&gt;(sp =>
///        Cluster.Builder()
///            .AddContactPoint("localhost")
///            .WithPort(9042)
///            .Build());
///
///    builder.Services.AddSingleton&lt;ISession&gt;(sp =>
///        sp.GetRequiredService&lt;ICluster&gt;().Connect("taskflow"));
///
///    builder.Services.AddPersistentEventBus&lt;MediatREventPublisher, CassandraEventStore&gt;(
///        mode: EventBusMode.Hybrid);
/// </summary>
public sealed class CassandraEventStore : IEventStore
{
    private readonly ISession _session;
    private readonly IMapper _mapper;
    private readonly ILogger<CassandraEventStore> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    // CQL Statements (prepared for performance)
    private readonly PreparedStatement _insertStatement;
    private readonly PreparedStatement _selectUnpublishedStatement;
    private readonly PreparedStatement _markPublishedStatement;
    private readonly PreparedStatement _markFailedStatement;
    private readonly PreparedStatement _selectByAggregateStatement;
    private readonly PreparedStatement _selectByEventTypeStatement;

    public CassandraEventStore(
        ISession session,
        ILogger<CassandraEventStore> logger)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _mapper = new Mapper(_session);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Prepare CQL statements for better performance
        _insertStatement = _session.Prepare(
            "INSERT INTO outbox_events (id, event_type, event_data, aggregate_id, aggregate_type, " +
            "occurred_at, created_at, is_published, published_at, retry_count, error_message, is_failed) " +
            "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");

        _selectUnpublishedStatement = _session.Prepare(
            "SELECT * FROM outbox_events WHERE is_published = false AND is_failed = false LIMIT ? ALLOW FILTERING");

        _markPublishedStatement = _session.Prepare(
            "UPDATE outbox_events SET is_published = true, published_at = ? WHERE id = ?");

        _markFailedStatement = _session.Prepare(
            "UPDATE outbox_events SET is_failed = true, error_message = ?, retry_count = retry_count + 1 WHERE id = ?");

        _selectByAggregateStatement = _session.Prepare(
            "SELECT * FROM outbox_events WHERE aggregate_id = ? ALLOW FILTERING");

        _selectByEventTypeStatement = _session.Prepare(
            "SELECT * FROM outbox_events WHERE event_type = ? ALLOW FILTERING");
    }

    public async Task SaveEventAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var storedEvent = CreateStoredEvent(@event);

        try
        {
            var boundStatement = _insertStatement.Bind(
                storedEvent.Id,
                storedEvent.EventType,
                storedEvent.EventData,
                storedEvent.AggregateId,
                storedEvent.AggregateType,
                storedEvent.OccurredAt,
                storedEvent.CreatedAt,
                storedEvent.IsPublished,
                storedEvent.PublishedAt,
                storedEvent.RetryCount,
                storedEvent.ErrorMessage,
                storedEvent.IsFailed);

            await _session.ExecuteAsync(boundStatement);

            _logger.LogInformation("Event {EventType} saved to Cassandra outbox with ID {EventId}",
                storedEvent.EventType, storedEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save event {EventType} to Cassandra outbox", storedEvent.EventType);
            throw;
        }
    }

    public async Task SaveEventsAsync(IEnumerable<object> events, CancellationToken cancellationToken = default)
    {
        // Cassandra supports batch operations for better performance
        var batch = new BatchStatement();

        foreach (var @event in events)
        {
            var storedEvent = CreateStoredEvent(@event);

            var boundStatement = _insertStatement.Bind(
                storedEvent.Id,
                storedEvent.EventType,
                storedEvent.EventData,
                storedEvent.AggregateId,
                storedEvent.AggregateType,
                storedEvent.OccurredAt,
                storedEvent.CreatedAt,
                storedEvent.IsPublished,
                storedEvent.PublishedAt,
                storedEvent.RetryCount,
                storedEvent.ErrorMessage,
                storedEvent.IsFailed);

            batch.Add(boundStatement);
        }

        try
        {
            await _session.ExecuteAsync(batch);
            _logger.LogInformation("Saved {Count} events to Cassandra outbox in batch", batch.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save {Count} events to Cassandra outbox", batch.Count);
            throw;
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> GetUnpublishedEventsAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var boundStatement = _selectUnpublishedStatement.Bind(batchSize);
            var rowSet = await _session.ExecuteAsync(boundStatement);

            var events = new List<StoredEvent>();
            foreach (var row in rowSet)
            {
                events.Add(MapStoredEvent(row));
            }

            _logger.LogDebug("Retrieved {Count} unpublished events from Cassandra outbox", events.Count);
            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve unpublished events from Cassandra outbox");
            throw;
        }
    }

    public async Task MarkAsPublishedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        try
        {
            var boundStatement = _markPublishedStatement.Bind(DateTime.UtcNow, eventId);
            await _session.ExecuteAsync(boundStatement);

            _logger.LogDebug("Event {EventId} marked as published in Cassandra", eventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark event {EventId} as published in Cassandra", eventId);
            throw;
        }
    }

    public async Task MarkAsFailedAsync(Guid eventId, string errorMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            var boundStatement = _markFailedStatement.Bind(errorMessage, eventId);
            await _session.ExecuteAsync(boundStatement);

            _logger.LogWarning("Event {EventId} marked as failed in Cassandra: {ErrorMessage}", eventId, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark event {EventId} as failed in Cassandra", eventId);
            throw;
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> GetEventsByAggregateIdAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var boundStatement = _selectByAggregateStatement.Bind(aggregateId);
            var rowSet = await _session.ExecuteAsync(boundStatement);

            var events = new List<StoredEvent>();
            foreach (var row in rowSet)
            {
                events.Add(MapStoredEvent(row));
            }

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve events for aggregate {AggregateId} from Cassandra", aggregateId);
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
            // Note: Cassandra doesn't efficiently support range queries without proper partitioning
            // For production, consider using a time-based partition key or a separate time-series table
            var cql = "SELECT * FROM outbox_events WHERE occurred_at >= ? AND occurred_at <= ? ALLOW FILTERING";
            var statement = await _session.PrepareAsync(cql);
            var boundStatement = statement.Bind(startTime, endTime);

            var rowSet = await _session.ExecuteAsync(boundStatement);

            var events = new List<StoredEvent>();
            foreach (var row in rowSet)
            {
                events.Add(MapStoredEvent(row));
            }

            _logger.LogWarning(
                "Time range query executed with ALLOW FILTERING. " +
                "Consider redesigning schema with time-based partition key for production use.");

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve events for time range {Start} - {End} from Cassandra",
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
            var boundStatement = _selectByEventTypeStatement.Bind(eventType);
            var rowSet = await _session.ExecuteAsync(boundStatement);

            var events = new List<StoredEvent>();
            foreach (var row in rowSet)
            {
                events.Add(MapStoredEvent(row));
            }

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve events of type {EventType} from Cassandra", eventType);
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

    private static StoredEvent MapStoredEvent(Row row)
    {
        return new StoredEvent
        {
            Id = row.GetValue<Guid>("id"),
            EventType = row.GetValue<string>("event_type"),
            EventData = row.GetValue<string>("event_data"),
            AggregateId = row.IsNull("aggregate_id") ? null : row.GetValue<Guid>("aggregate_id"),
            AggregateType = row.IsNull("aggregate_type") ? null : row.GetValue<string>("aggregate_type"),
            OccurredAt = row.GetValue<DateTime>("occurred_at"),
            CreatedAt = row.GetValue<DateTime>("created_at"),
            IsPublished = row.GetValue<bool>("is_published"),
            PublishedAt = row.IsNull("published_at") ? null : row.GetValue<DateTime>("published_at"),
            RetryCount = row.GetValue<int>("retry_count"),
            ErrorMessage = row.IsNull("error_message") ? null : row.GetValue<string>("error_message"),
            IsFailed = row.GetValue<bool>("is_failed")
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
