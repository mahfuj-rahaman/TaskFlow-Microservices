using Consul;
using TaskFlow.Gateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

// --- Custom Configuration Loading ---

var env = builder.Environment; // Development, Staging, Production

// Base + Environment files
builder.Configuration
    .SetBasePath(env.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Detect cloud provider (manual or automatic)
var provider = Environment.GetEnvironmentVariable("CLOUD_PROVIDER"); // e.g. "Aws", "Azure", "Gcp"
if (!string.IsNullOrEmpty(provider))
{
    var providerFile = $"appsettings.{provider}.{env.EnvironmentName}.json";
    builder.Configuration.AddJsonFile(providerFile, optional: true, reloadOnChange: true);
}

// Add environment variables and command-line overrides
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddCommandLine(args);



builder.Services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
{
    consulConfig.Address = new Uri(builder.Configuration["Consul:Host"]);
}));

builder.Services.AddDistributedMemoryCache();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.UseIdempotency();

app.MapReverseProxy();

app.Run();
