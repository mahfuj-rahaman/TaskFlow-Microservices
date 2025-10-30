# 🤖 Claude AI Context File

**Project**: TaskFlow Microservices
**Last Updated**: 2025-10-31
**Purpose**: Complete context preservation for Claude AI sessions

---

## 🎯 Project Overview

**TaskFlow** is a production-ready microservices architecture implementing:
- Clean Architecture (4 layers)
- Domain-Driven Design (DDD)
- CQRS Pattern
- Event-Driven Architecture
- AI-Powered Code Generation System

**Repository**: https://github.com/mahfuj-rahaman/TaskFlow-Microservices.git
**Branch**: main
**Current Commit**: 8121a27

---

## 🏗️ Architecture

### Technology Stack

**Backend**:
- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core
- MediatR (CQRS)
- FluentValidation
- Mapster (Object Mapping)
- Serilog (Logging)
- Polly (Resilience)

**Infrastructure**:
- Docker & Docker Compose
- PostgreSQL (Primary Database)
- RabbitMQ (Message Bus)
- Redis (Caching)
- Seq (Log Aggregation)
- Jaeger (Distributed Tracing)

**Testing**:
- xUnit
- FluentAssertions
- Moq
- Testcontainers

### Microservices

1. **User Service** - User management and authentication
2. **Catalog Service** - Product catalog management
3. **Order Service** - Order processing
4. **Notification Service** - Notifications and alerts
5. **API Gateway** - Entry point for all clients

### Clean Architecture Layers

Each microservice follows this structure:

```
TaskFlow.{Service}.Domain/          # Core business logic
├── Entities/                       # Domain entities (Aggregate Roots)
├── Events/                         # Domain events
├── Exceptions/                     # Domain-specific exceptions
└── Enums/                          # Domain enumerations

TaskFlow.{Service}.Application/     # Use cases and business rules
├── Features/                       # Organized by feature (CQRS)
│   └── {Feature}s/
│       ├── Commands/               # Write operations
│       ├── Queries/                # Read operations
│       └── DTOs/                   # Data Transfer Objects
├── Common/
│   ├── Interfaces/                 # Repository interfaces
│   ├── Mappings/                   # Object mappings
│   └── Behaviors/                  # MediatR pipeline behaviors

TaskFlow.{Service}.Infrastructure/  # External concerns
├── Persistence/
│   ├── Configurations/             # EF Core configurations
│   ├── Repositories/               # Repository implementations
│   └── {Service}DbContext.cs       # Database context
└── Services/                       # External service integrations

TaskFlow.{Service}.API/             # Presentation layer
├── Controllers/                    # REST API endpoints
├── Middleware/                     # HTTP middleware
└── Program.cs                      # Application entry point
```

---

## ⚡ AI-Powered Code Generation System

### Overview

**Status**: ✅ Production Ready
**Purpose**: Generate complete features in ~2 minutes (vs 5 hours manual)
**Generates**: 26+ files per feature across 4 architectural layers

### Core Scripts

Located in `scripts/`

1. **`ai-scaffold.sh`** - Interactive specification creator
   - Creates feature specification documents
   - Asks intelligent questions about requirements
   - Generates `{feature}_feature.md` and `{feature}_data.json`
   - Usage: `./scripts/ai-scaffold.sh <FeatureName> <ServiceName>`

2. **`generate-from-spec.sh`** - Complete code generator
   - Generates all 26+ files for a feature
   - Uses modular generators (domain, application, infrastructure, api, tests)
   - Usage: `./scripts/generate-from-spec.sh <FeatureName> <ServiceName>`

3. **`update-feature.sh`** - Smart update system ⭐ **CRITICAL**
   - Solves the "Update Paradox" (how to update generated code without losing custom changes)
   - Three-layer protection system (see below)
   - Usage: `./scripts/update-feature.sh <FeatureName> <ServiceName> --interactive`

### Modular Generators

Located in `scripts/generators/`

1. **`generate-domain.sh`** - Domain layer
   - Entity (Aggregate Root)
   - Domain Events
   - Domain Exceptions
   - Enumerations

