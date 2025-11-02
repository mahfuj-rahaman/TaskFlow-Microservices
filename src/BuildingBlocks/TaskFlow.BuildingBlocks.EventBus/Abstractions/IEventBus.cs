using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.BuildingBlocks.EventBus.Abstractions;

/// <summary>
/// Abstraction for publishing domain events to both in-process and distributed systems
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes a single domain event (in-process via MediatR and/or distributed via message bus)
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IDomainEvent;

    /// <summary>
    /// Publishes multiple domain events
    /// </summary>
    Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}
