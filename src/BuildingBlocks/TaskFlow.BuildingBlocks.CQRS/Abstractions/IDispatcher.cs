namespace TaskFlow.BuildingBlocks.CQRS.Abstractions;

/// <summary>
/// Framework-agnostic dispatcher for commands and queries
/// Abstraction over MediatR, Wolverine, Brighter, or custom mediator implementations
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Sends a command without expecting a response
    /// </summary>
    /// <typeparam name="TCommand">The command type</typeparam>
    /// <param name="command">The command to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    /// <summary>
    /// Sends a command and returns a response
    /// </summary>
    /// <typeparam name="TCommand">The command type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <param name="command">The command to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<TResponse> SendAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>;

    /// <summary>
    /// Sends a query and returns a response
    /// </summary>
    /// <typeparam name="TQuery">The query type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <param name="query">The query to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<TResponse> QueryAsync<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>;

    /// <summary>
    /// Publishes a notification to multiple handlers
    /// </summary>
    /// <typeparam name="TNotification">The notification type</typeparam>
    /// <param name="notification">The notification to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : class;
}
