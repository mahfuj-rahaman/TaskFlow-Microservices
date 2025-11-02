using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;

namespace TaskFlow.BuildingBlocks.EventBus.Adapters.Persistence;

/// <summary>
/// Entity Framework Core-based event store implementation
/// Works with any EF Core provider (PostgreSQL, SQL Server, MySQL, SQLite, etc.)
///
/// SETUP INSTRUCTIONS:
/// 1. Add this DbSet to your DbContext:
///    public DbSet&lt;StoredEvent&gt; OutboxEvents { get; set; }
///
/// 2. Configure the entity in OnModelCreating:
///    modelBuilder.Entity&lt;StoredEvent&gt;(entity =>
///    {
///        entity.ToTable("OutboxEvents");
///        entity.HasKey(e => e.Id);
///        entity.HasIndex(e => new { e.IsPublished, e.IsFailed, e.CreatedAt });
///        entity.HasIndex(e => e.AggregateId);
///        entity.Property(e => e.EventType).HasMaxLength(500).IsRequired();
///        entity.Property(e => e.EventData).IsRequired();
///    });
///
/// 3. Create migration:
///    dotnet ef migrations add AddOutboxEvents
///
/// 4. Update database:
///    dotnet ef database update
/// </summary>
public sealed class EfCoreEventStore : IEventStore
{
    private readonly DbContext _dbContext;
    private readonly ILogger<EfCoreEventStore> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public EfCoreEventStore(
        DbContext dbContext,
        ILogger<EfCoreEventStore> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
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

        try
        {
            _dbContext.Set<StoredEvent>().Add(storedEvent);
            await _dbContext.SaveChangesAsync(cancellationToken);

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
        var storedEvents = events.Select(e => CreateStoredEvent(e)).ToList();

        try
        {
            _dbContext.Set<StoredEvent>().AddRange(storedEvents);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Saved {Count} events to outbox", storedEvents.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save {Count} events to outbox", storedEvents.Count);
            throw;
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> GetUnpublishedEventsAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var events = await _dbContext.Set<StoredEvent>()
                .Where(e => !e.IsPublished && !e.IsFailed)
                .OrderBy(e => e.CreatedAt)
                .Take(batchSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

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
        try
        {
            var storedEvent = await _dbContext.Set<StoredEvent>()
                .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

            if (storedEvent is null)
            {
                _logger.LogWarning("Event {EventId} not found in outbox", eventId);
                return;
            }

            storedEvent.IsPublished = true;
            storedEvent.PublishedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

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
        try
        {
            var storedEvent = await _dbContext.Set<StoredEvent>()
                .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

            if (storedEvent is null)
            {
                _logger.LogWarning("Event {EventId} not found in outbox", eventId);
                return;
            }

            storedEvent.IsFailed = true;
            storedEvent.ErrorMessage = errorMessage;
            storedEvent.RetryCount++;

            await _dbContext.SaveChangesAsync(cancellationToken);

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
        try
        {
            var events = await _dbContext.Set<StoredEvent>()
                .Where(e => e.AggregateId == aggregateId)
                .OrderBy(e => e.OccurredAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

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
        try
        {
            var events = await _dbContext.Set<StoredEvent>()
                .Where(e => e.OccurredAt >= startTime && e.OccurredAt <= endTime)
                .OrderBy(e => e.OccurredAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

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
        try
        {
            var events = await _dbContext.Set<StoredEvent>()
                .Where(e => e.EventType == eventType)
                .OrderBy(e => e.OccurredAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

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
