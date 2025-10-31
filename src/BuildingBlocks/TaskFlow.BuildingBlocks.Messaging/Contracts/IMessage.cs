namespace TaskFlow.BuildingBlocks.Messaging.Contracts;

/// <summary>
/// Base contract for all messages (commands and events)
/// </summary>
public interface IMessage
{
    /// <summary>
    /// Unique identifier for the message (for idempotency and deduplication)
    /// </summary>
    Guid MessageId { get; }

    /// <summary>
    /// Timestamp when the message was created
    /// </summary>
    DateTime Timestamp { get; }
}
