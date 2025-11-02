using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.BuildingBlocks.EventBus.Abstractions;

/// <summary>
/// Abstraction for publishing events in-process (within application boundary)
/// Framework-agnostic - works with MediatR, Wolverine, Brighter, or custom event publisher
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes a single domain event to in-process handlers
    /// </summary>
    /// <typeparam name="TEvent">The type of domain event</typeparam>
    /// <param name="event">The domain event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the async operation</returns>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;

    /// <summary>
    /// Publishes multiple domain events to in-process handlers
    /// </summary>
    /// <param name="events">The collection of domain events to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the async operation</returns>
    Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}
