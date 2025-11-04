using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration["Seq:ServerUrl"] ?? "http://seq:5341")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TaskFlow User Service API",
        Version = "v1",
        Description = "User Service - User profile and preferences management for TaskFlow microservices"
    });
});

// Add Health Checks
builder.Services.AddHealthChecks();

// TODO: Add service-specific dependencies here
// - DbContext
// - Repositories
// - MediatR
// - FluentValidation
// - Mapster
// - Authentication/Authorization

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable Swagger in all environments for API Gateway aggregation
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

try
{
    Log.Information("Starting application...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}