2. **`generate-application.sh`** - Application layer
   - DTOs (Request/Response)
   - Commands (Create, Update, Delete) with Handlers and Validators
   - Queries (GetAll, GetById) with Handlers
   - Repository Interface

3. **`generate-infrastructure.sh`** - Infrastructure layer
   - Repository Implementation
   - EF Core Entity Configuration

4. **`generate-api.sh`** - API layer
   - REST API Controller with full CRUD endpoints

5. **`generate-tests.sh`** - Tests
   - Unit Tests (Domain entity tests)
   - Integration Tests (API endpoint tests)

### The Update Paradox Solution ⭐ IMPORTANT

**Problem**: How do you update generated code without losing custom business logic?

**Solution**: Three-layer protection system

#### Layer 1: [CUSTOM] Markers
Mark any custom code with special markers:

```csharp
// [CUSTOM]
public void MyCustomBusinessLogic()
{
    // Your custom code here
}
// [CUSTOM]
```

The update script will **preserve** any code between `[CUSTOM]` markers.

#### Layer 2: Interactive Diff Preview
Before applying any changes, the update script:
- Shows a diff preview
- Asks for confirmation
- Creates automatic backups (timestamped in `.backups/`)
- Allows selective updates

#### Layer 3: Partial Classes (Optional)
For complete separation:

```csharp
// Generated file: UserEntity.cs
public partial class UserEntity : AggregateRoot<Guid>
{
    // Generated properties and methods
}

// Custom file: UserEntity.Custom.cs
public partial class UserEntity
{
    // [CUSTOM]
    public void CustomBusinessLogic() { }
    // [CUSTOM]
}
```

### Generated Files Per Feature (26+ files)

**Domain Layer** (4 files):
- `{Feature}Entity.cs`
- `{Feature}CreatedDomainEvent.cs`
- `{Feature}UpdatedDomainEvent.cs`
- `{Feature}NotFoundException.cs`

**Application Layer** (14 files):
- DTOs: `{Feature}Dto.cs`, `Create{Feature}Request.cs`, `Update{Feature}Request.cs`
- Commands:
  - `Create{Feature}Command.cs` + Handler + Validator
  - `Update{Feature}Command.cs` + Handler + Validator
  - `Delete{Feature}Command.cs` + Handler + Validator
- Queries:
  - `GetAll{Feature}sQuery.cs` + Handler
  - `Get{Feature}ByIdQuery.cs` + Handler
- `I{Feature}Repository.cs`

**Infrastructure Layer** (2 files):
- `{Feature}Repository.cs`
- `{Feature}Configuration.cs`

**API Layer** (1 file):
- `{Feature}sController.cs`

**Tests** (5 files):
- `{Feature}EntityTests.cs`
- `Create{Feature}CommandTests.cs`
- `Update{Feature}CommandTests.cs`
- `Delete{Feature}CommandTests.cs`
- `{Feature}sControllerTests.cs`

---

## 📚 Documentation Structure

### Quick Start
- **`QUICKSTART_CODE_GENERATION.md`** - Start here! (2-minute guide)

### System Overview
- **`COMPLETE_SYSTEM_SUMMARY.md`** - Complete system documentation
- **`SCAFFOLDING_SYSTEM.md`** - System overview
- **`PROJECT_STATUS.md`** - Current project status

### Guides
- **`docs/CODE_GENERATION_SYSTEM.md`** - Detailed code generation docs
- **`docs/AI_SCAFFOLDING_GUIDE.md`** - AI scaffolding guide
- **`docs/FEATURE_UPDATE_GUIDE.md`** - Update guide
- **`docs/UPDATE_PARADOX_SOLVED.md`** - Update paradox solution ⭐

### Examples
- **`docs/features/Identity_feature_example.md`** - Real-world example

### Migration
- **`MIGRATION_GUIDE.md`** - Old PowerShell → New Shell system
- **`CLEANUP_SUMMARY.md`** - Cleanup report

