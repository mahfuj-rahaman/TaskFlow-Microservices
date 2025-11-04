using Consul;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using TaskFlow.Gateway.Configuration;
using TaskFlow.Gateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// 📁 CONFIGURATION LOADING
// =============================================================================

var env = builder.Environment;

builder.Configuration
    .SetBasePath(env.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

var provider = Environment.GetEnvironmentVariable("CLOUD_PROVIDER");
if (!string.IsNullOrEmpty(provider))
{
    var providerFile = $"appsettings.{provider}.{env.EnvironmentName}.json";
    builder.Configuration.AddJsonFile(providerFile, optional: true, reloadOnChange: true);
}

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddCommandLine(args);

// =============================================================================
// 🔐 JWT AUTHENTICATION
// =============================================================================

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = jwtSettings.GetValue<bool>("ValidateIssuer"),
            ValidateAudience = jwtSettings.GetValue<bool>("ValidateAudience"),
            ValidateLifetime = jwtSettings.GetValue<bool>("ValidateLifetime"),
            ValidateIssuerSigningKey = jwtSettings.GetValue<bool>("ValidateIssuerSigningKey"),
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Parse(jwtSettings["ClockSkew"] ?? "00:05:00")
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("authenticated", policy => policy.RequireAuthenticatedUser());
    options.AddPolicy("admin", policy => policy.RequireRole("Admin", "SuperAdmin"));
    options.AddPolicy("superadmin", policy => policy.RequireRole("SuperAdmin"));
});

// =============================================================================
// 🌐 CORS CONFIGURATION
// =============================================================================

var corsSettings = builder.Configuration.GetSection("Cors");
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        var origins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "*" };
        var methods = corsSettings.GetSection("AllowedMethods").Get<string[]>() ?? new[] { "GET", "POST", "PUT", "DELETE" };
        var headers = corsSettings.GetSection("AllowedHeaders").Get<string[]>() ?? new[] { "*" };
        var allowCredentials = corsSettings.GetValue<bool>("AllowCredentials");

        if (origins.Contains("*"))
            policy.AllowAnyOrigin();
        else
            policy.WithOrigins(origins);

        policy.WithMethods(methods)
              .WithHeaders(headers);

        if (allowCredentials && !origins.Contains("*"))
            policy.AllowCredentials();

        var exposedHeaders = corsSettings.GetSection("ExposedHeaders").Get<string[]>();
        if (exposedHeaders?.Length > 0)
            policy.WithExposedHeaders(exposedHeaders);
    });
});

// =============================================================================
// 🛡️ RATE LIMITING
// =============================================================================

var rateLimitSettings = builder.Configuration.GetSection("RateLimiting");
if (rateLimitSettings.GetValue<bool>("Enabled"))
{
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = rateLimitSettings.GetValue<int>("PermitLimit"),
                    QueueLimit = rateLimitSettings.GetValue<int>("QueueLimit"),
                    Window = TimeSpan.Parse(rateLimitSettings["Window"] ?? "00:01:00")
                }));

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });
}

// =============================================================================
// 🔍 SERVICE DISCOVERY (CONSUL)
// =============================================================================

builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
{
    consulConfig.Address = new Uri(builder.Configuration["Consul:Host"] ?? "http://localhost:8500");
}));

// =============================================================================
// 💾 DISTRIBUTED CACHE
// =============================================================================

builder.Services.AddDistributedMemoryCache();

// =============================================================================
// 🌐 HTTP CLIENT FACTORY (for Swagger forwarding)
// =============================================================================

builder.Services.AddHttpClient();

// =============================================================================
// 📚 SWAGGER / OPENAPI DOCUMENTATION
// =============================================================================

builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation(builder.Configuration);

// =============================================================================
// 🌐 YARP REVERSE PROXY
// =============================================================================

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// =============================================================================
// 🏗️ APPLICATION PIPELINE
// =============================================================================

var app = builder.Build();

// =============================================================================
// 📚 SWAGGER UI (Development & Staging)
// =============================================================================

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwaggerDocumentation(app.Configuration);
}

// Serve static files (for swagger-custom.css)
app.UseStaticFiles();

// =============================================================================
// 📚 SWAGGER FORWARDING MIDDLEWARE
// =============================================================================
// This middleware intercepts Swagger JSON requests and forwards them to
// downstream services, enabling API Gateway to aggregate all service docs
app.UseMiddleware<SwaggerForwardingMiddleware>();

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }

    await next();
});

// Request ID for tracing
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("X-Request-Id"))
    {
        context.Request.Headers.Append("X-Request-Id", Guid.NewGuid().ToString());
    }
    context.Response.Headers.Append("X-Request-Id", context.Request.Headers["X-Request-Id"].ToString());
    await next();
});

// Enable CORS
app.UseCors("DefaultCorsPolicy");

// Enable rate limiting
if (rateLimitSettings.GetValue<bool>("Enabled"))
{
    app.UseRateLimiter();
}

// Enable authentication & authorization
app.UseAuthentication();
app.UseAuthorization();

// Custom idempotency middleware
app.UseIdempotency();

// Map controllers (for API info endpoints)
app.MapControllers();

// YARP reverse proxy
app.MapReverseProxy();

app.Run();
