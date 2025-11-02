using System.Data;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;

namespace TaskFlow.BuildingBlocks.EventBus.Adapters.Persistence;

/// <summary>
/// SQL-based event store implementation using raw ADO.NET
/// Works with PostgreSQL, SQL Server, MySQL (framework-agnostic)
/// For production: Replace with Dapper, EF Core, or your preferred data access library
/// </summary>
public sealed class SqlEventStore : IEventStore
{
    private readonly IDbConnection _connection;
    private readonly ILogger<SqlEventStore> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public SqlEventStore(
        IDbConnection connection,
        ILogger<SqlEventStore> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task SaveEventAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var storedEvent = CreateStoredEvent(@event);

        const string sql = @"
            INSERT INTO OutboxEvents (Id, EventType, EventData, AggregateId, AggregateType, OccurredAt, CreatedAt, IsPublished, RetryCount, IsFailed)
            VALUES (@Id, @EventType, @EventData, @AggregateId, @AggregateType, @OccurredAt, @CreatedAt, @IsPublished, @RetryCount, @IsFailed)";

        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = sql;

            AddParameter(command, "@Id", storedEvent.Id);
            AddParameter(command, "@EventType", storedEvent.EventType);
            AddParameter(command, "@EventData", storedEvent.EventData);
            AddParameter(command, "@AggregateId", storedEvent.AggregateId ?? (object)DBNull.Value);
            AddParameter(command, "@AggregateType", storedEvent.AggregateType ?? (object)DBNull.Value);
            AddParameter(command, "@OccurredAt", storedEvent.OccurredAt);
            AddParameter(command, "@CreatedAt", storedEvent.CreatedAt);
            AddParameter(command, "@IsPublished", storedEvent.IsPublished);
            AddParameter(command, "@RetryCount", storedEvent.RetryCount);
            AddParameter(command, "@IsFailed", storedEvent.IsFailed);

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            await Task.Run(() => command.ExecuteNonQuery(), cancellationToken);

            _logger.LogInformation("Event {EventType} saved to outbox with ID {EventId}",
                storedEvent.EventType, storedEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save event {EventType} to outbox", storedEvent.EventType);
            throw;
        }
    }

    public async Task SaveEventsAsync(IEnumerable<object> events, CancellationToken cancellationToken = default)
    {
        foreach (var @event in events)
        {
            await SaveEventAsync(@event, cancellationToken);
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> GetUnpublishedEventsAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, EventType, EventData, AggregateId, AggregateType, OccurredAt, CreatedAt,
                   IsPublished, PublishedAt, RetryCount, ErrorMessage, IsFailed
            FROM OutboxEvents
            WHERE IsPublished = 0 AND IsFailed = 0
            ORDER BY CreatedAt
            LIMIT @BatchSize";

        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@BatchSize", batchSize);

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            var events = new List<StoredEvent>();

            using var reader = await Task.Run(() => command.ExecuteReader(), cancellationToken);
            while (await Task.Run(() => reader.Read(), cancellationToken))
            {
                events.Add(MapStoredEvent(reader));
            }

            _logger.LogDebug("Retrieved {Count} unpublished events from outbox", events.Count);
            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve unpublished events from outbox");
            throw;
        }
    }

    public async Task MarkAsPublishedAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE OutboxEvents
            SET IsPublished = 1, PublishedAt = @PublishedAt
            WHERE Id = @EventId";

        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@EventId", eventId);
            AddParameter(command, "@PublishedAt", DateTime.UtcNow);

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            await Task.Run(() => command.ExecuteNonQuery(), cancellationToken);

            _logger.LogDebug("Event {EventId} marked as published", eventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark event {EventId} as published", eventId);
            throw;
        }
    }

    public async Task MarkAsFailedAsync(Guid eventId, string errorMessage, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE OutboxEvents
            SET IsFailed = 1, ErrorMessage = @ErrorMessage, RetryCount = RetryCount + 1
            WHERE Id = @EventId";

        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@EventId", eventId);
            AddParameter(command, "@ErrorMessage", errorMessage);

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            await Task.Run(() => command.ExecuteNonQuery(), cancellationToken);

            _logger.LogWarning("Event {EventId} marked as failed: {ErrorMessage}", eventId, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark event {EventId} as failed", eventId);
            throw;
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> GetEventsByAggregateIdAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, EventType, EventData, AggregateId, AggregateType, OccurredAt, CreatedAt,
                   IsPublished, PublishedAt, RetryCount, ErrorMessage, IsFailed
            FROM OutboxEvents
            WHERE AggregateId = @AggregateId
            ORDER BY OccurredAt";

        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@AggregateId", aggregateId);

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            var events = new List<StoredEvent>();

            using var reader = await Task.Run(() => command.ExecuteReader(), cancellationToken);
            while (await Task.Run(() => reader.Read(), cancellationToken))
            {
                events.Add(MapStoredEvent(reader));
            }

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve events for aggregate {AggregateId}", aggregateId);
            throw;
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> GetEventsByTimeRangeAsync(
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, EventType, EventData, AggregateId, AggregateType, OccurredAt, CreatedAt,
                   IsPublished, PublishedAt, RetryCount, ErrorMessage, IsFailed
            FROM OutboxEvents
            WHERE OccurredAt BETWEEN @StartTime AND @EndTime
            ORDER BY OccurredAt";

        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@StartTime", startTime);
            AddParameter(command, "@EndTime", endTime);

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            var events = new List<StoredEvent>();

            using var reader = await Task.Run(() => command.ExecuteReader(), cancellationToken);
            while (await Task.Run(() => reader.Read(), cancellationToken))
            {
                events.Add(MapStoredEvent(reader));
            }

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve events for time range {Start} - {End}", startTime, endTime);
            throw;
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> GetEventsByTypeAsync(
        string eventType,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, EventType, EventData, AggregateId, AggregateType, OccurredAt, CreatedAt,
                   IsPublished, PublishedAt, RetryCount, ErrorMessage, IsFailed
            FROM OutboxEvents
            WHERE EventType = @EventType
            ORDER BY OccurredAt";

        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = sql;
            AddParameter(command, "@EventType", eventType);

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            var events = new List<StoredEvent>();

            using var reader = await Task.Run(() => command.ExecuteReader(), cancellationToken);
            while (await Task.Run(() => reader.Read(), cancellationToken))
            {
                events.Add(MapStoredEvent(reader));
            }

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve events of type {EventType}", eventType);
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

    private static StoredEvent MapStoredEvent(IDataReader reader)
    {
        return new StoredEvent
        {
            Id = reader.GetGuid(0),
            EventType = reader.GetString(1),
            EventData = reader.GetString(2),
            AggregateId = reader.IsDBNull(3) ? null : reader.GetGuid(3),
            AggregateType = reader.IsDBNull(4) ? null : reader.GetString(4),
            OccurredAt = reader.GetDateTime(5),
            CreatedAt = reader.GetDateTime(6),
            IsPublished = reader.GetBoolean(7),
            PublishedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
            RetryCount = reader.GetInt32(9),
            ErrorMessage = reader.IsDBNull(10) ? null : reader.GetString(10),
            IsFailed = reader.GetBoolean(11)
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

    private static void AddParameter(IDbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
