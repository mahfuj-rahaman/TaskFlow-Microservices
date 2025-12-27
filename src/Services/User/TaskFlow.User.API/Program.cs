using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TaskFlow.User.Application.Interfaces;
using TaskFlow.User.Infrastructure.Persistence;
using TaskFlow.User.Infrastructure.Repositories;

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
        Title = "TaskFlow User Service API",
        Version = "v1",
        Description = "User Service - User profile management for TaskFlow"
    });
});

builder.Services.AddHealthChecks();

builder.Services.AddDbContext<UserDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(UserDbContext).Assembly.FullName));
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IUserProfileRepository).Assembly);
});

builder.Services.AddValidatorsFromAssembly(typeof(IUserProfileRepository).Assembly);
TypeAdapterConfig.GlobalSettings.Scan(typeof(IUserProfileRepository).Assembly);

builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();

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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    await dbContext.Database.MigrateAsync();
}

try
{
    Log.Information("Starting User Service...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "User Service failed to start");
}
finally
{
    Log.CloseAndFlush();
}
