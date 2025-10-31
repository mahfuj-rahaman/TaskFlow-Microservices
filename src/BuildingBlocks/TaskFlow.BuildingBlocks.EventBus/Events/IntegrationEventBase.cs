using TaskFlow.BuildingBlocks.EventBus.Abstractions;

namespace TaskFlow.BuildingBlocks.EventBus.Events;

/// <summary>
/// Base class for integration events
/// </summary>
public abstract record IntegrationEventBase : IIntegrationEvent
{
    /// <summary>
    /// Unique identifier for the event (for idempotency)
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// When the event occurred
    /// </summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Event type identifier (must be overridden by derived classes)
    /// </summary>
    public abstract string EventType { get; }
}
