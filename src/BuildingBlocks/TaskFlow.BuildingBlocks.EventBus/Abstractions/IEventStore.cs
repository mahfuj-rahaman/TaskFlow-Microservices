namespace TaskFlow.BuildingBlocks.EventBus.Abstractions;

/// <summary>
/// Framework-agnostic event store for persisting domain and integration events
/// Supports Outbox pattern, event sourcing, and audit trail
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Saves an event to persistent storage (Outbox pattern)
    /// Events are saved in the same transaction as domain changes
    /// </summary>
    Task SaveEventAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class;

    /// <summary>
    /// Saves multiple events atomically
    /// </summary>
    Task SaveEventsAsync(IEnumerable<object> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unpublished events for background processing (Outbox pattern)
    /// Used by background worker to publish events that are stored but not yet published
    /// </summary>
    Task<IReadOnlyList<StoredEvent>> GetUnpublishedEventsAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks event as published after successful delivery
    /// </summary>
    Task MarkAsPublishedAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks event as failed after max retry attempts
    /// </summary>
    Task MarkAsFailedAsync(Guid eventId, string errorMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all events for a specific aggregate (Event Sourcing)
    /// </summary>
    Task<IReadOnlyList<StoredEvent>> GetEventsByAggregateIdAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets events within a time range (Audit trail)
    /// </summary>
    Task<IReadOnlyList<StoredEvent>> GetEventsByTimeRangeAsync(
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets events by type (useful for replaying specific events)
    /// </summary>
    Task<IReadOnlyList<StoredEvent>> GetEventsByTypeAsync(
        string eventType,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a stored event with metadata
/// </summary>
public sealed class StoredEvent
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty; // JSON serialized event
    public Guid? AggregateId { get; set; }
    public string? AggregateType { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsFailed { get; set; }
}
