using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;

namespace TaskFlow.BuildingBlocks.EventBus.Adapters;

/// <summary>
/// Wolverine adapter implementation of IEventPublisher
/// Wraps Wolverine's IMessageBus for in-process event publishing
/// Wolverine is a high-performance alternative to MediatR with better throughput
/// </summary>
/// <remarks>
/// To use this adapter:
/// 1. Install Wolverine NuGet package: dotnet add package Wolverine
/// 2. Register Wolverine: services.AddWolverine()
/// 3. Register adapter: services.AddEventBus&lt;WolverineEventPublisher&gt;()
/// </remarks>
public sealed class WolverineEventPublisher : IEventPublisher
{
    // Note: Wolverine's IMessageBus interface - uncomment when Wolverine is added
    // private readonly IMessageBus _messageBus;

    // Placeholder for compilation - replace with actual IMessageBus when Wolverine is referenced
    private readonly object _messageBus;

    public WolverineEventPublisher(object messageBus) // Change to IMessageBus when Wolverine added
    {
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
    }

    /// <summary>
    /// Publishes a single domain event using Wolverine's message bus
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        // Uncomment when Wolverine is added:
        // await _messageBus.PublishAsync(@event);

        // Placeholder for compilation
        await Task.CompletedTask;
        throw new NotImplementedException(
            "Wolverine adapter requires Wolverine package. Install: dotnet add package Wolverine");
    }

    /// <summary>
    /// Publishes multiple domain events using Wolverine's message bus
    /// </summary>
    public async Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        if (events is null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        foreach (var @event in events)
        {
            await PublishAsync(@event, cancellationToken);
        }
    }
}

/*
 * PRODUCTION IMPLEMENTATION (when Wolverine is added):
 *
 * Install package: dotnet add package Wolverine
 *
 * using Wolverine;
 *
 * public sealed class WolverineEventPublisher : IEventPublisher
 * {
 *     private readonly IMessageBus _messageBus;
 *
 *     public WolverineEventPublisher(IMessageBus messageBus)
 *     {
 *         _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
 *     }
 *
 *     public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
 *         where TEvent : IDomainEvent
 *     {
 *         if (@event is null) throw new ArgumentNullException(nameof(@event));
 *         await _messageBus.PublishAsync(@event);
 *     }
 *
 *     public async Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
 *     {
 *         if (events is null) throw new ArgumentNullException(nameof(events));
 *
 *         foreach (var @event in events)
 *         {
 *             await _messageBus.PublishAsync(@event);
 *         }
 *     }
 * }
 */
