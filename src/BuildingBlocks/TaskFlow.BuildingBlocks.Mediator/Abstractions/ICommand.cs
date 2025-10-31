namespace TaskFlow.BuildingBlocks.Mediator.Abstractions;

/// <summary>
/// Marker interface for commands (write operations)
/// Framework-agnostic - works with MediatR, Wolverine, Brighter, or custom mediator
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Marker interface for commands with a response
/// </summary>
/// <typeparam name="TResponse">The response type</typeparam>
public interface ICommand<out TResponse> : ICommand
{
}
