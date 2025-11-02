using MassTransit;
using Microsoft.Extensions.Logging;
using TaskFlow.BuildingBlocks.Messaging.Abstractions;

namespace TaskFlow.BuildingBlocks.Messaging.Adapters.MassTransit;

/// <summary>
/// Adapter that bridges MassTransit's IConsumer to our IMessageHandler
/// Allows framework-agnostic handlers to work with MassTransit
/// </summary>
/// <typeparam name="TMessage">The message type</typeparam>
public sealed class MassTransitConsumerAdapter<TMessage> : IConsumer<TMessage>
    where TMessage : class
{
    private readonly IMessageHandler<TMessage> _handler;
    private readonly ILogger<MassTransitConsumerAdapter<TMessage>> _logger;

    public MassTransitConsumerAdapter(
        IMessageHandler<TMessage> handler,
        ILogger<MassTransitConsumerAdapter<TMessage>> logger)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var messageType = typeof(TMessage).Name;

        _logger.LogInformation(
            "Consuming message {MessageType} with ID {MessageId}",
            messageType,
            context.MessageId);

        try
        {
            // Wrap MassTransit context in our framework-agnostic context
            var messageContext = new MassTransitMessageContext(context);

            // Call framework-agnostic handler
            await _handler.HandleAsync(context.Message, messageContext, context.CancellationToken);

            _logger.LogInformation(
                "Successfully consumed message {MessageType} with ID {MessageId}",
                messageType,
                context.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error consuming message {MessageType} with ID {MessageId}",
                messageType,
                context.MessageId);

            throw;
        }
    }
}
