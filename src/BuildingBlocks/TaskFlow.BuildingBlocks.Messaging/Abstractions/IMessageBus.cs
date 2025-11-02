namespace TaskFlow.BuildingBlocks.Messaging.Abstractions;

/// <summary>
/// Framework-agnostic message bus abstraction
/// Combines publishing and lifecycle management
/// </summary>
public interface IMessageBus : IMessagePublisher
{
    /// <summary>
    /// Starts the message bus and begins consuming messages
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the message bus and stops consuming messages
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets whether the bus is currently running
    /// </summary>
    bool IsRunning { get; }
}
