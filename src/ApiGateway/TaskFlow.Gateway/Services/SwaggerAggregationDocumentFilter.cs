using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TaskFlow.Gateway.Services;

/// <summary>
/// Document filter that aggregates downstream service endpoints into the gateway Swagger document
/// </summary>
public class SwaggerAggregationDocumentFilter : IDocumentFilter
{
    private readonly SwaggerDocumentAggregator _aggregator;
    private readonly ILogger<SwaggerAggregationDocumentFilter> _logger;

    public SwaggerAggregationDocumentFilter(
        SwaggerDocumentAggregator aggregator,
        ILogger<SwaggerAggregationDocumentFilter> logger)
    {
        _aggregator = aggregator;
        _logger = logger;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Only apply aggregation to the "gateway" document
        if (context.DocumentName != "gateway")
        {
            return;
        }

        try
        {
            _logger.LogInformation("Applying Swagger document aggregation for gateway...");

            // Aggregate downstream documents into this one
            var aggregatedDoc = _aggregator.AggregateDocumentsAsync(swaggerDoc).GetAwaiter().GetResult();

            // Replace paths with aggregated paths
            swaggerDoc.Paths = aggregatedDoc.Paths;

            // Merge schemas
            if (aggregatedDoc.Components?.Schemas != null)
            {
                swaggerDoc.Components ??= new OpenApiComponents();
                swaggerDoc.Components.Schemas ??= new Dictionary<string, OpenApiSchema>();

                foreach (var schema in aggregatedDoc.Components.Schemas)
                {
                    if (!swaggerDoc.Components.Schemas.ContainsKey(schema.Key))
                    {
                        swaggerDoc.Components.Schemas.Add(schema.Key, schema.Value);
                    }
                }
            }

            // Merge tags
            if (aggregatedDoc.Tags != null && aggregatedDoc.Tags.Any())
            {
                swaggerDoc.Tags ??= new List<OpenApiTag>();
                foreach (var tag in aggregatedDoc.Tags)
                {
                    if (!swaggerDoc.Tags.Any(t => t.Name == tag.Name))
                    {
                        swaggerDoc.Tags.Add(tag);
                    }
                }
            }

            _logger.LogInformation("Successfully aggregated {PathCount} paths into gateway Swagger document",
                swaggerDoc.Paths.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Swagger document aggregation");
        }
    }
}
