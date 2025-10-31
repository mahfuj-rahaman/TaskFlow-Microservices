namespace TaskFlow.BuildingBlocks.EventBus.Abstractions;

/// <summary>
/// Represents an integration event for distributed systems communication
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Unique identifier for the event (for idempotency)
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// When the event occurred
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Event type identifier (e.g., "order.completed", "user.registered")
    /// </summary>
    string EventType { get; }
}
