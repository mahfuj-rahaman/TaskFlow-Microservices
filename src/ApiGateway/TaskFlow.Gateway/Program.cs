using Consul;
using TaskFlow.Gateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

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
