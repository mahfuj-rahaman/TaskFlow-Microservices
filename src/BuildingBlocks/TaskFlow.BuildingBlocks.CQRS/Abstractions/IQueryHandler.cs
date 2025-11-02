namespace TaskFlow.BuildingBlocks.CQRS.Abstractions;

/// <summary>
/// Handler for queries
/// Framework-agnostic - works with MediatR, Wolverine, Brighter, or custom mediator
/// </summary>
/// <typeparam name="TQuery">The query type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Handles the query and returns a response
    /// </summary>
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
