using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;

namespace TaskFlow.BuildingBlocks.EventBus.Adapters;

/// <summary>
/// NServiceBus adapter implementation of IEventPublisher
/// Wraps NServiceBus's IMessageSession for in-process event publishing
/// NServiceBus is an enterprise-grade service bus with advanced routing and saga support
/// </summary>
/// <remarks>
/// To use this adapter:
/// 1. Install NServiceBus NuGet package: dotnet add package NServiceBus
/// 2. Register NServiceBus: endpointConfiguration.Build()
/// 3. Register adapter: services.AddEventBus&lt;NServiceBusEventPublisher&gt;()
/// </remarks>
public sealed class NServiceBusEventPublisher : IEventPublisher
{
    // Note: NServiceBus's IMessageSession interface - uncomment when NServiceBus is added
    // private readonly IMessageSession _messageSession;

    // Placeholder for compilation - replace with actual IMessageSession when NServiceBus is referenced
    private readonly object _messageSession;

    public NServiceBusEventPublisher(object messageSession) // Change to IMessageSession when NServiceBus added
    {
        _messageSession = messageSession ?? throw new ArgumentNullException(nameof(messageSession));
    }

    /// <summary>
    /// Publishes a single domain event using NServiceBus's message session
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        // Uncomment when NServiceBus is added:
        // await _messageSession.Publish(@event, cancellationToken);

        // Placeholder for compilation
        await Task.CompletedTask;
        throw new NotImplementedException(
            "NServiceBus adapter requires NServiceBus package. Install: dotnet add package NServiceBus");
    }

    /// <summary>
    /// Publishes multiple domain events using NServiceBus's message session
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
 * PRODUCTION IMPLEMENTATION (when NServiceBus is added):
 *
 * Install package: dotnet add package NServiceBus
 *
 * using NServiceBus;
 *
 * public sealed class NServiceBusEventPublisher : IEventPublisher
 * {
 *     private readonly IMessageSession _messageSession;
 *
 *     public NServiceBusEventPublisher(IMessageSession messageSession)
 *     {
 *         _messageSession = messageSession ?? throw new ArgumentNullException(nameof(messageSession));
 *     }
 *
 *     public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
 *         where TEvent : IDomainEvent
 *     {
 *         if (@event is null) throw new ArgumentNullException(nameof(@event));
 *
 *         // NServiceBus Publish sends to all subscribers
 *         await _messageSession.Publish(@event, cancellationToken);
 *     }
 *
 *     public async Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
 *     {
 *         if (events is null) throw new ArgumentNullException(nameof(events));
 *
 *         foreach (var @event in events)
 *         {
 *             await _messageSession.Publish(@event, cancellationToken);
 *         }
 *     }
 * }
 */
