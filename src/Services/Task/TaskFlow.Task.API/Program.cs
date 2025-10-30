using FluentValidation;
using Mapster;
using TaskFlow.Task.Application.Mappings;
using TaskFlow.Task.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(TaskFlow.Task.Application.DTOs.TaskDto).Assembly);
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(TaskFlow.Task.Application.DTOs.TaskDto).Assembly);

// Configure Mapster
TaskMappingConfig.Configure();

// Add Infrastructure (DbContext, Repositories, UnitOfWork)
builder.Services.AddInfrastructure(builder.Configuration);

// Add API versioning
builder.Services.AddEndpointsApiExplorer();

// Add Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "TaskFlow Task Service API",
        Version = "v1",
        Description = "RESTful API for Task Management Service",
        Contact = new()
        {
            Name = "TaskFlow Team",
            Email = "support@taskflow.com"
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TaskFlow.Task.Infrastructure.Persistence.TaskDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow Task Service API v1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

// Welcome endpoint
app.MapGet("/", () => new
{
    service = "TaskFlow Task Service",
    version = "v1.0.0",
    status = "running",
    documentation = "/swagger"
}).WithName("ServiceInfo").WithOpenApi();

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
