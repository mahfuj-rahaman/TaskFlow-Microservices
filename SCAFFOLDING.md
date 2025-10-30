# TaskFlow Microservices - Scaffolding Guide

## Overview

The TaskFlow scaffolding script automatically generates production-ready CRUD implementations following Clean Architecture, DDD, and CQRS patterns. Simply create your domain entity, run the script, and get a complete feature implementation.

## Features

✅ **Complete CRUD Generation**
- DTOs with Mapster configuration
- CQRS Commands (Create, Update, Delete)
- CQRS Queries (GetById, GetAll)
- FluentValidation validators
- Repository interface and implementation
- EF Core entity configuration
- RESTful API controller
- Unit and integration tests

✅ **Clean Architecture**
- Proper layer separation
- Dependency inversion
- Domain-driven design

✅ **Production Ready**
- Error handling with Result pattern
- Async/await throughout
- Cancellation token support
- Comprehensive validation
- Best practices applied

## Quick Start

### 1. Create Your Domain Entity

First, create your domain entity in the Domain layer:

```csharp
// src/Services/User/TaskFlow.User.Domain/Entities/UserEntity.cs
using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.User.Domain.Entities;

public sealed class UserEntity : AggregateRoot<Guid>
{
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public bool IsActive { get; private set; }

    // Private constructor for EF Core
    private UserEntity() { }

    // Factory method (will be completed after scaffolding)
    public static UserEntity Create(
        string email,
        string firstName,
        string lastName,
        DateTime dateOfBirth)
    {
        // TODO: Add validation and business logic
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return user;
    }

    // Domain methods
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
```

### 2. Run the Scaffolding Script

```powershell
# Basic usage
.\scripts\scaffold.ps1 -EntityPath "src/Services/User/TaskFlow.User.Domain/Entities/UserEntity.cs"

# Or using alias
.\scripts\scaffold.ps1 -Entity "src/Services/User/TaskFlow.User.Domain/Entities/UserEntity.cs"

# Overwrite existing files
.\scripts\scaffold.ps1 -Entity "src/Services/User/TaskFlow.User.Domain/Entities/UserEntity.cs" -Force
```

### 3. Complete the TODOs

The script generates files with TODO comments where you need to add business logic:

```csharp
// In CommandHandler
var entity = UserEntity.Create(
    request.Email,
    request.FirstName,
    request.LastName,
    request.DateOfBirth
);

// In Validator
RuleFor(x => x.Email)
    .NotEmpty()
    .WithMessage("Email is required")
    .EmailAddress()
    .WithMessage("Invalid email format");

// In Configuration
builder.Property(x => x.Email)
    .IsRequired()
    .HasMaxLength(256);

builder.HasIndex(x => x.Email)
    .IsUnique();
```

### 4. Register Services

Update `DependencyInjection.cs`:

```csharp
services.AddScoped<IUserRepository, UserRepository>();
```

Update `Program.cs`:

```csharp
UserMappingConfig.Configure();
```

Update `DbContext`:

```csharp
public DbSet<UserEntity> Users => Set<UserEntity>();

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
}
```

### 5. Create Migration

```bash
cd src/Services/User/TaskFlow.User.Infrastructure
dotnet ef migrations add AddUserEntity --startup-project ../TaskFlow.User.API
dotnet ef database update --startup-project ../TaskFlow.User.API
```

### 6. Test Your Implementation

```bash
# Run tests
dotnet test

# Run the API
cd src/Services/User/TaskFlow.User.API
dotnet run

# Test endpoints
curl http://localhost:5000/api/v1/Users
```

## What Gets Generated

### Application Layer

#### DTOs (1 file)
- `UserDto.cs` - Data transfer object with all properties

#### Interfaces (1 file)
- `IUserRepository.cs` - Repository interface with CRUD methods

#### Mappings (1 file)
- `UserMappingConfig.cs` - Mapster configuration

#### Commands (6 files)
- `CreateUserCommand.cs`
- `CreateUserCommandHandler.cs`
- `CreateUserCommandValidator.cs`
- `UpdateUserCommand.cs`
- `UpdateUserCommandHandler.cs`
- `DeleteUserCommand.cs`
- `DeleteUserCommandHandler.cs`

