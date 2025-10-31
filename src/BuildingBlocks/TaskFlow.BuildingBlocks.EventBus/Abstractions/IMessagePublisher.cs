namespace TaskFlow.BuildingBlocks.EventBus.Abstractions;

/// <summary>
/// Abstraction for publishing messages to a distributed message bus
/// This interface is framework-agnostic (works with MassTransit, NServiceBus, Rebus, CAP, etc.)
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message to the message bus
    /// </summary>
    /// <typeparam name="TMessage">The message type</typeparam>
    /// <param name="message">The message to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class;

    /// <summary>
    /// Publishes a message to a specific destination/topic
    /// </summary>
    /// <typeparam name="TMessage">The message type</typeparam>
    /// <param name="message">The message to publish</param>
    /// <param name="destination">The destination/topic name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<TMessage>(TMessage message, string destination, CancellationToken cancellationToken = default)
        where TMessage : class;

    /// <summary>
    /// Sends a command to a specific endpoint (point-to-point)
    /// </summary>
    /// <typeparam name="TCommand">The command type</typeparam>
    /// <param name="command">The command to send</param>
    /// <param name="endpoint">The endpoint address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendAsync<TCommand>(TCommand command, string endpoint, CancellationToken cancellationToken = default)
        where TCommand : class;
}
