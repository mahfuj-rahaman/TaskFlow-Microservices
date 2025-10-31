namespace TaskFlow.BuildingBlocks.Mediator.Abstractions;

/// <summary>
/// Handler for commands without response
/// Framework-agnostic - works with MediatR, Wolverine, Brighter, or custom mediator
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Handles the command
    /// </summary>
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handler for commands with response
/// </summary>
/// <typeparam name="TCommand">The command type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Handles the command and returns a response
    /// </summary>
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
