using MediatR;
using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;

namespace TaskFlow.BuildingBlocks.EventBus.Adapters;

/// <summary>
/// MediatR adapter implementation of IEventPublisher
/// Wraps MediatR's IPublisher/IMediator for in-process event publishing
/// </summary>
public sealed class MediatREventPublisher : IEventPublisher
{
    private readonly IMediator _mediator;

    public MediatREventPublisher(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Publishes a single domain event using MediatR
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        await _mediator.Publish(@event, cancellationToken);
    }

    /// <summary>
    /// Publishes multiple domain events using MediatR
    /// </summary>
    public async Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        if (events is null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        foreach (var @event in events)
        {
            await _mediator.Publish(@event, cancellationToken);
        }
    }
}