### Docker
- **`DOCKER.md`** - Docker guide
- **`DOCKER-QUICKSTART.md`** - Docker quick start
- **`DOCKER-FILES.md`** - Docker files reference
- **`DOCKER-TEST.md`** - Docker testing guide

---

## 🎯 Common Workflows

### Generate a New Feature

```bash
# Step 1: Create specification (AI asks questions)
./scripts/ai-scaffold.sh Product Catalog

# Answer questions:
# - What is the main purpose?
# - What properties should it have?
# - What business rules?
# - What operations?

# Step 2: Generate all code (26+ files)
./scripts/generate-from-spec.sh Product Catalog

# Step 3: Review generated code
# Files created in:
# - src/Services/Catalog/TaskFlow.Catalog.Domain/Entities/
# - src/Services/Catalog/TaskFlow.Catalog.Application/Features/Products/
# - src/Services/Catalog/TaskFlow.Catalog.Infrastructure/Persistence/
# - src/Services/Catalog/TaskFlow.Catalog.API/Controllers/
# - tests/
```

### Update an Existing Feature

```bash
# Step 1: Mark your custom code with [CUSTOM] markers
# (if you haven't already)

# Step 2: Update the specification
./scripts/ai-scaffold.sh User User
# Describe changes

# Step 3: Use update script (NOT generate script!)
./scripts/update-feature.sh User User --interactive

# Step 4: Review diffs and accept/reject changes
# Your [CUSTOM] code will be preserved automatically
```

### Add Custom Business Logic

```csharp
// In any generated file
public sealed class UserEntity : AggregateRoot<Guid>
{
    // Generated properties
    public string Email { get; private set; }
    public string FirstName { get; private set; }

    // Generated methods
    public static UserEntity Create(string email, string firstName)
    {
        // Generated validation
        return new UserEntity(Guid.NewGuid()) { Email = email, FirstName = firstName };
    }

    // [CUSTOM] - Add your custom business logic here
    public Result ValidateEmailDomain()
    {
        var allowedDomains = new[] { "company.com", "company.net" };
        var emailDomain = Email.Split('@')[1];

        if (!allowedDomains.Contains(emailDomain))
        {
            return Result.Failure("Email must be from company domain");
        }

        return Result.Success();
    }

    public void PromoteToAdmin()
    {
        if (Status != UserStatus.Active)
        {
            throw new UserInvalidOperationException("Cannot promote inactive user");
        }

        Role = UserRole.Admin;
        RaiseDomainEvent(new UserPromotedDomainEvent(Id));
    }
    // [CUSTOM]
}
```

### Docker Workflow

```bash
# Start all services
docker-compose up -d

# Check logs
docker-compose logs -f [service-name]

# Run tests in Docker
docker-compose -f docker-compose.test.yml up --abort-on-container-exit

# Stop all services
docker-compose down

# Clean everything
docker-compose down -v --remove-orphans
```

---

## 🗂️ Project Structure