#### Queries (4 files)
- `GetUserByIdQuery.cs`
- `GetUserByIdQueryHandler.cs`
- `GetAllUsersQuery.cs`
- `GetAllUsersQueryHandler.cs`

### Infrastructure Layer

#### Repositories (1 file)
- `UserRepository.cs` - Complete repository implementation

#### Configurations (1 file)
- `UserEntityConfiguration.cs` - EF Core Fluent API configuration

### API Layer

#### Controllers (2 files)
- `UsersController.cs` - RESTful API with 5 endpoints
- `ApiController.cs` - Base controller (if not exists)

### Tests

#### Unit Tests (1 file)
- `UserEntityTests.cs` - Domain entity tests

#### Integration Tests (1 file)
- `UsersControllerTests.cs` - API endpoint tests

### Documentation

#### Summary (1 file)
- `SCAFFOLD-SUMMARY-UserEntity.md` - Complete scaffolding guide

**Total:** ~20 files generated

## Generated API Endpoints

The scaffolding creates a complete RESTful API:

### GET /api/v1/Users
Get all users
```bash
curl http://localhost:5000/api/v1/Users
```

### GET /api/v1/Users/{id}
Get user by ID
```bash
curl http://localhost:5000/api/v1/Users/123e4567-e89b-12d3-a456-426614174000
```

### POST /api/v1/Users
Create new user
```bash
curl -X POST http://localhost:5000/api/v1/Users \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "dateOfBirth": "1990-01-01"
  }'
```

### PUT /api/v1/Users/{id}
Update user
```bash
curl -X PUT http://localhost:5000/api/v1/Users/123e4567-e89b-12d3-a456-426614174000 \
  -H "Content-Type: application/json" \
  -d '{
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "dateOfBirth": "1990-01-01"
  }'
```

### DELETE /api/v1/Users/{id}
Delete user
```bash
curl -X DELETE http://localhost:5000/api/v1/Users/123e4567-e89b-12d3-a456-426614174000
```

## Architecture Generated

```
User Service
├── Domain Layer
│   └── Entities/UserEntity.cs (Your entity)
│
├── Application Layer
│   ├── DTOs/
│   │   └── UserDto.cs
│   ├── Features/Users/
│   │   ├── Commands/
│   │   │   ├── CreateUser/
│   │   │   ├── UpdateUser/
│   │   │   └── DeleteUser/
│   │   └── Queries/
│   │       ├── GetUserById/
│   │       └── GetAllUsers/
│   ├── Interfaces/
│   │   └── IUserRepository.cs
│   └── Mappings/
│       └── UserMappingConfig.cs
│
├── Infrastructure Layer
│   ├── Repositories/
│   │   └── UserRepository.cs
│   └── Persistence/Configurations/
│       └── UserEntityConfiguration.cs
│
└── API Layer
    └── Controllers/
        └── UsersController.cs
```

## Script Features

### Automatic Detection
- ✅ Extracts entity name from file
- ✅ Determines service name from namespace
- ✅ Detects ID type (Guid, int, etc.)
- ✅ Parses entity properties
- ✅ Identifies enums and value objects

### Smart Generation
- ✅ Creates proper folder structure
- ✅ Generates correct namespaces
- ✅ Applies naming conventions
- ✅ Handles plural/singular names
- ✅ Includes XML documentation

### Safety Features
- ✅ Validates entity file exists
- ✅ Prevents overwriting (unless -Force)
- ✅ Creates directories automatically
- ✅ UTF-8 encoding
- ✅ Clear error messages

### Output
- ✅ Color-coded messages
- ✅ Progress indicators
- ✅ Success confirmations
- ✅ Summary document

## Advanced Usage

### Generate for Different ID Types

The script automatically detects the ID type from your entity:

```csharp
// Guid ID
public sealed class UserEntity : AggregateRoot<Guid>

// Int ID
public sealed class ProductEntity : AggregateRoot<int>

// Long ID
public sealed class OrderEntity : AggregateRoot<long>
```

### Force Overwrite Existing Files

