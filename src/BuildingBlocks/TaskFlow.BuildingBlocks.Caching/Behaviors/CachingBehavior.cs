using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.BuildingBlocks.Caching.Abstractions;

namespace TaskFlow.BuildingBlocks.Caching.Behaviors;

/// <summary>
/// MediatR pipeline behavior that automatically caches query results
/// Only works with queries that implement ICacheableQuery
/// </summary>
/// <typeparam name="TRequest">The request type (must implement ICacheableQuery)</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public sealed class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        ICacheService cacheService,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cacheKey = request.CacheKey;
        var requestType = typeof(TRequest).Name;

        _logger.LogDebug("Checking cache for query {QueryType} with key: {CacheKey}", requestType, cacheKey);

        // Try to get from cache
        var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cachedResponse is not null)
        {
            _logger.LogInformation("Cache hit for query {QueryType} with key: {CacheKey}", requestType, cacheKey);
            return cachedResponse;
        }

        _logger.LogDebug("Cache miss for query {QueryType} with key: {CacheKey}", requestType, cacheKey);

        // Execute the query handler
        var response = await next();

        // Cache the response
        if (response is not null)
        {
            await _cacheService.SetAsync(
                cacheKey,
                response,
                request.CacheExpiration,
                cancellationToken);

            _logger.LogDebug(
                "Cached response for query {QueryType} with key: {CacheKey}, expiration: {Expiration}",
                requestType,
                cacheKey,
                request.CacheExpiration);
        }

        return response;
    }
}
