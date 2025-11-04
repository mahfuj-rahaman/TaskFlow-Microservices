using Microsoft.OpenApi.Models;

namespace TaskFlow.Gateway.Configuration;

/// <summary>
/// Swagger/OpenAPI configuration for API Gateway with downstream service aggregation
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// Downstream services that will be documented
    /// </summary>
    public static class Services
    {
        public const string Identity = "identity";
        public const string User = "user";
        public const string Task = "task";
        public const string Admin = "admin";
        public const string Notification = "notification";
    }

    /// <summary>
    /// Add Swagger generation services
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            // =================================================================
            // GATEWAY API DOCUMENTATION
            // =================================================================
            options.SwaggerDoc("gateway", new OpenApiInfo
            {
                Title = "TaskFlow API Gateway",
                Version = "v1",
                Description = @"
**TaskFlow API Gateway** - Unified entry point for all TaskFlow microservices.

## Features
- 🔐 JWT Bearer Authentication
- 🛡️ Rate Limiting (100 req/min)
- 🌐 CORS Support
- 🔄 Load Balancing
- 📊 Distributed Tracing
- ✅ Health Checks

## Authentication
Most endpoints require JWT Bearer token authentication:
1. Register via `/api/v1/auth/register`
2. Login via `/api/v1/auth/login` to get your JWT token
3. Click **Authorize** button and enter: `Bearer <your-token>`

## Rate Limiting
- **Limit**: 100 requests per minute
- **Response**: 429 Too Many Requests when exceeded

## Base URL
- Development: `http://localhost:5000`
- Production: `https://api.taskflow.com`
",
                Contact = new OpenApiContact
                {
                    Name = "TaskFlow Team",
                    Email = "support@taskflow.com",
                    Url = new Uri("https://github.com/mahfuj-rahaman/TaskFlow-Microservices")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // =================================================================
            // IDENTITY SERVICE DOCUMENTATION
            // =================================================================
            options.SwaggerDoc(Services.Identity, new OpenApiInfo
            {
                Title = "Identity Service API",
                Version = "v1",
                Description = @"
**Identity Service** - Authentication and user management.

## Capabilities
- User registration with email confirmation
- JWT-based authentication (Access + Refresh tokens)
- Password reset flow
- Role-based authorization (User, Admin, SuperAdmin)
- Two-factor authentication (prepared)
- Account lockout protection

## Endpoints
- `/api/v1/auth/*` - Authentication endpoints (public)
- `/api/v1/identity/appusers/*` - User management (authenticated)
",
                Contact = new OpenApiContact
                {
                    Name = "Identity Service Team",
                    Email = "identity@taskflow.com"
                }
            });

            // =================================================================
            // USER SERVICE DOCUMENTATION
            // =================================================================
            options.SwaggerDoc(Services.User, new OpenApiInfo
            {
                Title = "User Service API",
                Version = "v1",
                Description = @"
**User Service** - User profile and preferences management.

## Capabilities
- User profile CRUD operations
- Profile settings management
- User preferences
- Avatar upload

## Endpoints
- `/api/v1/users/*` - User profile endpoints
",
                Contact = new OpenApiContact
                {
                    Name = "User Service Team",
                    Email = "user@taskflow.com"
                }
            });

            // =================================================================
            // TASK SERVICE DOCUMENTATION
            // =================================================================
            options.SwaggerDoc(Services.Task, new OpenApiInfo
            {
                Title = "Task Service API",
                Version = "v1",
                Description = @"
**Task Service** - Task management and tracking.

## Capabilities
- Task CRUD operations
- Task assignment
- Priority management
- Status tracking
- Due date management

## Endpoints
- `/api/v1/tasks/*` - Task management endpoints
",
                Contact = new OpenApiContact
                {
                    Name = "Task Service Team",
                    Email = "task@taskflow.com"
                }
            });

            // =================================================================
            // ADMIN SERVICE DOCUMENTATION
            // =================================================================
            options.SwaggerDoc(Services.Admin, new OpenApiInfo
            {
                Title = "Admin Service API",
                Version = "v1",
                Description = @"
**Admin Service** - Administrative operations and system management.

## Capabilities
- User activation/deactivation
- Role assignment
- System configuration
- Audit logs

## Endpoints
- `/api/v1/admin/*` - Admin endpoints (Admin/SuperAdmin only)

**Note**: Requires Admin or SuperAdmin role.
",
                Contact = new OpenApiContact
                {
                    Name = "Admin Service Team",
                    Email = "admin@taskflow.com"
                }
            });

            // =================================================================
            // NOTIFICATION SERVICE DOCUMENTATION
            // =================================================================
            options.SwaggerDoc(Services.Notification, new OpenApiInfo
            {
                Title = "Notification Service API",
                Version = "v1",
                Description = @"
**Notification Service** - Notification delivery and management.

## Capabilities
- Email notifications
- Push notifications
- Notification preferences
- Notification history

## Endpoints
- `/api/v1/notifications/*` - Notification endpoints
",
                Contact = new OpenApiContact
                {
                    Name = "Notification Service Team",
                    Email = "notification@taskflow.com"
                }
            });

            // =================================================================
            // JWT AUTHENTICATION CONFIGURATION
            // =================================================================
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = @"
JWT Authorization header using the Bearer scheme.

Enter your JWT token in the text input below.
Example: '12345abcdef'

You can obtain a JWT token by:
1. Registering via POST /api/v1/auth/register
2. Logging in via POST /api/v1/auth/login
3. The login response will contain 'accessToken' - copy that value
"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // =================================================================
            // ENABLE ANNOTATIONS
            // =================================================================
            options.EnableAnnotations();

            // =================================================================
            // ROUTE FILTERING BY SERVICE
            // =================================================================
            options.DocInclusionPredicate((docName, apiDesc) =>
            {
                if (docName == "gateway")
                    return true;

                var routePath = apiDesc.RelativePath?.ToLower() ?? "";

                return docName switch
                {
                    Services.Identity => routePath.Contains("/identity") || routePath.Contains("/auth"),
                    Services.User => routePath.Contains("/users"),
                    Services.Task => routePath.Contains("/tasks"),
                    Services.Admin => routePath.Contains("/admin"),
                    Services.Notification => routePath.Contains("/notifications"),
                    _ => false
                };
            });

            // =================================================================
            // SCHEMA CUSTOMIZATION
            // =================================================================
            options.CustomSchemaIds(type => type.FullName);
        });

        return services;
    }

    /// <summary>
    /// Use Swagger UI with custom configuration
    /// </summary>
    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "swagger/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(options =>
        {
            // =================================================================
            // SWAGGER UI CONFIGURATION
            // =================================================================
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "TaskFlow API Documentation";

            // Enable deep linking
            options.EnableDeepLinking();

            // Enable filtering
            options.EnableFilter();

            // Enable try-it-out by default
            options.EnableTryItOutByDefault();

            // Show request duration
            options.DisplayRequestDuration();

            // Default models expansion
            options.DefaultModelsExpandDepth(2);
            options.DefaultModelExpandDepth(2);

            // =================================================================
            // DOCUMENT ENDPOINTS (ORDER MATTERS FOR UI)
            // =================================================================
            options.SwaggerEndpoint("/swagger/gateway/swagger.json", "🌐 API Gateway (All Services)");
            options.SwaggerEndpoint("/swagger/identity/swagger.json", "🔐 Identity Service");
            options.SwaggerEndpoint("/swagger/user/swagger.json", "👤 User Service");
            options.SwaggerEndpoint("/swagger/task/swagger.json", "✅ Task Service");
            options.SwaggerEndpoint("/swagger/admin/swagger.json", "👑 Admin Service");
            options.SwaggerEndpoint("/swagger/notification/swagger.json", "📧 Notification Service");

            // =================================================================
            // CUSTOM CSS (Optional - for branding)
            // =================================================================
            options.InjectStylesheet("/swagger-custom.css");

            // =================================================================
            // OAUTH CONFIGURATION (if needed in future)
            // =================================================================
            var jwtSettings = configuration.GetSection("JwtSettings");
            options.OAuthClientId("taskflow-swagger");
            options.OAuthAppName("TaskFlow Swagger UI");
            options.OAuthUsePkce();
        });

        return app;
    }
}
