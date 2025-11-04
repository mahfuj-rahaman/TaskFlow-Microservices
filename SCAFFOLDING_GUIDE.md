# 🏗️ Service Scaffolding Guide

## Overview

This guide explains how to use the service scaffolding script to quickly create new microservices in the TaskFlow system with proper Clean Architecture structure and all necessary boilerplate code.

## Quick Start

### Complete Workflow (3 Scripts, 3 Minutes)

```bash
# 1. Scaffold the service structure (boilerplate only)
./scripts/scaffold-service.sh Identity

# 2. Create feature specification (AI asks questions)
./scripts/ai-scaffold.sh UserManagement Identity

# 3. Generate feature code from spec (AI generates code)
./scripts/generate-from-spec.sh UserManagement Identity

# Done! You now have a complete working microservice with:
# - Clean Architecture structure ✅
# - Domain entities with business rules ✅
# - CQRS commands and queries ✅
# - REST API controllers ✅
# - Unit and integration tests ✅
```

### Or Just Scaffold (Manual Coding)

```bash
# Create empty service structure, then code manually
./scripts/scaffold-service.sh Identity
./scripts/scaffold-service.sh Payment
./scripts/scaffold-service.sh Notification
```

## What Gets Created

The scaffolding script creates a complete microservice with:

### 📁 Folder Structure
```
src/Services/ServiceName/
├── TaskFlow.ServiceName.Domain/
│   ├── Entities/          (Domain entities - Aggregate Roots)
│   ├── Events/            (Domain events)
│   ├── Exceptions/        (Domain-specific exceptions)
│   └── Enums/             (Domain enumerations)
├── TaskFlow.ServiceName.Application/
│   ├── Features/          (CQRS Commands & Queries organized by feature)
│   └── Common/
│       ├── Interfaces/    (Repository interfaces)
│       ├── Mappings/      (Mapster mappings)
│       └── Behaviors/     (MediatR pipeline behaviors)
├── TaskFlow.ServiceName.Infrastructure/
│   ├── Persistence/
│   │   ├── Configurations/ (EF Core entity configurations)
│   │   └── Repositories/   (Repository implementations)
│   └── Services/          (External service integrations)
└── TaskFlow.ServiceName.API/
    ├── Controllers/       (REST API controllers)
    ├── Middleware/        (HTTP middleware)
    ├── Program.cs         (Application entry point)
    ├── appsettings.json   (Configuration)
    └── Dockerfile         (Docker configuration)
```

### 📦 Project Files

All 4 Clean Architecture layer projects with:

1. **Domain Project** (.NET 8.0 Class Library)
   - References to BuildingBlocks.Common
   - MediatR package
   - No external dependencies

2. **Application Project** (.NET 8.0 Class Library)
   - References to Domain layer
   - References to BuildingBlocks.CQRS
   - FluentValidation, Mapster, MediatR packages

3. **Infrastructure Project** (.NET 8.0 Class Library)
   - References to Application layer
   - References to BuildingBlocks.EventBus and Messaging
   - EF Core, MassTransit, Polly, Redis packages

4. **API Project** (.NET 8.0 Web API)
   - References to Infrastructure layer
   - Serilog, Swagger, JWT Authentication packages
   - Complete Program.cs with logging
   - appsettings.json with database, logging, messaging config

### ✅ Solution Integration

- All 4 projects are automatically added to TaskFlow.sln
- Proper project references set up
- BuildingBlocks references configured
- Ready to build immediately

### 🐳 Docker Support

- Dockerfile created for the API project
- Multi-stage build configuration
- Optimized for production deployment

## What It Does NOT Create

The scaffolding script creates **only boilerplate structure**, not business logic:

❌ No domain entities (you add these based on your feature specs)
❌ No controllers (you add these as you implement features)
❌ No database context (you create this when defining your data model)
❌ No repositories (you implement these for your specific entities)
❌ No command/query handlers (you add these per feature)

This is **intentional** - the script provides the foundation, and you or AI agents implement the business logic based on feature specifications.

## Workflow: From Scaffolding to Implementation

### Step 1: Scaffold the Service Structure

```bash
./scripts/scaffold-service.sh Identity
```

This creates the complete microservice structure and adds it to the solution with **boilerplate only** (no business logic).

### Step 2: Define Your Feature Specification

Create a feature specification document describing what the service should do:

```bash
# Create a feature specification with AI assistance
./scripts/ai-scaffold.sh UserManagement Identity
```

