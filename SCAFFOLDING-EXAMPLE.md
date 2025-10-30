# Scaffolding Example - Complete Walkthrough

This guide shows a complete example of using the scaffolding script to build a User Service from scratch.

## Step-by-Step Example

### Step 1: Create the Domain Entity

Create `src/Services/User/TaskFlow.User.Domain/Entities/UserEntity.cs`:

```csharp
using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.User.Domain.Enums;
using TaskFlow.User.Domain.Events;
using TaskFlow.User.Domain.Exceptions;

namespace TaskFlow.User.Domain.Entities;

/// <summary>
/// User aggregate root representing a system user
/// </summary>
public sealed class UserEntity : AggregateRoot<Guid>
{
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    // Private constructor for EF Core
    private UserEntity() { }

    /// <summary>
    /// Creates a new user
    /// </summary>
    public static UserEntity Create(
        string email,
        string firstName,
        string lastName,
        DateTime dateOfBirth)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(email))
            throw new UserDomainException("Email is required");

        if (string.IsNullOrWhiteSpace(firstName))
            throw new UserDomainException("First name is required");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new UserDomainException("Last name is required");

        if (dateOfBirth >= DateTime.Today)
            throw new UserDomainException("Date of birth must be in the past");

        if (dateOfBirth > DateTime.Today.AddYears(-18))
            throw new UserDomainException("User must be at least 18 years old");

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        // Raise domain event
        user.RaiseDomainEvent(new UserCreatedDomainEvent(user.Id, user.Email));

        return user;
    }

    /// <summary>
    /// Updates user information
    /// </summary>
    public void Update(string firstName, string lastName, DateTime dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new UserDomainException("First name is required");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new UserDomainException("Last name is required");

        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the user
    /// </summary>
    public void Deactivate()
    {
        if (Status == UserStatus.Inactive)
            throw new UserDomainException("User is already inactive");

        Status = UserStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new UserDeactivatedDomainEvent(Id));
    }

    /// <summary>
    /// Activates the user
    /// </summary>
    public void Activate()
    {
        if (Status == UserStatus.Active)
            throw new UserDomainException("User is already active");

        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records user login
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets full name
    /// </summary>
    public string GetFullName() => $"{FirstName} {LastName}";

    /// <summary>
    /// Calculates age
    /// </summary>
    public int GetAge()
    {
        var today = DateTime.Today;
        var age = today.Year - DateOfBirth.Year;
        if (DateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}
```

### Step 2: Create Supporting Domain Files

Create `src/Services/User/TaskFlow.User.Domain/Enums/UserStatus.cs`:

```csharp
namespace TaskFlow.User.Domain.Enums;

/// <summary>
/// User account status
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// User account is active
    /// </summary>
    Active = 1,

    /// <summary>
    /// User account is inactive
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// User account is suspended
    /// </summary>
    Suspended = 3
}
```

Create `src/Services/User/TaskFlow.User.Domain/Events/UserCreatedDomainEvent.cs`:

```csharp
using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.User.Domain.Events;

/// <summary>
/// Domain event raised when a user is created
/// </summary>
public sealed record UserCreatedDomainEvent(
    Guid UserId,
    string Email) : IDomainEvent;
```

Create `src/Services/User/TaskFlow.User.Domain/Events/UserDeactivatedDomainEvent.cs`:

```csharp
using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.User.Domain.Events;

/// <summary>
/// Domain event raised when a user is deactivated
/// </summary>
public sealed record UserDeactivatedDomainEvent(Guid UserId) : IDomainEvent;
```

Create `src/Services/User/TaskFlow.User.Domain/Exceptions/UserDomainException.cs`:

```csharp
using TaskFlow.BuildingBlocks.Common.Exceptions;

namespace TaskFlow.User.Domain.Exceptions;

/// <summary>
/// Exception for user domain rule violations
/// </summary>
public sealed class UserDomainException : DomainException
{
    public UserDomainException(string message) : base(message)
    {
    }

    public UserDomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
```

### Step 3: Run the Scaffolding Script

```powershell
.\scripts\scaffold.ps1 -Entity "src/Services/User/TaskFlow.User.Domain/Entities/UserEntity.cs"
```

### Step 4: Review Generated Files

The script generates ~20 files. Here are the key ones:

#### Application/DTOs/UserDto.cs
```csharp
public sealed record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public DateTime DateOfBirth { get; init; }
    public UserStatus Status { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
```

#### Application/Features/Users/Commands/CreateUser/CreateUserCommandHandler.cs
```csharp
public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IUserRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(
        IUserRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task<Result<Guid>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = UserEntity.Create(
            request.Email,
            request.FirstName,
            request.LastName,
            request.DateOfBirth
        );

        await _repository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}
```

