namespace TaskFlow.BuildingBlocks.Messaging.Abstractions;

/// <summary>
/// Framework-agnostic message publisher abstraction
/// Supports any message broker (MassTransit, NServiceBus, RabbitMQ, Azure Service Bus, etc.)
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message to the message broker
    /// </summary>
    /// <typeparam name="TMessage">The message type</typeparam>
    /// <param name="message">The message to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class;

    /// <summary>
    /// Sends a message to a specific endpoint/queue
    /// </summary>
    /// <typeparam name="TMessage">The message type</typeparam>
    /// <param name="destinationAddress">The destination endpoint URI or queue name</param>
    /// <param name="message">The message to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendAsync<TMessage>(Uri destinationAddress, TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class;

    /// <summary>
    /// Schedules a message to be published at a future time
    /// </summary>
    /// <typeparam name="TMessage">The message type</typeparam>
    /// <param name="message">The message to schedule</param>
    /// <param name="scheduledTime">When to publish the message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SchedulePublishAsync<TMessage>(TMessage message, DateTime scheduledTime, CancellationToken cancellationToken = default)
        where TMessage : class;
}