This creates `docs/features/UserManagement_feature.md` where you define:
- Domain model (entities, properties, business rules)
- Use cases (commands and queries)
- API endpoints
- Validation rules
- Integration requirements

The script will ask you questions and generate both:
- `UserManagement_feature.md` - Human-readable specification
- `UserManagement_data.json` - Machine-readable data for code generation

### Step 3: Generate Feature Code (Optional - AI-Powered)

**Option A: AI Code Generation** (Recommended for speed)

```bash
# Automatically generate all feature code from specification
./scripts/generate-from-spec.sh UserManagement Identity
```

This reads your specification and generates:
- Domain entities with business rules
- Commands and queries (CQRS)
- Command/Query handlers
- Validators (FluentValidation)
- DTOs and mappings
- Repository interface and implementation
- EF Core configuration
- REST API controller
- Unit and integration tests

**Option B: Manual Implementation** (For custom control)

Implement the domain layer manually (see Step 4 below).

### Step 4: Implement Domain Layer (If Not Using AI Generation)

Based on your feature spec, create domain entities in:
- `src/Services/Identity/TaskFlow.Identity.Domain/Entities/`

Example:
```csharp
// IdentityEntity.cs
public sealed class IdentityEntity : AggregateRoot<Guid>
{
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }

    public static IdentityEntity Create(string email, string password)
    {
        // Business logic here
        return new IdentityEntity { ... };
    }
}
```

### Step 4: Implement Application Layer

Create commands, queries, and handlers in:
- `src/Services/Identity/TaskFlow.Identity.Application/Features/`

Example structure:
```
Features/
└── Identity/
    ├── Commands/
    │   ├── RegisterIdentity/
    │   │   ├── RegisterIdentityCommand.cs
    │   │   ├── RegisterIdentityHandler.cs
    │   │   └── RegisterIdentityValidator.cs
    │   └── ...
    ├── Queries/
    │   └── GetIdentityById/
    │       ├── GetIdentityByIdQuery.cs
    │       └── GetIdentityByIdHandler.cs
    └── DTOs/
        └── IdentityDto.cs
```

### Step 5: Implement Infrastructure Layer

1. Create DbContext:
```csharp
// IdentityDbContext.cs in Infrastructure/Persistence/
public class IdentityDbContext : DbContext
{
    public DbSet<IdentityEntity> Identities { get; set; }
}
```

2. Create Repositories:
```csharp
// IdentityRepository.cs in Infrastructure/Persistence/Repositories/
public class IdentityRepository : IIdentityRepository
{
    // Implementation
}
```

3. Configure entities:
```csharp
// IdentityConfiguration.cs in Infrastructure/Persistence/Configurations/
public class IdentityConfiguration : IEntityTypeConfiguration<IdentityEntity>
{
    // EF Core configuration
}
```

### Step 6: Implement API Layer

1. Create Controllers:
```csharp
// IdentitiesController.cs in API/Controllers/
[ApiController]
[Route("api/v1/[controller]")]
public class IdentitiesController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterIdentityCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
```

2. Register services in Program.cs:
```csharp
// Add to Program.cs
builder.Services.AddDbContext<IdentityDbContext>(...);
builder.Services.AddScoped<IIdentityRepository, IdentityRepository>();
builder.Services.AddMediatR(...);
builder.Services.AddFluentValidation(...);
```

### Step 7: Create Database Migration

```bash
cd src/Services/Identity/TaskFlow.Identity.API
dotnet ef migrations add InitialCreate --project ../TaskFlow.Identity.Infrastructure
dotnet ef database update --project ../TaskFlow.Identity.Infrastructure
```

### Step 8: Test and Run

```bash
# Build
dotnet build

# Run
dotnet run --project src/Services/Identity/TaskFlow.Identity.API

# Or with Docker
docker build -t taskflow-identity -f src/Services/Identity/TaskFlow.Identity.API/Dockerfile .
docker run -p 5000:80 taskflow-identity
```

## Use AI Code Generation (Optional)

After scaffolding, you can use AI to generate feature implementations:

```bash
# Generate a complete feature based on specification
./scripts/generate-from-spec.sh Identity User
```

This reads `docs/features/Identity_data.json` and generates:
- Domain entities
- Commands and queries
- Handlers and validators
- Repositories
- Controllers
- Tests

## Example: Creating an Identity Service

