using MediatR;

namespace TaskFlow.BuildingBlocks.CQRS.Abstractions;

/// <summary>
/// Marker interface for commands (write operations)
/// Framework-agnostic - works with MediatR, Wolverine, Brighter, or custom mediator
/// </summary>
public interface ICommand : IRequest
{
}

/// <summary>
/// Marker interface for commands with a response
/// </summary>
/// <typeparam name="TResponse">The response type</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>, ICommand
{
}
