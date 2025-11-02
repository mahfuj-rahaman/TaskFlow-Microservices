namespace TaskFlow.BuildingBlocks.Messaging.Abstractions;

/// <summary>
/// Framework-agnostic message context abstraction
/// Provides access to message metadata without coupling to specific framework types
/// </summary>
public interface IMessageContext
{
    /// <summary>
    /// Gets the unique message identifier
    /// </summary>
    Guid MessageId { get; }

    /// <summary>
    /// Gets the correlation identifier for tracking related messages
    /// </summary>
    Guid? CorrelationId { get; }

    /// <summary>
    /// Gets the conversation identifier for grouping messages in a conversation
    /// </summary>
    Guid? ConversationId { get; }

    /// <summary>
    /// Gets the time the message was sent
    /// </summary>
    DateTime? SentTime { get; }

    /// <summary>
    /// Gets custom headers associated with the message
    /// </summary>
    IReadOnlyDictionary<string, object> Headers { get; }

    /// <summary>
    /// Gets the source address where the message originated
    /// </summary>
    Uri? SourceAddress { get; }

    /// <summary>
    /// Gets the destination address where the message was delivered
    /// </summary>
    Uri? DestinationAddress { get; }

    /// <summary>
    /// Responds to the current message by sending a response
    /// </summary>
    Task RespondAsync<TResponse>(TResponse response, CancellationToken cancellationToken = default)
        where TResponse : class;
}
