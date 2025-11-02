using Microsoft.Extensions.Logging;
using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;

namespace TaskFlow.BuildingBlocks.EventBus;

/// <summary>
/// Implementation of EventBus that publishes domain events with multiple delivery guarantees:
///
/// 1. In-Memory Mode (default):
///    - Publishes events in-process via IEventPublisher
///    - Fast but events are lost on crash
///    - Good for: Non-critical events, development, testing
///
/// 2. Persistent Mode (Outbox Pattern):
///    - Saves events to persistent storage first (IEventStore)
///    - Background processor publishes events asynchronously
///    - Guarantees at-least-once delivery
///    - Good for: Critical events, production, distributed systems
///
/// 3. Hybrid Mode:
///    - Publishes immediately in-process AND saves to outbox
///    - Best of both worlds: immediate processing + guaranteed delivery
///    - Good for: Most production scenarios
///
/// Framework-agnostic - works with any event publisher and message bus
/// </summary>
public sealed class EventBus : IEventBus
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IEventStore? _eventStore;
    private readonly IMessagePublisher? _messagePublisher;
    private readonly IIntegrationEventMapper? _eventMapper;
    private readonly ILogger<EventBus> _logger;
    private readonly EventBusMode _mode;

    public EventBus(
        IEventPublisher eventPublisher,
        ILogger<EventBus> logger,
        IEventStore? eventStore = null,
        IMessagePublisher? messagePublisher = null,
        IIntegrationEventMapper? eventMapper = null,
        EventBusMode mode = EventBusMode.InMemory)
    {
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventStore = eventStore;
        _messagePublisher = messagePublisher;
        _eventMapper = eventMapper;
        _mode = mode;

        // Validate configuration
        if (mode != EventBusMode.InMemory && eventStore is null)
        {
            throw new InvalidOperationException(
                $"EventBusMode.{mode} requires IEventStore to be registered. " +
                "Please register IEventStore implementation or use EventBusMode.InMemory");
        }

        _logger.LogInformation("EventBus initialized in {Mode} mode", mode);
    }

    /// <summary>
    /// Publishes a single domain event based on configured mode:
    /// - InMemory: Publishes immediately (fast, no persistence)
    /// - Persistent: Saves to outbox only (background processor publishes)
    /// - Hybrid: Publishes immediately AND saves to outbox (best of both)
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IDomainEvent
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        try
        {
            _logger.LogInformation(
                "Publishing domain event {EventType} with ID {EventId} in {Mode} mode",
                @event.GetType().Name,
                @event.EventId,
                _mode);

            // Choose publishing strategy based on mode
            switch (_mode)
            {
                case EventBusMode.InMemory:
                    await PublishInMemoryAsync(@event, cancellationToken);
                    break;

                case EventBusMode.Persistent:
                    await PublishPersistentAsync(@event, cancellationToken);
                    break;

                case EventBusMode.Hybrid:
                    // Publish immediately AND save to outbox
                    await PublishInMemoryAsync(@event, cancellationToken);
                    await PublishPersistentAsync(@event, cancellationToken);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown EventBusMode: {_mode}");
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
    /// Publishes event in-memory (immediate, no persistence)
    /// </summary>
    private async Task PublishInMemoryAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : class, IDomainEvent
    {
        // 1. Publish in-process via IEventPublisher
        await _eventPublisher.PublishAsync(@event, cancellationToken);

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
    }

    /// <summary>
    /// Saves event to persistent outbox (background processor will publish)
    /// </summary>
    private async Task PublishPersistentAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : class, IDomainEvent
    {
        if (_eventStore is null)
        {
            throw new InvalidOperationException("EventStore is required for persistent publishing");
        }

        await _eventStore.SaveEventAsync(@event, cancellationToken);

        _logger.LogDebug(
            "Event {EventType} with ID {EventId} saved to outbox",
            @event.GetType().Name,
            @event.EventId);
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