```bash
# 1. Scaffold the service
./scripts/scaffold-service.sh Identity

# 2. Verify it was added to solution
dotnet sln list | grep Identity

# 3. Build to ensure everything works
dotnet build

# 4. (Optional) Create feature specification
./scripts/ai-scaffold.sh Identity User

# 5. Implement your business logic
# Add entities, commands, queries, etc.

# 6. Create migrations
cd src/Services/Identity/TaskFlow.Identity.API
dotnet ef migrations add InitialCreate --project ../TaskFlow.Identity.Infrastructure

# 7. Run the service
dotnet run
```

## Multiple Services

You can scaffold multiple services:

```bash
./scripts/scaffold-service.sh Identity
./scripts/scaffold-service.sh Payment
./scripts/scaffold-service.sh Notification
./scripts/scaffold-service.sh Analytics
```

Each service is:
- Completely independent
- Has its own database
- Has its own API
- Can be deployed separately
- Communicates via events (RabbitMQ) or HTTP

## Configuration

Each scaffolded service includes:

### Database Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=TaskFlow_ServiceName;Username=postgres;Password=postgres"
  }
}
```

### Logging (Serilog + Seq)
```json
{
  "Seq": {
    "ServerUrl": "http://localhost:5341"
  }
}
```

### Messaging (RabbitMQ)
```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  }
}
```

### Caching (Redis)
```json
{
  "Redis": {
    "Configuration": "localhost:6379"
  }
}
```

### Authentication (JWT)
```json
{
  "JwtSettings": {
    "Secret": "your-secret-key-min-32-characters-long",
    "Issuer": "TaskFlow",
    "Audience": "TaskFlow",
    "ExpirationInMinutes": 1440
  }
}
```

## Clean Architecture Principles

The scaffolded structure follows Clean Architecture:

```
┌─────────────────────────────────────┐
│          API Layer                  │  ← HTTP/Controllers
│  (Presentation)                     │
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│     Infrastructure Layer            │  ← DbContext, Repositories
│  (External Concerns)                │     External Services
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│     Application Layer               │  ← Use Cases (CQRS)
│  (Business Logic)                   │     Handlers, Validators
└─────────────┬───────────────────────┘
              │
┌─────────────▼───────────────────────┐
│        Domain Layer                 │  ← Entities, Events
│  (Core Business Rules)              │     Exceptions, Enums
└─────────────────────────────────────┘
```

**Dependency Rule**: Inner layers don't know about outer layers
- Domain has no dependencies
- Application depends only on Domain
- Infrastructure depends on Application (implements interfaces)
- API depends on Infrastructure (composition root)

## Troubleshooting

### Service already exists
```bash
# Remove existing service first
rm -rf src/Services/ServiceName

# Or let the script remove it
./scripts/scaffold-service.sh ServiceName
# Answer 'y' when prompted to recreate
```

### Build errors after scaffolding
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Projects not in solution
```bash
# Manually add if needed
dotnet sln add src/Services/ServiceName/TaskFlow.ServiceName.Domain/*.csproj
dotnet sln add src/Services/ServiceName/TaskFlow.ServiceName.Application/*.csproj
dotnet sln add src/Services/ServiceName/TaskFlow.ServiceName.Infrastructure/*.csproj
dotnet sln add src/Services/ServiceName/TaskFlow.ServiceName.API/*.csproj
```

## Best Practices

### 1. One Service, One Responsibility
Each microservice should handle one bounded context.

### 2. Database Per Service
Each service has its own database - no shared databases.

### 3. Communicate via Events
Services communicate through domain events (RabbitMQ).

### 4. API Gateway Pattern
All client requests go through the API Gateway.

### 5. Start Simple
Scaffold the structure, implement one feature at a time.

## Next Steps

After scaffolding:

1. ✅ Read your feature specification document
2. ✅ Implement domain entities with business rules
3. ✅ Create commands and queries (CQRS)
4. ✅ Implement handlers with validation
5. ✅ Create repositories
6. ✅ Add controllers with proper routing
7. ✅ Write unit and integration tests
8. ✅ Configure DbContext and migrations
9. ✅ Set up authentication/authorization
10. ✅ Add API documentation (Swagger)
11. ✅ Configure logging and monitoring
12. ✅ Deploy to Docker

## Additional Resources

- **Clean Architecture**: See `CLAUDE.md` for detailed architecture documentation
- **CQRS Pattern**: See `docs/CODE_GENERATION_SYSTEM.md`
- **Feature Specifications**: See `docs/features/` for examples
- **AI Code Generation**: See `QUICKSTART_CODE_GENERATION.md`

---

**Ready to build microservices!** 🚀

Start with: `./scripts/scaffold-service.sh YourServiceName`
