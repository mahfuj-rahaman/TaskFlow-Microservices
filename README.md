# 🚀 ASP.NET Core Microservices - Production Guide

> **Master enterprise microservices with .NET 8** | Complete implementation with Clean Architecture, CQRS, Code-Level Idempotency, API Versioning, Event-Driven Design, AWS Deployment, and 87% test coverage

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Microservices](https://img.shields.io/badge/Architecture-Microservices-blue)](https://microservices.io/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Coverage](https://img.shields.io/badge/Coverage-87%25-brightgreen)](https://github.com/yourusername/TaskFlow-Microservices)

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Quick Facts](#-quick-facts)
- [Key Features](#-key-features)
- [Technology Stack](#️-technology-stack)
- [Architecture](#️-architecture-overview)
- [Project Structure](#-project-structure)
- [Code-Level Idempotency](#-code-level-idempotency-critical)
- [API Versioning](#-api-versioning-strategies)
- [Getting Started](#-getting-started)
- [Testing](#-testing)
- [Deployment](#-deployment)
- [Contributing](#-contributing)

---

## 🎯 Overview

**TaskFlow** is a production-ready microservices architecture demonstrating enterprise patterns often overlooked in tutorials:

- ✅ **Code-Level Idempotency** - Prevent duplicate operations (5 strategies)
- ✅ **API Versioning** - Backward compatibility (4 strategies)
- ✅ **High-Performance Mapping** - Mapster (5x faster than AutoMapper)
- ✅ **Clean Architecture** - DDD with clear boundaries
- ✅ **Event-Driven** - Async messaging with MassTransit
- ✅ **AWS Deployment** - Complete IaC with Terraform
- ✅ **87% Coverage** - Unit, integration, architecture tests

### ⚡ Quick Facts

| Aspect | Details |
|--------|---------|
| **Language** | C# 12 (.NET 8 LTS) |
| **Architecture** | Microservices + Clean Architecture + DDD |
| **Communication** | REST, gRPC, Event-Driven (SQS) |
| **Cloud** | AWS (ECS, RDS, ElastiCache, SQS) |
| **Database** | PostgreSQL 15 + Redis 7 |
| **Mapping** | Mapster (4-6x faster than AutoMapper) |
| **Testing** | xUnit, TestContainers, 87% coverage |
| **Frontend** | Blazor Server & WebAssembly |
| **Lines of Code** | 50,000+ (well-documented) |

---

## 🌟 Key Features

### Production Patterns

- **Idempotency**: 5 strategies (MediatR pipeline, HTTP middleware, database constraints, optimistic locking, message deduplication)
- **API Versioning**: URL-based, header-based, media type, query string
- **Resilience**: Circuit breaker, retry, timeout, bulkhead (Polly)
- **Observability**: OpenTelemetry, distributed tracing, structured logging
- **Security**: OAuth2, JWT, rate limiting, OWASP best practices

### Architecture Patterns

- Clean Architecture (Onion Architecture)
- CQRS with MediatR
- Event Sourcing
- Saga Pattern for distributed transactions
- Outbox Pattern for reliable messaging
- Inbox Pattern for idempotent event processing
- API Gateway (YARP/Ocelot)
- Database per Service

---

## 🛠️ Technology Stack

### Core Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 8.0 LTS | Runtime |
| ASP.NET Core | 8.0 | Web APIs |
| gRPC | Latest | Inter-service communication (7x faster) |
| MassTransit | 8.x | Message bus |
| MediatR | 12.x | CQRS/Mediator |
| EF Core | 8.0 | ORM |
| Mapster | 7.x | Object mapping (5x faster) |
| Polly | 8.x | Resilience |

### Infrastructure

| Service | Purpose | Free Tier |
|---------|---------|-----------|
| PostgreSQL 15 | Primary database | ✅ |
| Redis 7 | Cache, pub/sub, idempotency | ✅ |
| AWS SQS | Message queue | 1M req/mo |
| AWS ECS | Container orchestration | 750 hrs/mo |
| AWS RDS | Managed database | 750 hrs/mo |
| AWS ElastiCache | Managed Redis | 750 hrs/mo |

### Testing & DevOps

- **Testing**: xUnit, FluentAssertions, Moq, TestContainers, Bogus, NetArchTest
- **CI/CD**: GitHub Actions, Docker, Terraform
- **Monitoring**: OpenTelemetry, Prometheus, Grafana, Seq, Jaeger

---

## 🏗️ Architecture Overview

### System Diagram

```
                    ┌─────────────────┐
                    │   Clients       │
                    └────────┬────────┘
                             │ HTTPS
                    ┌────────▼────────┐
                    │  API Gateway    │
                    │  (Port 7000)    │
                    └─────┬─────┬─────┘
                          │     │
            ┌─────────────┼─────┼──────────────┐
            │             │     │              │
       ┌────▼───┐   ┌────▼───┐ ┌────▼───┐  ┌──▼──┐
       │ Task   │   │ User   │ │Identity│  │Notif│
       │Service │   │Service │ │Service │  │Svc  │
       │7001    │   │7002    │ │7003    │  │7004 │
       └────┬───┘   └────┬───┘ └────┬───┘  └──┬──┘
            │            │          │         │
            └────────────┴──────────┴─────────┘
                         │
                    ┌────▼─────┐
                    │ Event Bus│
                    │   (SQS)  │
                    └────┬─────┘
                         │
            ┌────────────┴──────────────┐
            │                           │
       ┌────▼─────┐              ┌─────▼────┐
       │PostgreSQL│              │  Redis   │
       │  (RDS)   │              │(ElastiCa)│
       └──────────┘              └──────────┘
```

### Service Communication

| From → To | Method | Technology | Use Case |
|-----------|--------|------------|----------|
| Client → Service | Sync | REST API | User requests |
| Service ↔ Service | Sync | gRPC | Critical path |
| Service → Service | Async | SQS | Non-critical |
| Server → Client | Real-time | SignalR | Notifications |

---

## 📂 Project Structure

```
TaskFlow-Microservices/
│
├── 📁 src/
│   ├── 📁 Services/
│   │   ├── 📁 Task/
│   │   │   ├── TaskFlow.Task.Domain/          # ⭐ No dependencies
│   │   │   ├── TaskFlow.Task.Application/     # ⭐ Depends on Domain
│   │   │   ├── TaskFlow.Task.Infrastructure/  # ⭐ External dependencies
│   │   │   └── TaskFlow.Task.API/             # ⭐ Presentation
│   │   ├── 📁 User/
│   │   ├── 📁 Identity/
│   │   └── 📁 Notification/
│   │
│   ├── 📁 ApiGateway/
│   ├── 📁 Web/                                # Blazor apps
│   │
│   └── 📁 BuildingBlocks/
│       ├── TaskFlow.Common/
│       ├── TaskFlow.EventBus/
│       ├── TaskFlow.Idempotency/              # ⭐ Shared idempotency
│       └── TaskFlow.Versioning/               # ⭐ API versioning
│
├── 📁 tests/
│   ├── UnitTests/
│   ├── IntegrationTests/
│   ├── ArchitectureTests/                     # Enforce rules
│   └── E2ETests/
│
├── 📁 docs/
├── 📁 scripts/
├── 📁 infrastructure/                         # Terraform
└── 📁 .github/workflows/                      # CI/CD
```

---

## 🔄 Code-Level Idempotency (Critical)

### Why Critical?

Distributed systems have duplicate requests due to:
- Network timeouts → User retries
- Load balancer retries
- Message queue redelivery
- Client bugs (double-click)

**Without idempotency:** Duplicate payments, multiple emails, data corruption
**With idempotency:** Same request = same result

### Strategy Matrix

| Strategy | Best For | Storage | Speed | Complexity |
|----------|----------|---------|-------|------------|
| **1. MediatR Pipeline** | Critical operations | Database | ⚡⚡ | 🟢 Low |
| **2. HTTP Middleware** | All endpoints | Redis | ⚡⚡⚡ | 🟢 Low |
| **3. Natural Keys** | External integrations | Database | ⚡⚡⚡ | 🟢 Very Low |
| **4. Optimistic Locking** | Concurrent updates | Database | ⚡⚡⚡ | 🟢 Low |
| **5. Inbox Pattern** | Event consumers | Database | ⚡⚡ | 🟡 Medium |

### Quick Implementation: MediatR Pipeline

```csharp
// 1. Marker Interface
public interface IIdempotentRequest
{
    string IdempotencyKey { get; }
}

// 2. Command with Idempotency
public class CreateTaskCommand : IRequest<Result<Guid>>, IIdempotentRequest
{
    [JsonIgnore]
    public string IdempotencyKey { get; init; } = default!;
    
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
}

// 3. MediatR Behavior
public class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IIdempotentRequest
{
    private readonly IIdempotencyRepository _repository;
    
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        // Check if already processed
        var existing = await _repository.GetByKeyAsync(request.IdempotencyKey, ct);
        if (existing != null)
        {
            return JsonSerializer.Deserialize<TResponse>(existing.ResponsePayload)!;
        }
        
        // Process and store
        var response = await next();
        await _repository.StoreAsync(request.IdempotencyKey, response, ct);
        
        return response;
    }
}

// 4. Database Record
public class IdempotencyRecord
{
    public string IdempotencyKey { get; set; } // Unique constraint
    public string RequestPath { get; set; }
    public string RequestPayload { get; set; }
    public string ResponsePayload { get; set; }
    public DateTime CreatedAt { get; set; }
}

// 5. Controller Usage
[HttpPost]
public async Task<IActionResult> CreateTask(
    [FromBody] CreateTaskRequest request,
    [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
{
    var command = new CreateTaskCommand
    {
        IdempotencyKey = idempotencyKey,
        Title = request.Title,
        Description = request.Description
    };
    
    var result = await _mediator.Send(command);
    return result.IsSuccess ? CreatedAtAction(nameof(GetTask), new { id = result.Data }, result.Data)
                           : BadRequest(result.Errors);
}

// 6. Client Usage
// JavaScript
const idempotencyKey = crypto.randomUUID(); // Generate ONCE
const response = await fetch('/api/tasks', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'Idempotency-Key': idempotencyKey // Same key on retry
    },
    body: JSON.stringify({ title: 'Task', description: 'Desc' })
});
```

**Full implementation:** [docs/tutorials/09-idempotency-implementation.md](docs/tutorials/09-idempotency-implementation.md)

---

## 🔀 API Versioning Strategies

### Why Version APIs?

- Evolve API without breaking existing clients
- Support multiple client versions simultaneously
- Gradual migration for partners/mobile apps

### Strategy Matrix

| Strategy | Route Example | Pros | Cons |
|----------|--------------|------|------|
| **URL-Based** | `/api/v1/tasks` | ✅ Clear, cacheable | URL changes |
| **Header-Based** | `api-version: 1.0` | ✅ Clean URLs | Hidden in headers |
| **Media Type** | `Accept: application/vnd.api.v1+json` | ✅ RESTful | Complex |
| **Query String** | `/api/tasks?v=1` | ✅ Simple | Not RESTful |

### Quick Implementation: URL-Based (Recommended)

```csharp
// 1. Install Package
// dotnet add package Asp.Versioning.Mvc

// 2. Configure Services
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true; // Add version headers
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// 3. Controller v1
[ApiController]
[Route("api/v{version:apiVersion}/tasks")]
[ApiVersion("1.0")]
public class TasksV1Controller : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<TaskDtoV1> GetTask(Guid id)
    {
        // v1 logic
    }
}

// 4. Controller v2 (New features)
[ApiController]
[Route("api/v{version:apiVersion}/tasks")]
[ApiVersion("2.0")]
public class TasksV2Controller : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<TaskDtoV2> GetTask(Guid id)
    {
        // v2 logic with breaking changes
    }
}

// 5. DTOs
public class TaskDtoV1
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Status { get; set; } // enum as string
}

public class TaskDtoV2
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public int Priority { get; set; } // New field
    public TaskStatusV2 Status { get; set; } // Different enum
}

// 6. Swagger Configuration (Multiple versions)
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TaskFlow API", Version = "v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "TaskFlow API", Version = "v2" });
});

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow API v1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "TaskFlow API v2");
});
```

**Multiple Strategies Combined:**

```csharp
// Support URL + Header + Query String
builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("api-version"),
        new QueryStringApiVersionReader("v")
    );
});

// Now all these work:
// GET /api/v1/tasks/123
// GET /api/tasks/123 (Header: api-version: 1.0)
// GET /api/tasks/123?v=1.0
```

**Full guide:** [docs/tutorials/10-api-versioning-setup.md](docs/tutorials/10-api-versioning-setup.md)

---

## 🚀 Getting Started

### Prerequisites

```bash
# Required
.NET 8 SDK
Docker Desktop
PostgreSQL 15
Redis 7

# Optional
Visual Studio 2022 / Rider / VS Code
AWS CLI (for deployment)
```

### Quick Start

```bash
# 1. Clone repository
git clone https://github.com/yourusername/TaskFlow-Microservices.git
cd TaskFlow-Microservices

# 2. Start dependencies (Docker Compose)
docker-compose up -d

# 3. Run migrations
dotnet ef database update --project src/Services/Task/TaskFlow.Task.Infrastructure

# 4. Start services
dotnet run --project src/Services/Task/TaskFlow.Task.API
dotnet run --project src/Services/User/TaskFlow.User.API
dotnet run --project src/Services/Identity/TaskFlow.Identity.API
dotnet run --project src/ApiGateway/TaskFlow.Gateway

# 5. Access
# API Gateway: http://localhost:7000
# Swagger: http://localhost:7001/swagger (Task Service)
# Blazor: http://localhost:7100
```

### Configuration

```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Database=TaskFlow;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "AWS": {
    "Region": "us-east-1",
    "SQS": {
      "QueueUrl": "https://sqs.us-east-1.amazonaws.com/123456/taskflow-events"
    }
  },
  "Idempotency": {
    "CacheDuration": "24:00:00",
    "RequireKeyForAllMutations": true
  }
}
```

---

## 🧪 Testing

### Test Coverage: 87%

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific category
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
```

### Test Structure

```csharp
// Unit Test Example
public class CreateTaskHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesTask()
    {
        // Arrange
        var repository = new Mock<ITaskRepository>();
        var handler = new CreateTaskHandler(repository.Object);
        var command = new CreateTaskCommand { Title = "Test" };
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        repository.Verify(x => x.AddAsync(It.IsAny<Task>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}

// Integration Test with TestContainers
public class TaskRepositoryTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgres;
    private TaskDbContext _context;
    
    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder().Build();
        await _postgres.StartAsync();
        
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        
        _context = new TaskDbContext(options);
        await _context.Database.MigrateAsync();
    }
    
    [Fact]
    public async Task AddAsync_ValidTask_StoresInDatabase()
    {
        // Test with real database
    }
}
```

---

## 🌐 Deployment

### AWS Architecture

```
Internet → Route 53 → ALB → ECS Fargate
                              ↓
                    [Service Containers]
                              ↓
                    RDS + ElastiCache + SQS
```

### Terraform Deployment

```bash
# Initialize
cd infrastructure/terraform
terraform init

# Plan
terraform plan -var-file=environments/dev/terraform.tfvars

# Apply
terraform apply -var-file=environments/dev/terraform.tfvars

# Outputs
# alb_dns_name = "taskflow-alb-123456.us-east-1.elb.amazonaws.com"
# rds_endpoint = "taskflow-db.abc123.us-east-1.rds.amazonaws.com"
```

### GitHub Actions CI/CD

```yaml
# .github/workflows/cd-production.yml
name: Deploy to Production

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Build Docker Images
        run: docker build -t task-service:${{ github.sha }} .
      
      - name: Push to ECR
        run: |
          aws ecr get-login-password | docker login --username AWS --password-stdin $ECR_REGISTRY
          docker push task-service:${{ github.sha }}
      
      - name: Deploy to ECS
        run: |
          aws ecs update-service --cluster taskflow --service task-service --force-new-deployment
```

**Monthly Cost Estimate:**
- Free tier: $0-5/month (within limits)
- Small production: $50-100/month
- Medium scale: $200-500/month

---

## 📚 Learning Resources

### Documentation
- [Architecture Decision Records](docs/architecture/adr/)
- [API Documentation](docs/api/)
- [Step-by-Step Tutorials](docs/tutorials/)
- [Deployment Guides](docs/deployment/)

### Key Tutorials
1. [Clean Architecture Setup](docs/tutorials/01-project-setup.md)
2. [Implementing CQRS with MediatR](docs/tutorials/03-application-layer.md)
3. [Code-Level Idempotency](docs/tutorials/09-idempotency-implementation.md)
4. [API Versioning](docs/tutorials/10-api-versioning-setup.md)
5. [AWS Deployment](docs/tutorials/13-aws-deployment.md)

---

## 🤝 Contributing

Contributions welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md)

```bash
# Fork, create branch
git checkout -b feature/amazing-feature

# Make changes, commit
git commit -m "Add amazing feature"

# Push, create PR
git push origin feature/amazing-feature
```

---

## 📊 Project Status

- ✅ Core microservices architecture
- ✅ Clean Architecture implementation
- ✅ CQRS with MediatR
- ✅ Code-level idempotency (5 strategies)
- ✅ API versioning (4 strategies)
- ✅ Event-driven with MassTransit
- ✅ Docker containerization
- ✅ 87% test coverage
- ✅ AWS deployment with Terraform
- 🚧 Kubernetes deployment (in progress)
- 🚧 Service mesh with Istio (planned)

---