```
TaskFlow-Microservices/
├── src/
│   ├── Services/
│   │   ├── User/
│   │   │   ├── TaskFlow.User.Domain/
│   │   │   ├── TaskFlow.User.Application/
│   │   │   ├── TaskFlow.User.Infrastructure/
│   │   │   └── TaskFlow.User.API/
│   │   ├── Catalog/
│   │   ├── Order/
│   │   └── Notification/
│   ├── Gateway/
│   │   └── TaskFlow.Gateway/
│   └── BuildingBlocks/
│       ├── TaskFlow.BuildingBlocks.Domain/
│       ├── TaskFlow.BuildingBlocks.Application/
│       └── TaskFlow.BuildingBlocks.Infrastructure/
├── tests/
│   ├── TaskFlow.User.UnitTests/
│   ├── TaskFlow.User.IntegrationTests/
│   └── ... (other service tests)
├── scripts/
│   ├── ai-scaffold.sh              ⭐ Interactive spec creator
│   ├── generate-from-spec.sh       ⭐ Code generator
│   ├── update-feature.sh           ⭐ Smart updater
│   ├── cleanup-old-files.sh
│   └── generators/
│       ├── generate-domain.sh
│       ├── generate-application.sh
│       ├── generate-infrastructure.sh
│       ├── generate-api.sh
│       └── generate-tests.sh
├── docs/
│   ├── CODE_GENERATION_SYSTEM.md
│   ├── AI_SCAFFOLDING_GUIDE.md
│   ├── FEATURE_UPDATE_GUIDE.md
│   ├── UPDATE_PARADOX_SOLVED.md
│   └── features/
│       ├── Identity_feature_example.md
│       └── Product_data.json (example)
├── docker/
│   ├── Dockerfile.user
│   ├── Dockerfile.catalog
│   └── ... (other service Dockerfiles)
├── docker-compose.yml              ⭐ Main compose file
├── docker-compose.override.yml
├── docker-compose.test.yml
├── .env.example                    ⭐ Environment template
├── .gitignore
├── TaskFlow.sln
├── README.md
├── QUICKSTART_CODE_GENERATION.md   ⭐ Start here!
├── COMPLETE_SYSTEM_SUMMARY.md
├── PROJECT_STATUS.md               ⭐ Current status
├── CLAUDE.md                       ⭐ This file
└── GEMINI.md                       ⭐ Gemini context
```

---

## 🔑 Key Concepts & Patterns

### Domain-Driven Design (DDD)

**Aggregate Root**:
```csharp
public abstract class AggregateRoot<TId> : Entity<TId>
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
```

**Domain Events**:
```csharp
public sealed record UserCreatedDomainEvent(Guid UserId) : IDomainEvent;
```

**Domain Exceptions**:
```csharp
public sealed class UserNotFoundException : NotFoundException
{
    public UserNotFoundException(Guid userId)
        : base($"User with ID {userId} was not found") { }
}
```

### CQRS Pattern

**Commands** (Write operations):
```csharp
public sealed record CreateUserCommand : IRequest<Result<Guid>>
{
    public required string Email { get; init; }
    public required string FirstName { get; init; }
}

public sealed class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = UserEntity.Create(request.Email, request.FirstName);
        await _repository.AddAsync(user, cancellationToken);
        return Result.Success(user.Id);
    }
}
```

**Queries** (Read operations):
```csharp
public sealed record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDto>>;

public sealed class GetUserByIdQueryHandler
    : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return user is null
            ? Result.Failure<UserDto>("User not found")
            : Result.Success(user.Adapt<UserDto>());
    }
}
```

### Repository Pattern

```csharp
// Interface (in Application layer)
public interface IUserRepository
{
    Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(UserEntity user, CancellationToken cancellationToken = default);
    void Update(UserEntity user);
    void Delete(UserEntity user);
}

// Implementation (in Infrastructure layer)
public sealed class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;

    public async Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserEntity>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
```

### Result Pattern

```csharp
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);
    public static Result<T> Failure<T>(string error) => new(default, false, error);
}
```

---

## 🚨 Important Notes for Claude

### DO's ✅

1. **Always read the quick start first** if unfamiliar:
   - `QUICKSTART_CODE_GENERATION.md`
   - `COMPLETE_SYSTEM_SUMMARY.md`

2. **Use the update script for existing features**:
   ```bash
   ./scripts/update-feature.sh Feature Service --interactive
   ```
   NOT `generate-from-spec.sh` (which overwrites!)

3. **Preserve [CUSTOM] markers**:
   - Any code between `// [CUSTOM]` markers is user-written
   - NEVER remove or modify [CUSTOM] marked sections
   - Always suggest adding [CUSTOM] markers for new business logic

4. **Follow Clean Architecture**:
   - Domain layer: No dependencies on other layers
   - Application layer: Depends only on Domain
   - Infrastructure layer: Implements Application interfaces
   - API layer: Thin controllers, delegate to MediatR

5. **Use MediatR for all operations**:
   ```csharp
   // In Controller
   [HttpPost]
   public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
   {
       var command = request.Adapt<CreateUserCommand>();
       var result = await _mediator.Send(command);
       return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
   }
   ```

