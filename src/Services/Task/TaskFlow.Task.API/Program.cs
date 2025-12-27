using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TaskFlow.Task.Application.Interfaces;
using TaskFlow.Task.Infrastructure.Persistence;
using TaskFlow.Task.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration["Seq:ServerUrl"] ?? "http://seq:5341")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TaskFlow Task Service API",
        Version = "v1",
        Description = "Task Service - Task management for TaskFlow"
    });
});

builder.Services.AddHealthChecks();

builder.Services.AddDbContext<TaskDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(TaskDbContext).Assembly.FullName));
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ITaskItemRepository).Assembly);
});

builder.Services.AddValidatorsFromAssembly(typeof(ITaskItemRepository).Assembly);
TypeAdapterConfig.GlobalSettings.Scan(typeof(ITaskItemRepository).Assembly);

builder.Services.AddScoped<ITaskItemRepository, TaskItemRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Service API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    await dbContext.Database.MigrateAsync();
}

try
{
    Log.Information("Starting Task Service...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Task Service failed to start");
}
finally
{
    Log.CloseAndFlush();
}
