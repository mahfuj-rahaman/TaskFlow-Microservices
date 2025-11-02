namespace TaskFlow.BuildingBlocks.Messaging.Abstractions;

/// <summary>
/// Framework-agnostic message handler abstraction
/// Replaces MassTransit's IConsumer, NServiceBus's IHandleMessages, etc.
/// </summary>
/// <typeparam name="TMessage">The message type to handle</typeparam>
public interface IMessageHandler<in TMessage>
    where TMessage : class
{
    /// <summary>
    /// Handles the received message
    /// </summary>
    /// <param name="message">The message to handle</param>
    /// <param name="context">The message context containing metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task HandleAsync(TMessage message, IMessageContext context, CancellationToken cancellationToken = default);
}