6. **Create backups before major changes**:
   ```bash
   git checkout -b backup-before-changes
   git tag before-changes
   ```

7. **Run tests after generation/updates**:
   ```bash
   dotnet test
   ```

### DON'Ts ❌

1. **DON'T use `generate-from-spec.sh` on existing features**
   - It will overwrite custom code!
   - Use `update-feature.sh` instead

2. **DON'T modify generated code without [CUSTOM] markers**
   - It will be lost on next update
   - Always mark custom code

3. **DON'T skip validation in commands**
   - All commands must have FluentValidation validators

4. **DON'T put business logic in Controllers**
   - Controllers are thin, logic goes in Domain/Application

5. **DON'T create direct database dependencies in Domain**
   - Use repository interfaces in Application layer

6. **DON'T forget to update specifications**
   - Always run `ai-scaffold.sh` before `update-feature.sh`
   - Keep `{feature}_feature.md` in sync with code

7. **DON'T bypass the Result pattern**
   - Always return `Result` or `Result<T>`
   - Use `Result.Success()` and `Result.Failure()`

---

## 🔧 Troubleshooting

### "My custom code was overwritten!"

**Solution**:
1. Restore from git: `git reset --hard HEAD` or `git restore .`
2. Check `.backups/` folder for automatic backups
3. Mark custom code with `[CUSTOM]` markers
4. Use `update-feature.sh` (not `generate-from-spec.sh`)

### "Generation script doesn't find my specification"

**Solution**:
1. Ensure `{feature}_data.json` exists in `docs/features/`
2. Run `ai-scaffold.sh` first to create specification
3. Check file naming: `Product_data.json` not `product_data.json`

### "EF Core migration errors"

**Solution**:
```bash
# Add migration
dotnet ef migrations add InitialCreate --project src/Services/User/TaskFlow.User.Infrastructure --startup-project src/Services/User/TaskFlow.User.API

# Update database
dotnet ef database update --project src/Services/User/TaskFlow.User.Infrastructure --startup-project src/Services/User/TaskFlow.User.API
```

### "Docker containers won't start"

**Solution**:
```bash
# Check logs
docker-compose logs -f

# Clean and rebuild
docker-compose down -v
docker-compose build --no-cache
docker-compose up -d
```

---

## 📊 Current Implementation Status

### Completed ✅

**User Service**:
- ✅ UserEntity with full DDD implementation
- ✅ User domain (Entity, Events, Exceptions, Enums)
- ✅ User domain enums: UserStatus, UserRole
- ✅ Clean Architecture structure
- ✅ Ready for Application layer generation

**Code Generation System**:
- ✅ AI-powered scaffolding (`ai-scaffold.sh`)
- ✅ Complete code generation (`generate-from-spec.sh`)
- ✅ Smart update system (`update-feature.sh`)
- ✅ Modular generators (5 scripts)
- ✅ Update paradox solution (3-layer protection)
- ✅ Comprehensive documentation (10+ guides)
- ✅ Production ready

**Infrastructure**:
- ✅ Docker Compose setup
- ✅ Multi-environment support (dev, staging, prod)
- ✅ PostgreSQL, RabbitMQ, Redis, Seq, Jaeger
- ✅ .NET 8.0 project structure

### Pending 📋

**Features to Generate**:
- [ ] Product feature (Catalog service)
- [ ] Order feature (Order service)
- [ ] Notification feature (Notification service)
- [ ] Generate Application/Infrastructure/API layers for User

**Future Enhancements**:
- [ ] Entity relationships support in generators
- [ ] Automatic EF migration generation
- [ ] GraphQL endpoint generation
- [ ] Event handler generation
- [ ] Performance optimization templates
- [ ] API Gateway implementation
- [ ] Authentication/Authorization
- [ ] Integration with message bus (RabbitMQ)

---

## 📖 Learning Resources

### New to the Project?

1. **Start here** (2 minutes):
   ```bash
   cat QUICKSTART_CODE_GENERATION.md
   ```

