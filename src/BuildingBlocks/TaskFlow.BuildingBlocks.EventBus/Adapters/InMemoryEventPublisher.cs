using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;

namespace TaskFlow.BuildingBlocks.EventBus.Adapters;

/// <summary>
/// In-memory event publisher implementation using manual dependency resolution
/// Perfect for testing, simple applications, or when you don't need MediatR/Wolverine/etc.
/// Automatically resolves and invokes all registered event handlers
/// </summary>
/// <remarks>
/// To use this adapter:
/// 1. Register event handlers: services.AddScoped&lt;IDomainEventHandler&lt;UserCreatedDomainEvent&gt;, UserCreatedEventHandler&gt;()
/// 2. Register adapter: services.AddEventBus&lt;InMemoryEventPublisher&gt;()
///
/// Example handler:
/// <code>
/// public class UserCreatedEventHandler : IDomainEventHandler&lt;UserCreatedDomainEvent&gt;
/// {
///     public async Task HandleAsync(UserCreatedDomainEvent @event, CancellationToken ct)
///     {
///         // Handle the event
///     }
/// }
/// </code>
/// </remarks>
public sealed class InMemoryEventPublisher : IEventPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InMemoryEventPublisher> _logger;

    public InMemoryEventPublisher(
        IServiceProvider serviceProvider,
        ILogger<InMemoryEventPublisher> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Publishes a single domain event to all registered handlers
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        _logger.LogDebug(
            "Publishing in-memory event {EventType} with ID {EventId}",
            typeof(TEvent).Name,
            @event.EventId);

        // Resolve all handlers for this event type
        var handlers = _serviceProvider.GetServices<IDomainEventHandler<TEvent>>();

        var handlersList = handlers.ToList();
        if (handlersList.Count == 0)
        {
            _logger.LogWarning(
                "No handlers registered for event {EventType}. Event will be ignored.",
                typeof(TEvent).Name);
            return;
        }

        _logger.LogDebug(
            "Found {HandlerCount} handler(s) for event {EventType}",
            handlersList.Count,
            typeof(TEvent).Name);

        // Invoke all handlers
        foreach (var handler in handlersList)
        {
            try
            {
                _logger.LogDebug(
                    "Invoking handler {HandlerType} for event {EventType}",
                    handler.GetType().Name,
                    typeof(TEvent).Name);

                await handler.HandleAsync(@event, cancellationToken);

                _logger.LogDebug(
                    "Successfully invoked handler {HandlerType} for event {EventType}",
                    handler.GetType().Name,
                    typeof(TEvent).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error invoking handler {HandlerType} for event {EventType}",
                    handler.GetType().Name,
                    typeof(TEvent).Name);

                // Re-throw to fail fast (or you can continue to invoke other handlers)
                throw;
            }
        }

        _logger.LogDebug(
            "Completed publishing event {EventType} to {HandlerCount} handler(s)",
            typeof(TEvent).Name,
            handlersList.Count);
    }

    /// <summary>
    /// Publishes multiple domain events to all registered handlers
    /// </summary>
    public async Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        if (events is null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        var eventsList = events.ToList();
        if (eventsList.Count == 0)
        {
            _logger.LogDebug("No events to publish");
            return;
        }

        _logger.LogDebug("Publishing {EventCount} in-memory events", eventsList.Count);

        foreach (var @event in eventsList)
        {
            // Use reflection to call the generic PublishAsync method
            var eventType = @event.GetType();
            var publishMethod = GetType()
                .GetMethod(nameof(PublishAsync), new[] { eventType, typeof(CancellationToken) });

            if (publishMethod != null)
            {
                var task = publishMethod.Invoke(this, new object[] { @event, cancellationToken }) as Task;
                if (task != null)
                {
                    await task;
                }
            }
            else
            {
                _logger.LogWarning(
                    "Could not find PublishAsync method for event type {EventType}",
                    eventType.Name);
            }
        }

        _logger.LogDebug("Completed publishing {EventCount} in-memory events", eventsList.Count);
    }
}
