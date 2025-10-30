using MediatR;

namespace TaskFlow.BuildingBlocks.Common.Domain;

/// <summary>
/// Marker interface for domain events
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// When the event occurred
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Unique identifier for this event
    /// </summary>
    Guid EventId { get; }
}