```powershell
.\scripts\scaffold.ps1 -Entity "path/to/Entity.cs" -Force
```

### Multiple Entities

Run the script once for each entity:

```powershell
.\scripts\scaffold.ps1 -Entity "src/Services/User/TaskFlow.User.Domain/Entities/UserEntity.cs"
.\scripts\scaffold.ps1 -Entity "src/Services/User/TaskFlow.User.Domain/Entities/ProfileEntity.cs"
.\scripts\scaffold.ps1 -Entity "src/Services/User/TaskFlow.User.Domain/Entities/AddressEntity.cs"
```

## Customization Guide

### 1. Customize Command Properties

Edit the generated Command file:

```csharp
public sealed record CreateUserCommand : IRequest<Result<Guid>>
{
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required DateTime DateOfBirth { get; init; }

    // Add custom properties
    public string? PhoneNumber { get; init; }
    public string? Address { get; init; }
}
```

### 2. Add Custom Validation Rules

Edit the Validator file:

```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-zA-Z ]*$")
            .WithMessage("First name can only contain letters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.Today)
            .WithMessage("Date of birth must be in the past")
            .Must(BeAtLeast18YearsOld)
            .WithMessage("Must be at least 18 years old");
    }

    private bool BeAtLeast18YearsOld(DateTime dateOfBirth)
    {
        return dateOfBirth <= DateTime.Today.AddYears(-18);
    }
}
```

### 3. Add Custom Query Filters

Create additional query files:

```csharp
// GetUserByEmailQuery.cs
public sealed record GetUserByEmailQuery(string Email) : IRequest<UserDto?>;

// GetUserByEmailQueryHandler.cs
public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, UserDto?>
{
    private readonly IUserRepository _repository;

    public GetUserByEmailQueryHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserDto?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByEmailAsync(request.Email, cancellationToken);
        return user?.Adapt<UserDto>();
    }
}
```

### 4. Extend Repository Interface

Add custom methods to the repository interface:

```csharp
public interface IUserRepository
{
    // Generated methods
    Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    // ...

    // Custom methods
    Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserEntity>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
```

### 5. Add Custom Endpoints

Extend the controller:

```csharp
[HttpGet("by-email/{email}")]
public async Task<IActionResult> GetUserByEmail(string email, CancellationToken cancellationToken)
{
    var query = new GetUserByEmailQuery(email);
    var result = await _sender.Send(query, cancellationToken);

    if (result is null)
        return NotFound(new { message = "User not found" });

    return Ok(result);
}

[HttpGet("active")]
public async Task<IActionResult> GetActiveUsers(CancellationToken cancellationToken)
{
    var query = new GetActiveUsersQuery();
    var result = await _sender.Send(query, cancellationToken);
    return Ok(result);
}
```

## Patterns Implemented

### Clean Architecture
- Domain Layer: Entities, Value Objects, Events
- Application Layer: Use Cases (Commands/Queries)
- Infrastructure Layer: Data Access
- API Layer: Controllers, DTOs

### Domain-Driven Design
- Aggregate Roots
- Domain Events
- Domain Exceptions
- Encapsulation

### CQRS (Command Query Responsibility Segregation)
- Separate Commands (writes)
- Separate Queries (reads)
- MediatR pipeline

### Repository Pattern
- Interface in Application Layer
- Implementation in Infrastructure Layer
- Unit of Work for transactions

### Result Pattern
- Functional error handling
- No exception throwing for business errors
- Explicit success/failure states

## Troubleshooting

### Issue: "Entity file not found"

**Solution:** Ensure the path is relative to the project root:
```powershell
# Wrong
.\scripts\scaffold.ps1 -Entity "UserEntity.cs"

# Correct
.\scripts\scaffold.ps1 -Entity "src/Services/User/TaskFlow.User.Domain/Entities/UserEntity.cs"
```

### Issue: "Could not determine service name"

**Solution:** Ensure your namespace follows the pattern:
```csharp
namespace TaskFlow.ServiceName.Domain.Entities;
```

### Issue: "Files already exist"

**Solution:** Use `-Force` to overwrite:
```powershell
.\scripts\scaffold.ps1 -Entity "path/to/Entity.cs" -Force
```