#### API/Controllers/UsersController.cs
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ApiController
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        var query = new GetAllUsersQuery();
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return HandleResult(result, id => CreatedAtAction(
            nameof(GetUserById),
            new { id },
            new { id }));
    }

    // ... other endpoints
}
```

### Step 5: Complete the TODOs

#### Update CreateUserCommandValidator.cs

```csharp
public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(256)
            .WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(100)
            .WithMessage("First name must not exceed 100 characters")
            .Matches("^[a-zA-Z ]+$")
            .WithMessage("First name can only contain letters and spaces");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(100)
            .WithMessage("Last name must not exceed 100 characters")
            .Matches("^[a-zA-Z ]+$")
            .WithMessage("Last name can only contain letters and spaces");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .WithMessage("Date of birth is required")
            .LessThan(DateTime.Today)
            .WithMessage("Date of birth must be in the past")
            .Must(BeAtLeast18YearsOld)
            .WithMessage("User must be at least 18 years old");
    }

    private bool BeAtLeast18YearsOld(DateTime dateOfBirth)
    {
        return dateOfBirth <= DateTime.Today.AddYears(-18);
    }
}
```

#### Update UserEntityConfiguration.cs

```csharp
public sealed class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.DateOfBirth)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.LastLoginAt)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.CreatedAt);

        // Ignore domain events
        builder.Ignore(x => x.DomainEvents);
    }
}
```

### Step 6: Register Services

#### Update DependencyInjection.cs

```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Database
    var provider = configuration.GetValue<string>("DatabaseProvider") ?? "PostgreSQL";
    var connectionString = configuration.GetConnectionString("UserDb");

    if (provider == "SqlServer")
    {
        services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(connectionString));
    }
    else
    {
        services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(connectionString));
    }

    // Repositories
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IUnitOfWork, UnitOfWork>();

    return services;
}
```

#### Update Program.cs

```csharp
using FluentValidation;
using Mapster;
using TaskFlow.User.Application.Mappings;
using TaskFlow.User.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(TaskFlow.User.Application.DTOs.UserDto).Assembly);
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(TaskFlow.User.Application.DTOs.UserDto).Assembly);

// Mapster
UserMappingConfig.Configure();

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TaskFlow.User.Infrastructure.Persistence.UserDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
```

#### Create/Update UserDbContext.cs

```csharp
using Microsoft.EntityFrameworkCore;
using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.User.Domain.Entities;

namespace TaskFlow.User.Infrastructure.Persistence;

public sealed class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Get domain events before saving
        var domainEvents = ChangeTracker
            .Entries<Entity<Guid>>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .SelectMany(e =>
            {
                var events = e.DomainEvents.ToList();
                e.ClearDomainEvents();
                return events;
            })
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // TODO: Publish domain events here (implement event dispatcher)

        return result;
    }
}
```

### Step 7: Create Migration

```bash
cd src/Services/User/TaskFlow.User.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../TaskFlow.User.API
dotnet ef database update --startup-project ../TaskFlow.User.API
```

### Step 8: Test the Implementation

#### Run Unit Tests

```bash
cd tests/UnitTests/TaskFlow.User.UnitTests
dotnet test
```

#### Run the API

```bash
cd src/Services/User/TaskFlow.User.API
dotnet run
```

#### Test with curl

```bash
# Create user
curl -X POST http://localhost:5000/api/v1/Users \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "dateOfBirth": "1990-01-15"
  }'

# Get all users
curl http://localhost:5000/api/v1/Users

# Get user by ID
curl http://localhost:5000/api/v1/Users/123e4567-e89b-12d3-a456-426614174000

# Update user
curl -X PUT http://localhost:5000/api/v1/Users/123e4567-e89b-12d3-a456-426614174000 \
  -H "Content-Type: application/json" \
  -d '{
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Smith",
    "dateOfBirth": "1990-01-15"
  }'

# Delete user
curl -X DELETE http://localhost:5000/api/v1/Users/123e4567-e89b-12d3-a456-426614174000
```

### Step 9: Test with Swagger

1. Navigate to http://localhost:5000/swagger
2. Explore the generated API documentation
3. Try out the endpoints directly from Swagger UI

## Result

You now have a complete, production-ready User Service with:

✅ **Domain Layer**
- UserEntity with business logic
- Domain events
- Domain exceptions
- Enums

✅ **Application Layer**
- CQRS commands and queries
- Validators with FluentValidation
- DTOs
- Repository interfaces

✅ **Infrastructure Layer**
- Repository implementation
- EF Core configuration
- Database migrations
- UnitOfWork

✅ **API Layer**
- RESTful controller
- 5 endpoints (GET, POST, PUT, DELETE)
- Swagger documentation
- Health checks

✅ **Tests**
- Unit tests for domain
- Integration tests for API
- Test infrastructure ready

## Time Saved

**Manual Implementation:** ~8-10 hours
**With Scaffolding:** ~30 minutes (mostly filling in TODOs)

**Time Saved:** ~7-9 hours per entity! 🎉

## Next Steps

1. Add custom queries (e.g., GetByEmail, GetActiveUsers)
2. Implement domain event handlers
3. Add more comprehensive tests
4. Add authentication/authorization
5. Deploy with Docker

---

**This example demonstrates the power of the TaskFlow scaffolding script!**
