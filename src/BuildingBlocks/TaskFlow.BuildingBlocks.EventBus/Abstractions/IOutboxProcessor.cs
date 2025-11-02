namespace TaskFlow.BuildingBlocks.EventBus.Abstractions;

/// <summary>
/// Background processor for Outbox pattern
/// Periodically checks for unpublished events and publishes them
/// Ensures at-least-once delivery guarantee
/// </summary>
public interface IOutboxProcessor
{
    /// <summary>
    /// Processes unpublished events from the outbox
    /// Returns number of events successfully processed
    /// </summary>
    Task<int> ProcessOutboxAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts continuous background processing
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops background processing gracefully
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);
}
