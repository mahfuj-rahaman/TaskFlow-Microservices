using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.BuildingBlocks.EventBus.Abstractions;

/// <summary>
/// Defines a handler for domain events
/// Framework-agnostic - can be used with any event publisher (MediatR, Wolverine, InMemory, etc.)
/// </summary>
/// <typeparam name="TEvent">The type of domain event to handle</typeparam>
/// <remarks>
/// This interface is used by framework-agnostic event publishers like InMemoryEventPublisher.
/// For MediatR, use INotificationHandler&lt;TEvent&gt; instead.
/// For Wolverine, use convention-based handlers.
/// For Brighter, use IHandleMessagesAsync&lt;TEvent&gt;.
/// </remarks>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    /// <summary>
    /// Handles the domain event asynchronously
    /// </summary>
    /// <param name="event">The domain event to handle</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>A task representing the async operation</returns>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
