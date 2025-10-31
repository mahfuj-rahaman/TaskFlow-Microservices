using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;

namespace TaskFlow.BuildingBlocks.EventBus;

/// <summary>
/// Implementation of EventBus that publishes domain events both in-process (MediatR)
/// and to distributed message bus (framework-agnostic via IMessagePublisher)
/// </summary>
public sealed class EventBus : IEventBus
{
    private readonly IMediator _mediator;
    private readonly IMessagePublisher? _messagePublisher;
    private readonly IIntegrationEventMapper? _eventMapper;
    private readonly ILogger<EventBus> _logger;

    public EventBus(
        IMediator mediator,
        ILogger<EventBus> logger,
        IMessagePublisher? messagePublisher = null,
        IIntegrationEventMapper? eventMapper = null)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messagePublisher = messagePublisher;
        _eventMapper = eventMapper;
    }

    /// <summary>
    /// Publishes a single domain event (in-process via MediatR and/or distributed via message bus)
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        try
        {
            _logger.LogInformation(
                "Publishing domain event {EventType} with ID {EventId}",
                @event.GetType().Name,
                @event.EventId);

            // 1. Publish in-process via MediatR
            await _mediator.Publish(@event, cancellationToken);

            // 2. Optionally publish to message bus as integration event
            if (_messagePublisher is not null && _eventMapper is not null)
            {
                var integrationEvent = _eventMapper.Map(@event);
                if (integrationEvent is not null)
                {
                    _logger.LogInformation(
                        "Publishing integration event {EventType} with ID {EventId} to message bus",
                        integrationEvent.EventType,
                        integrationEvent.EventId);

                    await _messagePublisher.PublishAsync(integrationEvent, cancellationToken);
                }
            }

            _logger.LogInformation(
                "Successfully published domain event {EventType} with ID {EventId}",
                @event.GetType().Name,
                @event.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error publishing domain event {EventType} with ID {EventId}",
                @event.GetType().Name,
                @event.EventId);

            throw;
        }
    }

    /// <summary>
    /// Publishes multiple domain events
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
            _logger.LogDebug("No domain events to publish");
            return;
        }

        _logger.LogInformation("Publishing {Count} domain events", eventsList.Count);

        foreach (var @event in eventsList)
        {
            await PublishAsync(@event, cancellationToken);
        }

        _logger.LogInformation("Successfully published {Count} domain events", eventsList.Count);
    }
}