2. **Understand the system** (15 minutes):
   ```bash
   cat COMPLETE_SYSTEM_SUMMARY.md
   cat docs/UPDATE_PARADOX_SOLVED.md
   ```

3. **See a real example** (10 minutes):
   ```bash
   cat docs/features/Identity_feature_example.md
   ```

4. **Generate your first feature** (5 minutes):
   ```bash
   ./scripts/ai-scaffold.sh TestProduct Catalog
   ./scripts/generate-from-spec.sh TestProduct Catalog
   ls -R src/Services/Catalog/
   ```

### Understanding Clean Architecture

**Flow of a Request**:
```
HTTP Request
    ↓
Controller (API Layer)
    ↓
MediatR.Send(Command/Query)
    ↓
CommandHandler/QueryHandler (Application Layer)
    ↓
Repository Interface (Application Layer)
    ↓
Repository Implementation (Infrastructure Layer)
    ↓
DbContext (Infrastructure Layer)
    ↓
Database
```

**Dependency Direction**:
```
API → Application → Domain
         ↑
    Infrastructure
```

### Understanding CQRS

**Write Path (Commands)**:
```
CreateUserRequest (API)
    → CreateUserCommand (Application)
    → CreateUserCommandValidator (Application)
    → CreateUserCommandHandler (Application)
    → UserEntity.Create() (Domain)
    → IUserRepository.AddAsync() (Application Interface)
    → UserRepository.AddAsync() (Infrastructure Implementation)
    → SaveChangesAsync()
```

**Read Path (Queries)**:
```
GET /api/users/{id} (API)
    → GetUserByIdQuery (Application)
    → GetUserByIdQueryHandler (Application)
    → IUserRepository.GetByIdAsync() (Application Interface)
    → UserRepository.GetByIdAsync() (Infrastructure Implementation)
    → Adapt<UserDto>() (Mapster)
    → Return DTO
```

---

## 🎓 Code Generation Examples

### Example 1: Generate Product Feature

```bash
# Step 1: Create specification
./scripts/ai-scaffold.sh Product Catalog

# AI asks questions:
# Q: What is the main purpose of the Product feature?
# A: Manage product catalog with pricing and inventory

# Q: What properties should the ProductEntity have?
# A: Name (string, required, max 200 chars)
# A: Description (string, optional, max 1000 chars)
# A: Price (decimal, required, > 0)
# A: SKU (string, required, unique)
# A: StockQuantity (int, required, >= 0)
# A: (press Enter when done)

# Q: What business rules should be enforced?
# A: Price must be greater than 0
# A: SKU must be unique
# A: Cannot delete product with active orders
# A: (press Enter when done)

# Q: What operations should be available?
# A: Create product
# A: Update product details
# A: Update stock quantity
# A: Delete product
# A: Get all products
# A: Get product by ID
# A: Search products by name/SKU
# A: (press Enter when done)

# Step 2: Generate code
./scripts/generate-from-spec.sh Product Catalog

# Result: 26+ files created in Catalog service!
```

### Example 2: Update User Feature with Custom Logic

```bash
# You added custom business logic to UserEntity:
# [CUSTOM]
# public void PromoteToAdmin() { ... }
# public Result ValidateEmailDomain() { ... }
# [CUSTOM]

# Now you want to add a new property: PhoneNumber

# Step 1: Update specification
./scripts/ai-scaffold.sh User User
# Add PhoneNumber property in the questions

# Step 2: Update (NOT generate!)
./scripts/update-feature.sh User User --interactive

# The script will:
# ✓ Preserve your [CUSTOM] methods
# ✓ Show diff for adding PhoneNumber
# ✓ Ask for confirmation
# ✓ Create backup before changes
# ✓ Apply only approved changes

# Your custom logic is safe!
```

---

## 🔍 Quick Reference Commands

### Code Generation
```bash
# Create specification
./scripts/ai-scaffold.sh <Feature> <Service>

# Generate new feature
./scripts/generate-from-spec.sh <Feature> <Service>

# Update existing feature
./scripts/update-feature.sh <Feature> <Service> --interactive
```