### Issue: "Compilation errors after scaffolding"

**Solution:** Complete the TODOs and register services:
1. Implement domain Create method
2. Register repository in DI
3. Configure Mapster
4. Add DbSet to DbContext
5. Create migration

## Best Practices

### 1. Domain Entity Design
```csharp
✅ Good: Use factory method
public static UserEntity Create(...) { }

✅ Good: Private setters
public string Email { get; private set; }

✅ Good: Domain methods
public void Activate() { }
public void ChangeEmail(string email) { }

❌ Bad: Public setters
public string Email { get; set; }

❌ Bad: Anemic domain model
```

### 2. Validation Strategy
```csharp
✅ Domain validation: Business rules
if (age < 18) throw new UserDomainException("...");

✅ Application validation: Input validation
RuleFor(x => x.Email).NotEmpty().EmailAddress();

❌ Controller validation: Keep controllers thin
```

### 3. Repository Methods
```csharp
✅ Good: Specific methods
Task<User?> GetByEmailAsync(string email);

✅ Good: Read-only for queries
Task<IReadOnlyList<User>> GetAllAsync();

❌ Bad: Generic get methods
Task<User> Get(Expression<Func<User, bool>> predicate);
```

### 4. Error Handling
```csharp
✅ Good: Return Result
return Result.Failure(new Error("User.NotFound", "User not found"));

✅ Good: Throw domain exceptions
throw new UserDomainException("Invalid age");

❌ Bad: Return null without Result wrapper
❌ Bad: Throw generic exceptions
```

## Examples

### Example 1: User Service

```bash
# 1. Create entity
# src/Services/User/TaskFlow.User.Domain/Entities/UserEntity.cs

# 2. Run scaffold
.\scripts\scaffold.ps1 -Entity "src/Services/User/TaskFlow.User.Domain/Entities/UserEntity.cs"

# 3. Complete implementation
# - Add validation rules
# - Implement domain methods
# - Create migration
# - Run tests
```

### Example 2: Product Service

```bash
# 1. Create entity with decimal price
# src/Services/Product/TaskFlow.Product.Domain/Entities/ProductEntity.cs

# 2. Run scaffold
.\scripts\scaffold.ps1 -Entity "src/Services/Product/TaskFlow.Product.Domain/Entities/ProductEntity.cs"

# 3. Customize
# - Add price validation (must be > 0)
# - Add stock management methods
# - Add GetLowStockProducts query
```

### Example 3: Order Service

```bash
# 1. Create entity with complex properties
# src/Services/Order/TaskFlow.Order.Domain/Entities/OrderEntity.cs

# 2. Run scaffold
.\scripts\scaffold.ps1 -Entity "src/Services/Order/TaskFlow.Order.Domain/Entities/OrderEntity.cs"

# 3. Extend
# - Add order items collection
# - Add status transitions
# - Add GetOrdersByCustomer query
# - Add order total calculation
```

## Migration Guide

### From Manual Implementation

If you have an existing domain entity and want to use scaffolding:

1. **Backup your current implementation**
2. **Run scaffold with `-Force` flag**
3. **Merge your custom logic** into generated files
4. **Update tests** to match new structure

### To New Service

When creating a completely new service:

1. **Create the service structure** (if not exists)
2. **Create your domain entity**
3. **Run scaffold** to generate all layers
4. **Complete TODOs** and customize
5. **Test thoroughly**

## Contributing

To improve the scaffolding script:

1. Edit `scripts/scaffold.ps1`
2. Test with various entity types
3. Update this documentation
4. Submit pull request

## Version History

### v1.0.0 (2025-10-30)
- Initial release
- Complete CRUD generation
- Clean Architecture support
- CQRS pattern implementation
- Repository pattern
- Result pattern
- Unit and integration test templates

## License

MIT License - Part of TaskFlow Microservices

## Support

For issues or questions:
- GitHub Issues: https://github.com/mahfuj-rahaman/TaskFlow-Microservices/issues
- Documentation: SCAFFOLDING.md (this file)
- Examples: See `tests/` directory

---

**Generated with ❤️ by TaskFlow Team**
**Powered by Claude Code**
