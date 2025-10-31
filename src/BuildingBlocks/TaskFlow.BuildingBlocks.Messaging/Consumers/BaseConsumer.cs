using MassTransit;
using Microsoft.Extensions.Logging;
using TaskFlow.BuildingBlocks.Messaging.Contracts;

namespace TaskFlow.BuildingBlocks.Messaging.Consumers;

/// <summary>
/// Base class for message consumers with built-in logging and error handling
/// </summary>
/// <typeparam name="TMessage">The message type to consume</typeparam>
public abstract class BaseConsumer<TMessage> : IConsumer<TMessage>
    where TMessage : class, IMessage
{
    /// <summary>
    /// Logger instance for derived consumers
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// Initializes a new instance of the BaseConsumer class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    protected BaseConsumer(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Consumes a message with logging and error handling
    /// </summary>
    /// <param name="context">The consume context</param>
    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var message = context.Message;
        var messageType = typeof(TMessage).Name;

        Logger.LogInformation(
            "Consuming message {MessageType} with ID {MessageId} (Timestamp: {Timestamp})",
            messageType,
            message.MessageId,
            message.Timestamp);

        try
        {
            await ConsumeMessage(context);

            Logger.LogInformation(
                "Successfully consumed message {MessageType} with ID {MessageId}",
                messageType,
                message.MessageId);
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "Error consuming message {MessageType} with ID {MessageId}",
                messageType,
                message.MessageId);

            // Re-throw to allow MassTransit retry mechanism to handle
            throw;
        }
    }

    /// <summary>
    /// Abstract method to be implemented by derived consumers
    /// Contains the actual message processing logic
    /// </summary>
    /// <param name="context">The consume context</param>
    protected abstract Task ConsumeMessage(ConsumeContext<TMessage> context);
}