### .NET Commands
```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Run specific service
dotnet run --project src/Services/User/TaskFlow.User.API

# Add migration
dotnet ef migrations add <Name> --project <Infrastructure> --startup-project <API>

# Update database
dotnet ef database update --project <Infrastructure> --startup-project <API>
```

### Docker Commands
```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f [service]

# Rebuild and start
docker-compose up -d --build

# Stop all
docker-compose down

# Clean everything
docker-compose down -v --remove-orphans
```

### Git Commands
```bash
# Check status
git status

# Create backup
git checkout -b backup-$(date +%Y%m%d-%H%M%S)

# View recent commits
git log --oneline -10

# See changes
git diff

# Restore file
git restore <file>
```

---

## 💡 Pro Tips

### Tip 1: Always Use Interactive Mode

```bash
# Good ✅
./scripts/update-feature.sh User User --interactive

# Risky ❌
./scripts/update-feature.sh User User --force
```

### Tip 2: Mark Custom Code Immediately

```csharp
// As soon as you write custom logic:
// [CUSTOM]
public void MyCustomMethod()
{
    // Your logic
}
// [CUSTOM]
```

### Tip 3: Keep Specifications Updated

```bash
# Before any changes, update the spec:
./scripts/ai-scaffold.sh User User
# Then update code:
./scripts/update-feature.sh User User --interactive
```

### Tip 4: Use Partial Classes for Heavy Customization

```csharp
// UserEntity.cs (generated)
public partial class UserEntity : AggregateRoot<Guid>
{
    // Generated code
}

// UserEntity.Custom.cs (your file)
public partial class UserEntity
{
    // [CUSTOM]
    // All your custom logic here
    // [CUSTOM]
}
```

### Tip 5: Check Backups After Updates

```bash
# Automatic backups are in:
ls -la .backups/

# Restore if needed:
cp .backups/UserEntity_20251031_143022.cs.bak src/.../UserEntity.cs
```

---

## 🎯 Session Checklist

When starting a new Claude session, quickly check:

- [ ] Read `PROJECT_STATUS.md` for current state
- [ ] Check `git status` for uncommitted changes
- [ ] Review recent commits: `git log --oneline -5`
- [ ] Check Docker services: `docker-compose ps`
- [ ] Verify backups: `ls .backups/` if doing updates
- [ ] Confirm which feature you're working on
- [ ] Use `--interactive` mode for updates

---

## 📞 Quick Help

**"I want to generate a new feature"**
→ Read `QUICKSTART_CODE_GENERATION.md`

**"I want to update existing feature"**
→ Read `docs/FEATURE_UPDATE_GUIDE.md`

**"I need to understand the update paradox"**
→ Read `docs/UPDATE_PARADOX_SOLVED.md`

**"Show me a complete example"**
→ Read `docs/features/Identity_feature_example.md`

**"How does the system work?"**
→ Read `COMPLETE_SYSTEM_SUMMARY.md`

**"I'm migrating from old system"**
→ Read `MIGRATION_GUIDE.md`

---

## 🚀 Ready to Code!

This context file ensures you understand:
- ✅ Project architecture
- ✅ Code generation system
- ✅ Update paradox solution
- ✅ Clean Architecture patterns
- ✅ CQRS implementation
- ✅ DDD principles
- ✅ Common workflows
- ✅ Troubleshooting

**Next Steps**:
1. Check `PROJECT_STATUS.md` for current implementation status
2. Review `git log` for recent changes
3. Read user's request carefully
4. Use appropriate script (generate vs update!)
5. Preserve [CUSTOM] markers
6. Test with `dotnet test`

**Remember**:
- Use `update-feature.sh` for existing features
- Always use `--interactive` mode
- Preserve [CUSTOM] markers
- Create backups before major changes
- Follow Clean Architecture
- Test after changes

---

**Last Updated**: 2025-10-31
**Commit**: 8121a27
**System Status**: 🟢 Production Ready
**Documentation**: 📚 Complete

Happy coding! 🚀
