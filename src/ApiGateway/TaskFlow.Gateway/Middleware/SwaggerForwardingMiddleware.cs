namespace TaskFlow.Gateway.Middleware;

/// <summary>
/// Middleware to forward Swagger/OpenAPI requests to downstream services
/// This enables API Gateway to aggregate all microservice documentation
/// </summary>
public class SwaggerForwardingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SwaggerForwardingMiddleware> _logger;

    private static readonly Dictionary<string, string> ServiceEndpoints = new()
    {
        { "identity", "http://identity-service:8080" },
        { "user", "http://user-service:8080" },
        { "task", "http://task-service:8080" },
        { "admin", "http://admin-service:8080" },
        { "notification", "http://notif-service:8080" }
    };

    public SwaggerForwardingMiddleware(
        RequestDelegate next,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<SwaggerForwardingMiddleware> logger)
    {
        _next = next;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only intercept Swagger JSON requests for downstream services
        if (context.Request.Path.StartsWithSegments("/swagger") &&
            context.Request.Path.Value?.EndsWith("/swagger.json") == true)
        {
            var pathSegments = context.Request.Path.Value.Split('/');
            if (pathSegments.Length >= 3)
            {
                var serviceName = pathSegments[2]; // e.g., "identity", "user", etc.

                // Skip gateway - it's handled by normal Swagger generation
                if (serviceName != "gateway" && ServiceEndpoints.TryGetValue(serviceName, out var serviceUrl))
                {
                    await ForwardSwaggerRequest(context, serviceName, serviceUrl);
                    return;
                }
            }
        }

        await _next(context);
    }

    private async Task ForwardSwaggerRequest(HttpContext context, string serviceName, string serviceUrl)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            // Request the Swagger JSON from the downstream service
            var downstreamUrl = $"{serviceUrl}/swagger/v1/swagger.json";
            _logger.LogInformation("Fetching Swagger doc from {ServiceName} at {Url}", serviceName, downstreamUrl);

            var response = await client.GetAsync(downstreamUrl);

            if (response.IsSuccessStatusCode)
            {
                var swaggerJson = await response.Content.ReadAsStringAsync();

                // Modify the paths to include the API Gateway route prefix
                swaggerJson = TransformSwaggerPaths(swaggerJson, serviceName);

                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(swaggerJson);

                _logger.LogInformation("Successfully forwarded Swagger doc from {ServiceName}", serviceName);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to fetch Swagger doc from {ServiceName}. Status: {StatusCode}",
                    serviceName,
                    response.StatusCode);

                // Return a placeholder Swagger document
                var placeholderDoc = CreatePlaceholderSwaggerDoc(serviceName, serviceUrl);
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(placeholderDoc);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forwarding Swagger request for {ServiceName}", serviceName);

            // Return a placeholder document on error
            var placeholderDoc = CreatePlaceholderSwaggerDoc(serviceName, serviceUrl);
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(placeholderDoc);
        }
    }

    private string TransformSwaggerPaths(string swaggerJson, string serviceName)
    {
        try
        {
            // Parse and modify the Swagger JSON to prepend gateway route prefix
            // This maps downstream service paths to gateway routes
            var pathMappings = new Dictionary<string, string>
            {
                { "identity", "/api/v1" },  // Identity paths already include /api/v1
                { "user", "/api/v1" },      // User paths should be prefixed
                { "task", "/api/v1" },      // Task paths should be prefixed
                { "admin", "/api/v1" },     // Admin paths should be prefixed
                { "notification", "/api/v1" } // Notification paths should be prefixed
            };

            // Note: For more sophisticated path transformation, consider using
            // a JSON library like System.Text.Json or Newtonsoft.Json
            // For now, we'll return the document as-is and rely on YARP routing

            return swaggerJson;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transforming Swagger paths for {ServiceName}", serviceName);
            return swaggerJson;
        }
    }

    private string CreatePlaceholderSwaggerDoc(string serviceName, string serviceUrl)
    {
        var serviceDisplayName = serviceName switch
        {
            "identity" => "Identity Service",
            "user" => "User Service",
            "task" => "Task Service",
            "admin" => "Admin Service",
            "notification" => "Notification Service",
            _ => $"{serviceName} Service"
        };

        var description = $"⚠️ Could not fetch API documentation from {serviceDisplayName} at {serviceUrl}.\n\n" +
                         $"**Possible reasons:**\n" +
                         $"- Service is not running\n" +
                         $"- Service is starting up (check again in a few seconds)\n" +
                         $"- Service does not have Swagger enabled\n" +
                         $"- Network connectivity issues\n\n" +
                         $"**Direct access:** {serviceUrl.Replace("8080", GetServicePort(serviceName))}/swagger";

        return $$"""
        {
          "openapi": "3.0.1",
          "info": {
            "title": "{{serviceDisplayName}} API",
            "description": "{{description}}",
            "version": "v1"
          },
          "paths": {},
          "components": {
            "schemas": {}
          }
        }
        """;
    }

    private string GetServicePort(string serviceName)
    {
        return serviceName switch
        {
            "identity" => "5006",
            "user" => "5001",
            "task" => "5005",
            "admin" => "5007",
            "notification" => "5004",
            _ => "8080"
        };
    }
}
