using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;

namespace TaskFlow.BuildingBlocks.EventBus.Adapters;

/// <summary>
/// Brighter (Paramore.Brighter) adapter implementation of IEventPublisher
/// Wraps Brighter's IAmACommandProcessor for in-process event publishing
/// Brighter implements the Service Activator and Command Processor patterns
/// </summary>
/// <remarks>
/// To use this adapter:
/// 1. Install Brighter NuGet package: dotnet add package Paramore.Brighter
/// 2. Register Brighter: services.AddBrighter()
/// 3. Register adapter: services.AddEventBus&lt;BrighterEventPublisher&gt;()
/// </remarks>
public sealed class BrighterEventPublisher : IEventPublisher
{
    // Note: Brighter's IAmACommandProcessor interface - uncomment when Brighter is added
    // private readonly IAmACommandProcessor _commandProcessor;

    // Placeholder for compilation - replace with actual IAmACommandProcessor when Brighter is referenced
    private readonly object _commandProcessor;

    public BrighterEventPublisher(object commandProcessor) // Change to IAmACommandProcessor when Brighter added
    {
        _commandProcessor = commandProcessor ?? throw new ArgumentNullException(nameof(commandProcessor));
    }

    /// <summary>
    /// Publishes a single domain event using Brighter's command processor
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        // Uncomment when Brighter is added:
        // await _commandProcessor.PublishAsync(@event, cancellationToken: cancellationToken);

        // Placeholder for compilation
        await Task.CompletedTask;
        throw new NotImplementedException(
            "Brighter adapter requires Paramore.Brighter package. Install: dotnet add package Paramore.Brighter");
    }

    /// <summary>
    /// Publishes multiple domain events using Brighter's command processor
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
 * PRODUCTION IMPLEMENTATION (when Brighter is added):
 *
 * Install package: dotnet add package Paramore.Brighter
 *
 * using Paramore.Brighter;
 *
 * public sealed class BrighterEventPublisher : IEventPublisher
 * {
 *     private readonly IAmACommandProcessor _commandProcessor;
 *
 *     public BrighterEventPublisher(IAmACommandProcessor commandProcessor)
 *     {
 *         _commandProcessor = commandProcessor ?? throw new ArgumentNullException(nameof(commandProcessor));
 *     }
 *
 *     public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
 *         where TEvent : IDomainEvent
 *     {
 *         if (@event is null) throw new ArgumentNullException(nameof(@event));
 *
 *         // Brighter uses Post for async publish
 *         await _commandProcessor.PublishAsync(@event, cancellationToken: cancellationToken);
 *     }
 *
 *     public async Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
 *     {
 *         if (events is null) throw new ArgumentNullException(nameof(events));
 *
 *         foreach (var @event in events)
 *         {
 *             await _commandProcessor.PublishAsync(@event, cancellationToken: cancellationToken);
 *         }
 *     }
 * }
 */
