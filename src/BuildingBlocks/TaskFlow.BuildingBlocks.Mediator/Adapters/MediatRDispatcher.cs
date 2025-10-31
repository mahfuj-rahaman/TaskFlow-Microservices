
using MediatR;
using TaskFlow.BuildingBlocks.Mediator.Abstractions;

namespace TaskFlow.BuildingBlocks.Mediator.Adapters;

/// <summary>
/// MediatR-specific implementation of the IDispatcher interface
/// </summary>
public sealed class MediatRDispatcher : IDispatcher
{
    private readonly IMediator _mediator;

    public MediatRDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        return _mediator.Send(command, cancellationToken);
    }

    public Task<TResponse> SendAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>
    {
        return _mediator.Send(command, cancellationToken);
    }

    public Task<TResponse> QueryAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>
    {
        return _mediator.Send(query, cancellationToken);
    }

    public Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : class
    {
        return _mediator.Publish(notification, cancellationToken);
    }
}
