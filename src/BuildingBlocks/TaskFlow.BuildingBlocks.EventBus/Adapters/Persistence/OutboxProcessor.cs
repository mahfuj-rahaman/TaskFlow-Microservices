using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;

namespace TaskFlow.BuildingBlocks.EventBus.Adapters.Persistence;

/// <summary>
/// Background service that processes unpublished events from the outbox
/// Implements the Outbox pattern for guaranteed at-least-once delivery
///
/// How it works:
/// 1. Periodically checks for unpublished events in the outbox (default: every 10 seconds)
/// 2. Publishes events using the configured IEventPublisher
/// 3. Marks events as published on success
/// 4. Retries failed events with exponential backoff
/// 5. Marks events as permanently failed after max retry attempts
/// </summary>
public sealed class OutboxProcessor : BackgroundService, IOutboxProcessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly OutboxProcessorOptions _options;

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessor> logger,
        OutboxProcessorOptions? options = null)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new OutboxProcessorOptions();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor started. Processing interval: {Interval}s",
            _options.ProcessingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_options.ProcessingIntervalSeconds), stoppingToken);

                var processedCount = await ProcessOutboxAsync(stoppingToken);

                if (processedCount > 0)
                {
                    _logger.LogInformation("Processed {Count} events from outbox", processedCount);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown, don't log as error
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing outbox");
                // Continue processing after error
            }
        }

        _logger.LogInformation("Outbox Processor stopped");
    }

    public async Task<int> ProcessOutboxAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();

        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var unpublishedEvents = await eventStore.GetUnpublishedEventsAsync(
            _options.BatchSize,
            cancellationToken);

        if (!unpublishedEvents.Any())
        {
            return 0;
        }

        var successCount = 0;

        foreach (var storedEvent in unpublishedEvents)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                // Check if max retries exceeded
                if (storedEvent.RetryCount >= _options.MaxRetryAttempts)
                {
                    await eventStore.MarkAsFailedAsync(
                        storedEvent.Id,
                        $"Max retry attempts ({_options.MaxRetryAttempts}) exceeded",
                        cancellationToken);

                    _logger.LogError(
                        "Event {EventId} of type {EventType} permanently failed after {RetryCount} attempts",
                        storedEvent.Id, storedEvent.EventType, storedEvent.RetryCount);

                    continue;
                }

                // Deserialize and publish event
                var eventType = Type.GetType(storedEvent.EventType);
                if (eventType is null)
                {
                    await eventStore.MarkAsFailedAsync(
                        storedEvent.Id,
                        $"Event type '{storedEvent.EventType}' not found",
                        cancellationToken);

                    _logger.LogError("Event type {EventType} not found for event {EventId}",
                        storedEvent.EventType, storedEvent.Id);

                    continue;
                }

                var @event = JsonSerializer.Deserialize(storedEvent.EventData, eventType);
                if (@event is null)
                {
                    await eventStore.MarkAsFailedAsync(
                        storedEvent.Id,
                        "Failed to deserialize event data",
                        cancellationToken);

                    _logger.LogError("Failed to deserialize event {EventId} of type {EventType}",
                        storedEvent.Id, storedEvent.EventType);

                    continue;
                }

                // Publish the event
                await PublishEventAsync(eventPublisher, @event, cancellationToken);

                // Mark as published
                await eventStore.MarkAsPublishedAsync(storedEvent.Id, cancellationToken);

                successCount++;

                _logger.LogDebug("Successfully published event {EventId} of type {EventType}",
                    storedEvent.Id, storedEvent.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish event {EventId} of type {EventType}. Retry count: {RetryCount}",
                    storedEvent.Id, storedEvent.EventType, storedEvent.RetryCount);

                try
                {
                    // Don't mark as failed yet, just increment retry count
                    // This will be retried on next processing cycle
                    await eventStore.MarkAsFailedAsync(
                        storedEvent.Id,
                        ex.Message,
                        cancellationToken);
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "Failed to update event {EventId} retry status", storedEvent.Id);
                }
            }
        }

        return successCount;
    }

    private static async Task PublishEventAsync(
        IEventPublisher eventPublisher,
        object @event,
        CancellationToken cancellationToken)
    {
        // Use reflection to call the generic PublishAsync method
        var method = typeof(IEventPublisher)
            .GetMethod(nameof(IEventPublisher.PublishAsync))
            ?.MakeGenericMethod(@event.GetType());

        if (method is null)
        {
            throw new InvalidOperationException($"Could not find PublishAsync method for event type {@event.GetType().Name}");
        }

        var task = method.Invoke(eventPublisher, new[] { @event, cancellationToken }) as Task;
        if (task is not null)
        {
            await task;
        }
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Outbox Processor");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Outbox Processor");
        return base.StopAsync(cancellationToken);
    }
}

/// <summary>
/// Configuration options for the Outbox Processor
/// </summary>
public sealed class OutboxProcessorOptions
{
    /// <summary>
    /// How often to check for unpublished events (in seconds)
    /// Default: 10 seconds
    /// </summary>
    public int ProcessingIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Maximum number of events to process in one batch
    /// Default: 100
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Maximum number of retry attempts before marking event as permanently failed
    /// Default: 5
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 5;
}
