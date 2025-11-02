using MediatR;

namespace TaskFlow.BuildingBlocks.CQRS.Abstractions;

/// <summary>
/// Marker interface for queries (read operations)
/// Framework-agnostic - works with MediatR, Wolverine, Brighter, or custom mediator
/// </summary>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
