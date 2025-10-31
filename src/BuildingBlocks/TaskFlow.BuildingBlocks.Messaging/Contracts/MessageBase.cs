namespace TaskFlow.BuildingBlocks.Messaging.Contracts;

/// <summary>
/// Base class for all messages (commands and events)
/// Provides default implementation of IMessage
/// </summary>
public abstract record MessageBase : IMessage
{
    /// <summary>
    /// Unique identifier for the message (for idempotency and deduplication)
    /// </summary>
    public Guid MessageId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the message was created
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
