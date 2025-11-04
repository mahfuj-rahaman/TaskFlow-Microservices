using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace TaskFlow.Gateway.Controllers;

/// <summary>
/// API Information and Health endpoints
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
public class ApiInfoController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ApiInfoController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Get API Gateway information
    /// </summary>
    /// <returns>API metadata including version, services, and capabilities</returns>
    /// <response code="200">Returns API information</response>
    [HttpGet("info")]
    [SwaggerOperation(
        Summary = "Get API Gateway information",
        Description = "Returns metadata about the API Gateway including version, available services, and capabilities",
        Tags = new[] { "API Gateway" }
    )]
    [SwaggerResponse(200, "API information retrieved successfully", typeof(ApiInfo))]
    [ProducesResponseType(typeof(ApiInfo), StatusCodes.Status200OK)]
    public IActionResult GetApiInfo()
    {
        var apiInfo = new ApiInfo
        {
            Name = "TaskFlow API Gateway",
            Version = "v1",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            Timestamp = DateTime.UtcNow,
            Services = new List<ServiceInfo>
            {
                new()
                {
                    Name = "Identity Service",
                    Description = "Authentication and user management",
                    BaseRoute = "/api/v1/identity",
                    Health = "/health",
                    Status = "Available"
                },
                new()
                {
                    Name = "User Service",
                    Description = "User profile management",
                    BaseRoute = "/api/v1/users",
                    Health = "/health",
                    Status = "Available"
                },
                new()
                {
                    Name = "Task Service",
                    Description = "Task management and tracking",
                    BaseRoute = "/api/v1/tasks",
                    Health = "/health",
                    Status = "Available"
                },
                new()
                {
                    Name = "Admin Service",
                    Description = "Administrative operations",
                    BaseRoute = "/api/v1/admin",
                    Health = "/health",
                    Status = "Available"
                },
                new()
                {
                    Name = "Notification Service",
                    Description = "Notification delivery",
                    BaseRoute = "/api/v1/notifications",
                    Health = "/health",
                    Status = "Available"
                }
            },
            Features = new List<string>
            {
                "JWT Bearer Authentication",
                "Rate Limiting (100 req/min)",
                "CORS Support",
                "Load Balancing",
                "Distributed Tracing",
                "Health Checks",
                "API Versioning (v1)",
                "OpenAPI/Swagger Documentation"
            },
            Documentation = new DocumentationInfo
            {
                SwaggerUrl = "/swagger",
                OpenApiUrl = "/swagger/gateway/swagger.json",
                PostmanCollection = "/api/postman-collection"
            }
        };

        return Ok(apiInfo);
    }

    /// <summary>
    /// Gateway health check
    /// </summary>
    /// <returns>Health status of the gateway</returns>
    /// <response code="200">Gateway is healthy</response>
    [HttpGet("health")]
    [SwaggerOperation(
        Summary = "Gateway health check",
        Description = "Returns the health status of the API Gateway",
        Tags = new[] { "Health" }
    )]
    [SwaggerResponse(200, "Gateway is healthy", typeof(HealthResponse))]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        var health = new HealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Uptime = TimeSpan.FromMilliseconds(Environment.TickCount64),
            Version = "1.0.0"
        };

        return Ok(health);
    }

    /// <summary>
    /// Get Postman collection for API testing
    /// </summary>
    /// <returns>Postman collection JSON</returns>
    /// <response code="200">Postman collection</response>
    [HttpGet("postman-collection")]
    [SwaggerOperation(
        Summary = "Get Postman collection",
        Description = "Downloads a Postman collection for easy API testing",
        Tags = new[] { "API Gateway" }
    )]
    [SwaggerResponse(200, "Postman collection", typeof(object))]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetPostmanCollection()
    {
        var collection = new
        {
            info = new
            {
                name = "TaskFlow API Gateway",
                description = "Complete API collection for TaskFlow microservices",
                version = "1.0.0",
                schema = "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
            },
            auth = new
            {
                type = "bearer",
                bearer = new[]
                {
                    new { key = "token", value = "{{jwt_token}}", type = "string" }
                }
            },
            variable = new[]
            {
                new { key = "base_url", value = "http://localhost:5000", type = "string" },
                new { key = "jwt_token", value = "", type = "string" }
            },
            item = new object[]
            {
                new
                {
                    name = "Authentication",
                    item = new object[]
                    {
                        new
                        {
                            name = "Register",
                            request = new
                            {
                                method = "POST",
                                url = "{{base_url}}/api/v1/auth/register",
                                body = new
                                {
                                    mode = "raw",
                                    raw = "{\n  \"username\": \"testuser\",\n  \"email\": \"test@example.com\",\n  \"password\": \"Test@1234\",\n  \"firstName\": \"Test\",\n  \"lastName\": \"User\"\n}",
                                    options = new { raw = new { language = "json" } }
                                }
                            }
                        },
                        new
                        {
                            name = "Login",
                            request = new
                            {
                                method = "POST",
                                url = "{{base_url}}/api/v1/auth/login",
                                body = new
                                {
                                    mode = "raw",
                                    raw = "{\n  \"emailOrUsername\": \"testuser\",\n  \"password\": \"Test@1234\"\n}",
                                    options = new { raw = new { language = "json" } }
                                }
                            }
                        }
                    }
                },
                new
                {
                    name = "Identity Service",
                    item = new object[]
                    {
                        new
                        {
                            name = "Get All Users",
                            request = new
                            {
                                method = "GET",
                                url = "{{base_url}}/api/v1/identity/appusers"
                            }
                        }
                    }
                }
            }
        };

        return Ok(collection);
    }
}

#region Response Models

/// <summary>
/// API information response
/// </summary>
public class ApiInfo
{
    /// <summary>API name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>API version</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Current environment</summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>Server timestamp</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Available services</summary>
    public List<ServiceInfo> Services { get; set; } = new();

    /// <summary>Enabled features</summary>
    public List<string> Features { get; set; } = new();

    /// <summary>Documentation links</summary>
    public DocumentationInfo Documentation { get; set; } = new();
}

/// <summary>
/// Service information
/// </summary>
public class ServiceInfo
{
    /// <summary>Service name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Service description</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Base route</summary>
    public string BaseRoute { get; set; } = string.Empty;

    /// <summary>Health endpoint</summary>
    public string Health { get; set; } = string.Empty;

    /// <summary>Current status</summary>
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Documentation links
/// </summary>
public class DocumentationInfo
{
    /// <summary>Swagger UI URL</summary>
    public string SwaggerUrl { get; set; } = string.Empty;

    /// <summary>OpenAPI spec URL</summary>
    public string OpenApiUrl { get; set; } = string.Empty;

    /// <summary>Postman collection URL</summary>
    public string PostmanCollection { get; set; } = string.Empty;
}

/// <summary>
/// Health check response
/// </summary>
public class HealthResponse
{
    /// <summary>Health status</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Timestamp</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Uptime</summary>
    public TimeSpan Uptime { get; set; }

    /// <summary>Version</summary>
    public string Version { get; set; } = string.Empty;
}

#endregion
