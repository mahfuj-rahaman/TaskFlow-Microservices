namespace TaskFlow.BuildingBlocks.EventBus.Abstractions;

/// <summary>
/// Defines the event publishing mode for EventBus
/// </summary>
public enum EventBusMode
{
    /// <summary>
    /// In-memory publishing only (default)
    /// - Fast, immediate event delivery
    /// - Events are lost on application crash/restart
    /// - No persistence overhead
    /// - Good for: Development, testing, non-critical events
    /// </summary>
    InMemory = 0,

    /// <summary>
    /// Persistent publishing using Outbox pattern
    /// - Events saved to database first
    /// - Background processor publishes asynchronously
    /// - Guarantees at-least-once delivery
    /// - Events survive crashes and restarts
    /// - Small latency (published by background worker)
    /// - Good for: Critical events, production systems
    /// </summary>
    Persistent = 1,

    /// <summary>
    /// Hybrid mode: Immediate publishing + Outbox persistence
    /// - Publishes immediately in-memory for fast handling
    /// - ALSO saves to outbox for guaranteed delivery
    /// - Best of both worlds: speed + reliability
    /// - Slight overhead (double processing possible)
    /// - Good for: Most production scenarios
    /// </summary>
    Hybrid = 2
}
