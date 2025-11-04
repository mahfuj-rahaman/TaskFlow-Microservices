using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace TaskFlow.Gateway.Services;

/// <summary>
/// Custom Swagger provider that aggregates downstream service documents
/// </summary>
public class AggregatedSwaggerProvider : ISwaggerProvider
{
    private readonly ISwaggerProvider _defaultProvider;
    private readonly SwaggerDocumentAggregator _aggregator;
    private readonly ILogger<AggregatedSwaggerProvider> _logger;
    private OpenApiDocument? _cachedGatewayDoc;
    private DateTime _cacheTime;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

    public AggregatedSwaggerProvider(
        ISwaggerProvider defaultProvider,
        SwaggerDocumentAggregator aggregator,
        ILogger<AggregatedSwaggerProvider> logger)
    {
        _defaultProvider = defaultProvider;
        _aggregator = aggregator;
        _logger = logger;
    }

    public OpenApiDocument GetSwagger(string documentName, string? host = null, string? basePath = null)
    {
        // Only aggregate for the "gateway" document
        if (documentName != "gateway")
        {
            return _defaultProvider.GetSwagger(documentName, host, basePath);
        }

        // Check cache
        if (_cachedGatewayDoc != null && DateTime.UtcNow - _cacheTime < _cacheExpiration)
        {
            _logger.LogDebug("Returning cached gateway Swagger document");
            return _cachedGatewayDoc;
        }

        try
        {
            _logger.LogInformation("Generating aggregated gateway Swagger document...");

            // Get the base gateway document
            var gatewayDoc = _defaultProvider.GetSwagger(documentName, host, basePath);

            // Aggregate downstream service documents
            var aggregatedDoc = _aggregator.AggregateDocumentsAsync(gatewayDoc).GetAwaiter().GetResult();

            // Cache the result
            _cachedGatewayDoc = aggregatedDoc;
            _cacheTime = DateTime.UtcNow;

            _logger.LogInformation("Successfully generated aggregated gateway Swagger document");
            return aggregatedDoc;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating aggregated Swagger document");
            // Fallback to default provider
            return _defaultProvider.GetSwagger(documentName, host, basePath);
        }
    }
}
