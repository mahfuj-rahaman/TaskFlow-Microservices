using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System.Text.Json;

namespace TaskFlow.Gateway.Services;

/// <summary>
/// Aggregates Swagger documents from downstream services into the gateway document
/// </summary>
public class SwaggerDocumentAggregator
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SwaggerDocumentAggregator> _logger;

    private static readonly Dictionary<string, string> ServiceEndpoints = new()
    {
        { "identity", "http://identity-service:8080" },
        { "user", "http://user-service:8080" },
        { "task", "http://task-service:8080" },
        { "admin", "http://admin-service:8080" },
        { "notification", "http://notif-service:8080" }
    };

    private static readonly Dictionary<string, string> ServicePrefixes = new()
    {
        { "identity", "/api/v1" },
        { "user", "/api/v1/users" },
        { "task", "/api/v1/tasks" },
        { "admin", "/api/v1/admin" },
        { "notification", "/api/v1/notifications" }
    };

    public SwaggerDocumentAggregator(
        IHttpClientFactory httpClientFactory,
        ILogger<SwaggerDocumentAggregator> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Fetches and aggregates Swagger documents from all downstream services
    /// </summary>
    public async Task<OpenApiDocument> AggregateDocumentsAsync(OpenApiDocument gatewayDoc)
    {
        _logger.LogInformation("Starting Swagger document aggregation...");

        var aggregatedDoc = new OpenApiDocument
        {
            Info = gatewayDoc.Info,
            Servers = gatewayDoc.Servers,
            Paths = new OpenApiPaths(),
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>(),
                SecuritySchemes = gatewayDoc.Components?.SecuritySchemes ?? new Dictionary<string, OpenApiSecurityScheme>()
            },
            SecurityRequirements = gatewayDoc.SecurityRequirements,
            Tags = new List<OpenApiTag>()
        };

        // Add gateway's own paths (if any)
        if (gatewayDoc.Paths != null)
        {
            foreach (var path in gatewayDoc.Paths)
            {
                aggregatedDoc.Paths.Add(path.Key, path.Value);
            }
        }

        // Fetch and merge downstream service documents
        foreach (var service in ServiceEndpoints)
        {
            try
            {
                var serviceDoc = await FetchServiceDocumentAsync(service.Key, service.Value);
                if (serviceDoc != null)
                {
                    MergeServiceDocument(aggregatedDoc, serviceDoc, service.Key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch Swagger document from {Service}", service.Key);
            }
        }

        _logger.LogInformation("Swagger document aggregation completed. Total paths: {PathCount}", aggregatedDoc.Paths.Count);

        return aggregatedDoc;
    }

    private async Task<OpenApiDocument?> FetchServiceDocumentAsync(string serviceName, string serviceUrl)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            var swaggerUrl = $"{serviceUrl}/swagger/v1/swagger.json";
            _logger.LogDebug("Fetching Swagger from {ServiceName} at {Url}", serviceName, swaggerUrl);

            var response = await client.GetAsync(swaggerUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch Swagger from {ServiceName}. Status: {Status}", serviceName, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var reader = new OpenApiStringReader();
            var document = reader.Read(json, out var diagnostic);

            if (diagnostic.Errors.Count > 0)
            {
                _logger.LogWarning("Errors parsing Swagger from {ServiceName}: {Errors}",
                    serviceName, string.Join(", ", diagnostic.Errors));
            }

            _logger.LogInformation("Successfully fetched Swagger from {ServiceName}. Paths: {PathCount}",
                serviceName, document.Paths.Count);

            return document;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Swagger document from {ServiceName}", serviceName);
            return null;
        }
    }

    private void MergeServiceDocument(OpenApiDocument target, OpenApiDocument source, string serviceName)
    {
        // Add service tag
        var serviceTag = new OpenApiTag
        {
            Name = serviceName,
            Description = source.Info?.Description ?? $"{serviceName} service endpoints"
        };
        target.Tags.Add(serviceTag);

        // Merge paths with gateway route prefix
        foreach (var path in source.Paths)
        {
            var gatewayPath = TransformPathForGateway(path.Key, serviceName);

            // Add service tag to all operations
            foreach (var operation in path.Value.Operations.Values)
            {
                if (operation.Tags == null || !operation.Tags.Any())
                {
                    operation.Tags = new List<OpenApiTag> { serviceTag };
                }
                else
                {
                    // Prepend service name to existing tags
                    var existingTags = operation.Tags.ToList();
                    operation.Tags.Clear();
                    operation.Tags.Add(serviceTag);
                    foreach (var tag in existingTags)
                    {
                        operation.Tags.Add(tag);
                    }
                }
            }

            // Add the path (avoid duplicates)
            if (!target.Paths.ContainsKey(gatewayPath))
            {
                target.Paths.Add(gatewayPath, path.Value);
            }
            else
            {
                _logger.LogWarning("Duplicate path detected: {Path}", gatewayPath);
            }
        }

        // Merge schemas
        if (source.Components?.Schemas != null)
        {
            foreach (var schema in source.Components.Schemas)
            {
                // Prefix schema names with service name to avoid conflicts
                var schemaName = $"{serviceName}.{schema.Key}";
                if (!target.Components.Schemas.ContainsKey(schemaName))
                {
                    target.Components.Schemas.Add(schemaName, schema.Value);
                }
            }
        }
    }

    private string TransformPathForGateway(string originalPath, string serviceName)
    {
        // Map service paths to gateway routes based on YARP configuration
        // YARP transforms: Gateway Route → Downstream Service Path
        // We need to do the reverse: Downstream Service Path → Gateway Route

        return serviceName switch
        {
            // Identity Service:
            // - /api/v1/appusers/{**} → /api/v1/identity/appusers/{**}
            // - /api/v1/auth/{**} → /api/v1/auth/{**} (no change)
            "identity" => originalPath.Replace("/api/v1/appusers", "/api/v1/identity/appusers")
                                     .Replace("/api/v1/AppUsers", "/api/v1/identity/appusers"),

            // User Service:
            // - /api/users/{**} → /api/v1/users/{**}
            // - /api/Values → /api/v1/users/values (if it's the Values controller)
            "user" => originalPath.StartsWith("/api/users")
                ? originalPath.Replace("/api/users", "/api/v1/users")
                : originalPath.Replace("/api/", "/api/v1/users/"),

            // Task Service:
            // - /api/tasks/{**} → /api/v1/tasks/{**}
            "task" => originalPath.Replace("/api/tasks", "/api/v1/tasks")
                                 .Replace("/api/", "/api/v1/tasks/"),

            // Admin Service:
            // - /api/{**} → /api/v1/admin/{**}
            "admin" => originalPath.Replace("/api/", "/api/v1/admin/"),

            // Notification Service:
            // - /api/{**} → /api/v1/notifications/{**}
            "notification" => originalPath.Replace("/api/", "/api/v1/notifications/"),

            _ => originalPath
        };
    }
}
