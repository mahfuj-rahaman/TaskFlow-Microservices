# 🚀 TaskFlow - Enterprise Microservices with .NET 8

> **Production-ready microservices architecture** demonstrating Clean Architecture, CQRS, Event-Driven Design, Idempotency, API Versioning, and Multi-Cloud deployment strategies

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Microservices](https://img.shields.io/badge/Architecture-Microservices-blue)](https://microservices.io/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Coverage](https://img.shields.io/badge/Coverage-87%25-brightgreen)](.)

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Architecture](#️-architecture)
- [Technology Stack](#-technology-stack)
- [Key Implementations](#-key-implementations)
- [Cloud Abstraction](#-cloud-abstraction)
- [Getting Started](#-getting-started)
- [Deployment Strategies](#-deployment-strategies)
- [CI/CD Pipelines](#-cicd-pipelines)
- [Testing](#-testing)
- [Documentation](#-documentation)

---

## 🎯 Overview

**TaskFlow** is a complete microservices implementation covering essential enterprise patterns:

| Pattern | Implementation | Status |
|---------|---------------|--------|
| **Clean Architecture** | Domain, Application, Infrastructure, Presentation | ✅ |
| **CQRS** | MediatR with separate read/write models | ✅ |
| **Event-Driven** | MassTransit with message queues | ✅ |
| **Idempotency** | Multiple strategies for duplicate prevention | ✅ |
| **API Versioning** | URL-based and header-based strategies | ✅ |
| **Distributed Caching** | Redis with fallback mechanisms | ✅ |
| **Service Communication** | REST, gRPC, Async messaging | ✅ |
| **Resilience** | Circuit breaker, retry, timeout (Polly) | ✅ |
| **Observability** | Logging, tracing, metrics | ✅ |
| **Security** | OAuth2, JWT, API keys | ✅ |
| **Multi-Cloud** | AWS, Azure, GCP deployment | ✅ |
| **Testing** | Unit, Integration, Architecture, E2E | ✅ 87% |
| **CI/CD** | GitHub Actions, Azure DevOps | ✅ |
| **IaC** | Terraform (Cloud-agnostic) | ✅ |

---

## 🏗️ Architecture

### System Overview

```
┌─────────────────────────────────────────────────────────┐
│                     Client Layer                         │
│          (Web, Mobile, External Systems)                 │
└───────────────────────┬─────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│                   API Gateway                            │
│   • Routing  • Auth  • Rate Limiting  • Versioning      │
└───────┬─────────────┬─────────────┬─────────────────────┘
        │             │             │
        ▼             ▼             ▼
┌──────────┐   ┌──────────┐   ┌──────────┐   ┌──────────┐
│   Task   │   │   User   │   │ Identity │   │  Notif   │
│ Service  │   │ Service  │   │ Service  │   │ Service  │
│  :7001   │   │  :7002   │   │  :7003   │   │  :7004   │
└────┬─────┘   └────┬─────┘   └────┬─────┘   └────┬─────┘
     │              │              │              │
     └──────────────┴──────────────┴──────────────┘
                    │
                    ▼
     ┌──────────────────────────────────────┐
     │        Message Broker (Abstracted)    │
     │     SQS / Azure Service Bus / Kafka   │
     └──────────────┬───────────────────────┘
                    │
     ┌──────────────┴───────────────┐
     │                              │
     ▼                              ▼
┌─────────┐                    ┌─────────┐
│Database │                    │  Cache  │
│PostgreSQL                    │  Redis  │
│  / SQL  │                    │         │
└─────────┘                    └─────────┘
```

---

## 🛠 Technology Stack

### Backend

```yaml
Runtime: .NET 8.0 LTS (C# 12)
Web Framework: ASP.NET Core 8.0
Communication:
  - HTTP/REST: ASP.NET Core Web API
  - gRPC: Google.Protobuf (7x faster than REST)
  - Async: MassTransit (message abstraction)
Patterns:
  - CQRS: MediatR
  - Mapping: Mapster (5x faster than AutoMapper)
  - Validation: FluentValidation
  - Resilience: Polly
Database:
  - Primary: PostgreSQL / SQL Server (abstracted)
  - Cache: Redis / Azure Cache (abstracted)
  - ORM: Entity Framework Core 8.0
Observability:
  - Logging: Serilog
  - Tracing: OpenTelemetry
  - Metrics: Prometheus
```

### Testing

```yaml
Framework: xUnit
Assertions: FluentAssertions
Mocking: Moq
Integration: TestContainers (real dependencies)
Architecture: NetArchTest (enforce rules)
Data Generation: Bogus
Coverage: 87%
```

---

## 🔑 Key Implementations

### 1. Clean Architecture (DDD)

```
TaskFlow.Task.Domain/         ← Core (no dependencies)
TaskFlow.Task.Application/    ← Use cases (depends on Domain)
TaskFlow.Task.Infrastructure/ ← External concerns
TaskFlow.Task.API/            ← Presentation
```

### 2. CQRS with MediatR

```csharp
// Command (Write)
public class CreateTaskCommand : IRequest<Result<Guid>>
{
    public string Title { get; init; }
}

// Query (Read)
public class GetTaskQuery : IRequest<TaskDto>
{
    public Guid TaskId { get; init; }
}
```

### 3. Idempotency (5 Strategies)

```csharp
// Strategy 1: MediatR Pipeline (Critical operations)
public interface IIdempotentRequest { string IdempotencyKey { get; } }

// Strategy 2: HTTP Middleware (All endpoints)
app.UseIdempotencyMiddleware();

// Strategy 3: Natural Keys (External integrations)
builder.HasIndex(x => new { x.ExternalId, x.Source }).IsUnique();

// Strategy 4: Optimistic Locking (Concurrent updates)
public class Task { public int Version { get; set; } }

// Strategy 5: Inbox Pattern (Event consumers)
public class InboxMessage { public string MessageId { get; set; } }
```

### 4. API Versioning

```csharp
// URL-based versioning
[Route("api/v{version:apiVersion}/tasks")]
[ApiVersion("1.0")]
public class TasksV1Controller : ControllerBase { }

// Multiple strategies support
builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("api-version"),
        new QueryStringApiVersionReader("v")
    );
});
```

---

## ☁️ Cloud Abstraction

**Avoid vendor lock-in with abstraction layer:**

### Message Queue Abstraction

```csharp
// Interface (BuildingBlocks/TaskFlow.Messaging)
public interface IMessageBroker
{
    Task PublishAsync<T>(T message, CancellationToken ct);
    Task SubscribeAsync<T>(Func<T, Task> handler, CancellationToken ct);
}

// AWS Implementation
public class AwsSqsMessageBroker : IMessageBroker { }

// Azure Implementation
public class AzureServiceBusMessageBroker : IMessageBroker { }

// GCP Implementation
public class GcpPubSubMessageBroker : IMessageBroker { }

// Registration
builder.Services.AddMessageBroker(config.CloudProvider);
```

### Cloud Services Mapping

| Component | AWS | Azure | GCP |
|-----------|-----|-------|-----|
| **Compute** | ECS Fargate | Container Apps | Cloud Run |
| **Orchestration** | ECS | AKS | GKE |
| **Database** | RDS PostgreSQL | Azure Database | Cloud SQL |
| **Cache** | ElastiCache | Azure Cache | Memorystore |
| **Queue** | SQS | Service Bus | Pub/Sub |
| **Storage** | S3 | Blob Storage | Cloud Storage |
| **Secrets** | Secrets Manager | Key Vault | Secret Manager |
| **DNS** | Route 53 | DNS Zone | Cloud DNS |
| **CDN** | CloudFront | Azure CDN | Cloud CDN |

---

## 🚀 Getting Started

### Prerequisites

```bash
# Required
.NET 8 SDK
Docker Desktop

# Optional (for development)
Visual Studio 2022 / Rider / VS Code
```

### Quick Start (Local Development)

```bash
# 1. Clone repository
git clone https://github.com/mahfuj/TaskFlow-Microservices.git
cd TaskFlow-Microservices

# 2. Start dependencies (PostgreSQL + Redis + Message Broker)
docker-compose up -d

# 3. Run database migrations
dotnet ef database update --project src/Services/Task/TaskFlow.Task.Infrastructure

# 4. Start all services
./scripts/start-services.sh
# Or individually:
# dotnet run --project src/Services/Task/TaskFlow.Task.API
# dotnet run --project src/Services/User/TaskFlow.User.API
# dotnet run --project src/ApiGateway/TaskFlow.Gateway

# 5. Access
# API Gateway: http://localhost:7000
# Task Service: http://localhost:7001/swagger
# Blazor App: http://localhost:7100
```

### Configuration

```json
// appsettings.Development.json
{
  "CloudProvider": "Local", // Local, AWS, Azure, GCP
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionString": "Host=localhost;Database=TaskFlow;Username=postgres;Password=postgres"
  },
  "Cache": {
    "Provider": "Redis",
    "ConnectionString": "localhost:6379"
  },
  "MessageBroker": {
    "Provider": "InMemory", // AWS, Azure, GCP, InMemory (dev)
    "ConnectionString": ""
  }
}
```

---

## 🌐 Deployment Strategies

### Deployment Architecture Comparison

```
┌─────────────┬──────────────────────────────────────────────────────┐
│             │            Deployment Options                        │
├─────────────┼──────────────────────────────────────────────────────┤
│ Local       │ Docker Compose (Development)                         │
├─────────────┼──────────────────────────────────────────────────────┤
│ AWS         │ ECS Fargate (Serverless) / EKS (Kubernetes)         │
├─────────────┼──────────────────────────────────────────────────────┤
│ Azure       │ Container Apps (Serverless) / AKS (Kubernetes)      │
├─────────────┼──────────────────────────────────────────────────────┤
│ GCP         │ Cloud Run (Serverless) / GKE (Kubernetes)           │
└─────────────┴──────────────────────────────────────────────────────┘
```

---

### 🐳 Local Deployment (Docker Compose)

**Best for:** Development, testing, demo

```yaml
# docker-compose.yml
version: '3.8'

services:
  # Infrastructure
  postgres:
    image: postgres:15-alpine
    environment:
      POSTGRES_DB: TaskFlow
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  # Services
  task-service:
    build:
      context: .
      dockerfile: src/Services/Task/TaskFlow.Task.API/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__PostgreSQL=Host=postgres;Database=TaskFlow;Username=postgres;Password=postgres
      - ConnectionStrings__Redis=redis:6379
    ports:
      - "7001:80"
    depends_on:
      - postgres
      - redis

  user-service:
    build:
      context: .
      dockerfile: src/Services/User/TaskFlow.User.API/Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__PostgreSQL=Host=postgres;Database=TaskFlow;Username=postgres;Password=postgres
      - ConnectionStrings__Redis=redis:6379
    ports:
      - "7002:80"
    depends_on:
      - postgres
      - redis

  identity-service:
    build:
      context: .
      dockerfile: src/Services/Identity/TaskFlow.Identity.API/Dockerfile
    ports:
      - "7003:80"
    depends_on:
      - postgres

  api-gateway:
    build:
      context: .
      dockerfile: src/ApiGateway/TaskFlow.Gateway/Dockerfile
    ports:
      - "7000:80"
    depends_on:
      - task-service
      - user-service
      - identity-service

volumes:
  postgres-data:
```

**Commands:**

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Scale services
docker-compose up -d --scale task-service=3

# Stop all services
docker-compose down

# Clean up (remove volumes)
docker-compose down -v
```

---

### ☁️ AWS Deployment (ECS Fargate)

**Best for:** Serverless containers, auto-scaling, pay-per-use

#### Infrastructure (Terraform)

```hcl
# infrastructure/terraform/environments/aws/main.tf

provider "aws" {
  region = var.aws_region
}

# VPC and Networking
module "vpc" {
  source = "../../modules/networking"
  
  vpc_cidr = "10.0.0.0/16"
  availability_zones = ["us-east-1a", "us-east-1b"]
  public_subnet_cidrs = ["10.0.1.0/24", "10.0.2.0/24"]
  private_subnet_cidrs = ["10.0.11.0/24", "10.0.12.0/24"]
}

# RDS PostgreSQL
module "database" {
  source = "../../modules/database"
  
  engine = "postgres"
  engine_version = "15.3"
  instance_class = "db.t3.micro" # Free tier eligible
  allocated_storage = 20
  database_name = "taskflow"
  username = "taskflow_user"
  password = var.db_password # From Secrets Manager
  vpc_id = module.vpc.vpc_id
  subnet_ids = module.vpc.private_subnet_ids
}

# ElastiCache Redis
module "cache" {
  source = "../../modules/cache"
  
  engine = "redis"
  node_type = "cache.t3.micro" # Free tier eligible
  num_cache_nodes = 1
  vpc_id = module.vpc.vpc_id
  subnet_ids = module.vpc.private_subnet_ids
}

# SQS Queue
resource "aws_sqs_queue" "taskflow_events" {
  name = "taskflow-events"
  visibility_timeout_seconds = 300
  message_retention_seconds = 345600 # 4 days
  
  tags = {
    Environment = var.environment
    Application = "TaskFlow"
  }
}

# ECS Cluster
resource "aws_ecs_cluster" "taskflow" {
  name = "taskflow-cluster"
  
  setting {
    name  = "containerInsights"
    value = "enabled"
  }
}

# Application Load Balancer
module "alb" {
  source = "../../modules/load-balancer"
  
  name = "taskflow-alb"
  vpc_id = module.vpc.vpc_id
  subnet_ids = module.vpc.public_subnet_ids
  
  target_groups = [
    {
      name = "task-service"
      port = 80
      health_check_path = "/health"
    },
    {
      name = "user-service"
      port = 80
      health_check_path = "/health"
    }
  ]
}

# ECS Task Definition - Task Service
resource "aws_ecs_task_definition" "task_service" {
  family                   = "task-service"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = "256"
  memory                   = "512"
  execution_role_arn       = aws_iam_role.ecs_execution_role.arn
  task_role_arn            = aws_iam_role.ecs_task_role.arn

  container_definitions = jsonencode([
    {
      name  = "task-service"
      image = "${aws_ecr_repository.task_service.repository_url}:latest"
      
      portMappings = [
        {
          containerPort = 80
          protocol      = "tcp"
        }
      ]
      
      environment = [
        {
          name  = "ASPNETCORE_ENVIRONMENT"
          value = "Production"
        },
        {
          name  = "CloudProvider"
          value = "AWS"
        }
      ]
      
      secrets = [
        {
          name      = "ConnectionStrings__PostgreSQL"
          valueFrom = "${aws_secretsmanager_secret.db_connection.arn}:connectionString::"
        },
        {
          name      = "ConnectionStrings__Redis"
          valueFrom = "${aws_secretsmanager_secret.redis_connection.arn}:connectionString::"
        }
      ]
      
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = "/ecs/task-service"
          "awslogs-region"        = var.aws_region
          "awslogs-stream-prefix" = "ecs"
        }
      }
      
      healthCheck = {
        command     = ["CMD-SHELL", "curl -f http://localhost/health || exit 1"]
        interval    = 30
        timeout     = 5
        retries     = 3
        startPeriod = 60
      }
    }
  ])
}

# ECS Service - Task Service
resource "aws_ecs_service" "task_service" {
  name            = "task-service"
  cluster         = aws_ecs_cluster.taskflow.id
  task_definition = aws_ecs_task_definition.task_service.arn
  desired_count   = 2
  launch_type     = "FARGATE"

  network_configuration {
    subnets          = module.vpc.private_subnet_ids
    security_groups  = [aws_security_group.ecs_tasks.id]
    assign_public_ip = false
  }

  load_balancer {
    target_group_arn = module.alb.target_groups["task-service"].arn
    container_name   = "task-service"
    container_port   = 80
  }

  depends_on = [module.alb]
}

# Auto Scaling
resource "aws_appautoscaling_target" "task_service" {
  max_capacity       = 10
  min_capacity       = 2
  resource_id        = "service/${aws_ecs_cluster.taskflow.name}/${aws_ecs_service.task_service.name}"
  scalable_dimension = "ecs:service:DesiredCount"
  service_namespace  = "ecs"
}

resource "aws_appautoscaling_policy" "task_service_cpu" {
  name               = "task-service-cpu-scaling"
  policy_type        = "TargetTrackingScaling"
  resource_id        = aws_appautoscaling_target.task_service.resource_id
  scalable_dimension = aws_appautoscaling_target.task_service.scalable_dimension
  service_namespace  = aws_appautoscaling_target.task_service.service_namespace

  target_tracking_scaling_policy_configuration {
    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageCPUUtilization"
    }
    target_value = 70.0
  }
}

# Outputs
output "alb_dns_name" {
  value       = module.alb.dns_name
  description = "Load Balancer DNS name"
}

output "database_endpoint" {
  value       = module.database.endpoint
  description = "Database endpoint"
}
```

#### Deployment Commands

```bash
# Initialize Terraform
cd infrastructure/terraform/environments/aws
terraform init

# Plan deployment
terraform plan -var-file="production.tfvars" -out=tfplan

# Apply deployment
terraform apply tfplan

# Get outputs
terraform output alb_dns_name

# Destroy (cleanup)
terraform destroy -var-file="production.tfvars"
```

**Cost Estimate (AWS):**
- Free Tier: $0-10/month
- Small Production: $50-100/month
- Medium Scale: $200-500/month

---

### 🔷 Azure Deployment (Container Apps)

**Best for:** Serverless containers with built-in scaling, KEDA support

#### Infrastructure (Terraform)

```hcl
# infrastructure/terraform/environments/azure/main.tf

provider "azurerm" {
  features {}
}

# Resource Group
resource "azurerm_resource_group" "taskflow" {
  name     = "taskflow-rg"
  location = var.azure_region
}

# Container Registry
resource "azurerm_container_registry" "taskflow" {
  name                = "taskflowacr"
  resource_group_name = azurerm_resource_group.taskflow.name
  location            = azurerm_resource_group.taskflow.location
  sku                 = "Basic"
  admin_enabled       = true
}

# Azure Database for PostgreSQL
resource "azurerm_postgresql_flexible_server" "taskflow" {
  name                = "taskflow-postgres"
  resource_group_name = azurerm_resource_group.taskflow.name
  location            = azurerm_resource_group.taskflow.location
  
  sku_name   = "B_Standard_B1ms" # Burstable, cost-effective
  storage_mb = 32768
  version    = "15"
  
  administrator_login    = "taskflowadmin"
  administrator_password = var.db_password
  
  backup_retention_days = 7
  geo_redundant_backup_enabled = false
}

resource "azurerm_postgresql_flexible_server_database" "taskflow" {
  name      = "taskflow"
  server_id = azurerm_postgresql_flexible_server.taskflow.id
  charset   = "UTF8"
  collation = "en_US.utf8"
}

# Azure Cache for Redis
resource "azurerm_redis_cache" "taskflow" {
  name                = "taskflow-redis"
  location            = azurerm_resource_group.taskflow.location
  resource_group_name = azurerm_resource_group.taskflow.name
  
  capacity            = 0
  family              = "C"
  sku_name            = "Basic"
  enable_non_ssl_port = false
  minimum_tls_version = "1.2"
}

# Service Bus
resource "azurerm_servicebus_namespace" "taskflow" {
  name                = "taskflow-servicebus"
  location            = azurerm_resource_group.taskflow.location
  resource_group_name = azurerm_resource_group.taskflow.name
  sku                 = "Standard"
}

resource "azurerm_servicebus_queue" "events" {
  name         = "taskflow-events"
  namespace_id = azurerm_servicebus_namespace.taskflow.id
  
  enable_partitioning = false
  max_size_in_megabytes = 1024
}

# Container Apps Environment
resource "azurerm_container_app_environment" "taskflow" {
  name                = "taskflow-env"
  location            = azurerm_resource_group.taskflow.location
  resource_group_name = azurerm_resource_group.taskflow.name
  
  log_analytics_workspace_id = azurerm_log_analytics_workspace.taskflow.id
}

# Log Analytics Workspace
resource "azurerm_log_analytics_workspace" "taskflow" {
  name                = "taskflow-logs"
  location            = azurerm_resource_group.taskflow.location
  resource_group_name = azurerm_resource_group.taskflow.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

# Container App - Task Service
resource "azurerm_container_app" "task_service" {
  name                         = "task-service"
  container_app_environment_id = azurerm_container_app_environment.taskflow.id
  resource_group_name          = azurerm_resource_group.taskflow.name
  revision_mode                = "Single"

  template {
    container {
      name   = "task-service"
      image  = "${azurerm_container_registry.taskflow.login_server}/task-service:latest"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }
      
      env {
        name  = "CloudProvider"
        value = "Azure"
      }
      
      env {
        name        = "ConnectionStrings__PostgreSQL"
        secret_name = "db-connection"
      }
      
      env {
        name        = "ConnectionStrings__Redis"
        secret_name = "redis-connection"
      }
    }

    min_replicas = 1
    max_replicas = 10
  }

  ingress {
    external_enabled = true
    target_port      = 80
    
    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  secret {
    name  = "db-connection"
    value = "Host=${azurerm_postgresql_flexible_server.taskflow.fqdn};Database=taskflow;Username=${azurerm_postgresql_flexible_server.taskflow.administrator_login};Password=${var.db_password}"
  }
  
  secret {
    name  = "redis-connection"
    value = "${azurerm_redis_cache.taskflow.hostname}:${azurerm_redis_cache.taskflow.ssl_port},password=${azurerm_redis_cache.taskflow.primary_access_key},ssl=True"
  }

  identity {
    type = "SystemAssigned"
  }
}

# Auto-scaling rule (CPU based)
resource "azurerm_container_app_scaling_rule" "task_service_cpu" {
  name              = "cpu-scaling"
  container_app_id  = azurerm_container_app.task_service.id
  
  scale_trigger {
    type = "cpu"
    metadata = {
      type  = "Utilization"
      value = "70"
    }
  }
}

# Outputs
output "task_service_url" {
  value       = azurerm_container_app.task_service.latest_revision_fqdn
  description = "Task Service URL"
}

output "database_fqdn" {
  value       = azurerm_postgresql_flexible_server.taskflow.fqdn
  description = "Database FQDN"
}
```

#### Deployment Commands

```bash
# Initialize Terraform
cd infrastructure/terraform/environments/azure
terraform init

# Login to Azure
az login

# Plan deployment
terraform plan -var-file="production.tfvars" -out=tfplan

# Apply deployment
terraform apply tfplan

# Get outputs
terraform output task_service_url

# Cleanup
terraform destroy -var-file="production.tfvars"
```

**Cost Estimate (Azure):**
- Free Tier: $0/month (limited)
- Small Production: $40-80/month
- Medium Scale: $150-400/month

---

### 🔶 GCP Deployment (Cloud Run)

**Best for:** Fully managed serverless, scale to zero, pay per request

#### Infrastructure (Terraform)

```hcl
# infrastructure/terraform/environments/gcp/main.tf

provider "google" {
  project = var.gcp_project_id
  region  = var.gcp_region
}

# Cloud SQL (PostgreSQL)
resource "google_sql_database_instance" "taskflow" {
  name             = "taskflow-postgres"
  database_version = "POSTGRES_15"
  region           = var.gcp_region
  
  settings {
    tier = "db-f1-micro" # Shared-core, cost-effective
    
    backup_configuration {
      enabled = true
      start_time = "03:00"
    }
    
    ip_configuration {
      ipv4_enabled = true
      authorized_networks {
        name  = "all"
        value = "0.0.0.0/0"
      }
    }
    
    database_flags {
      name  = "max_connections"
      value = "100"
    }
  }
  
  deletion_protection = false
}

resource "google_sql_database" "taskflow" {
  name     = "taskflow"
  instance = google_sql_database_instance.taskflow.name
}

resource "google_sql_user" "taskflow" {
  name     = "taskflow_user"
  instance = google_sql_database_instance.taskflow.name
  password = var.db_password
}

# Memorystore (Redis)
resource "google_redis_instance" "taskflow" {
  name           = "taskflow-redis"
  tier           = "BASIC"
  memory_size_gb = 1
  region         = var.gcp_region
  
  redis_version = "REDIS_7_0"
  
  authorized_network = google_compute_network.taskflow.id
}

# VPC Network
resource "google_compute_network" "taskflow" {
  name                    = "taskflow-network"
  auto_create_subnetworks = true
}

# Pub/Sub Topic
resource "google_pubsub_topic" "taskflow_events" {
  name = "taskflow-events"
  
  message_retention_duration = "86400s" # 1 day
}

# Pub/Sub Subscription
resource "google_pubsub_subscription" "taskflow_events" {
  name  = "taskflow-events-sub"
  topic = google_pubsub_topic.taskflow_events.name
  
  ack_deadline_seconds = 300
  
  retry_policy {
    minimum_backoff = "10s"
    maximum_backoff = "600s"
  }
}

# Container Registry
resource "google_artifact_registry_repository" "taskflow" {
  location      = var.gcp_region
  repository_id = "taskflow"
  format        = "DOCKER"
}

# Cloud Run Service - Task Service
resource "google_cloud_run_service" "task_service" {
  name     = "task-service"
  location = var.gcp_region

  template {
    spec {
      containers {
        image = "${var.gcp_region}-docker.pkg.dev/${var.gcp_project_id}/taskflow/task-service:latest"
        
        ports {
          container_port = 80
        }
        
        env {
          name  = "ASPNETCORE_ENVIRONMENT"
          value = "Production"
        }
        
        env {
          name  = "CloudProvider"
          value = "GCP"
        }
        
        env {
          name = "ConnectionStrings__PostgreSQL"
          value_from {
            secret_key_ref {
              name = google_secret_manager_secret.db_connection.secret_id
              key  = "latest"
            }
          }
        }
        
        env {
          name  = "ConnectionStrings__Redis"
          value = "${google_redis_instance.taskflow.host}:${google_redis_instance.taskflow.port}"
        }
        
        resources {
          limits = {
            cpu    = "1000m"
            memory = "512Mi"
          }
        }
      }
      
      container_concurrency = 80
      timeout_seconds       = 300
    }
    
    metadata {
      annotations = {
        "autoscaling.knative.dev/minScale" = "1"
        "autoscaling.knative.dev/maxScale" = "10"
        "run.googleapis.com/cpu-throttling" = "false"
      }
    }
  }

  traffic {
    percent         = 100
    latest_revision = true
  }
}

# Cloud Run IAM (Public access)
resource "google_cloud_run_service_iam_member" "task_service_public" {
  service  = google_cloud_run_service.task_service.name
  location = google_cloud_run_service.task_service.location
  role     = "roles/run.invoker"
  member   = "allUsers"
}

# Secret Manager
resource "google_secret_manager_secret" "db_connection" {
  secret_id = "db-connection-string"
  
  replication {
    automatic = true
  }
}

resource "google_secret_manager_secret_version" "db_connection" {
  secret = google_secret_manager_secret.db_connection.id
  secret_data = "Host=${google_sql_database_instance.taskflow.public_ip_address};Database=taskflow;Username=${google_sql_user.taskflow.name};Password=${var.db_password}"
}

# Cloud Load Balancer (for multiple services)
resource "google_compute_backend_service" "task_service" {
  name = "task-service-backend"
  
  backend {
    group = google_cloud_run_service.task_service.status[0].url
  }
  
  health_checks = [google_compute_health_check.task_service.id]
}

resource "google_compute_health_check" "task_service" {
  name = "task-service-health"
  
  http_health_check {
    port         = 80
    request_path = "/health"
  }
}

# Outputs
output "task_service_url" {
  value       = google_cloud_run_service.task_service.status[0].url
  description = "Task Service URL"
}

output "database_ip" {
  value       = google_sql_database_instance.taskflow.public_ip_address
  description = "Database IP"
}
```

#### Deployment Commands

```bash
# Initialize Terraform
cd infrastructure/terraform/environments/gcp
terraform init

# Authenticate
gcloud auth application-default login

# Plan deployment
terraform plan -var-file="production.tfvars" -out=tfplan

# Apply deployment
terraform apply tfplan

# Get outputs
terraform output task_service_url

# Cleanup
terraform destroy -var-file="production.tfvars"
```

**Cost Estimate (GCP):**
- Free Tier: $0/month (generous limits)
- Small Production: $30-70/month
- Medium Scale: $120-350/month

---

## 🔄 CI/CD Pipelines

### GitHub Actions (Multi-Cloud)

```yaml
# .github/workflows/ci-cd.yml
name: CI/CD Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  DOTNET_VERSION: '8.0.x'
  CLOUD_PROVIDER: ${{ secrets.CLOUD_PROVIDER }} # AWS, Azure, GCP, Local

jobs:
  # ============================================
  # Build & Test
  # ============================================
  build-and-test:
    name: Build and Test
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore --configuration Release
      
      - name: Unit Tests
        run: dotnet test --no-build --configuration Release --filter Category=Unit --logger "trx;LogFileName=unit-tests.trx"
      
      - name: Integration Tests
        run: dotnet test --no-build --configuration Release --filter Category=Integration --logger "trx;LogFileName=integration-tests.trx"
      
      - name: Architecture Tests
        run: dotnet test --no-build --configuration Release --filter Category=Architecture --logger "trx;LogFileName=architecture-tests.trx"
      
      - name: Code Coverage
        run: |
          dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=coverage.xml
          dotnet tool install --global dotnet-reportgenerator-globaltool
          reportgenerator -reports:coverage.xml -targetdir:coverage-report -reporttypes:Html
      
      - name: Upload Test Results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: test-results
          path: |
            **/*.trx
            coverage-report/
      
      - name: Publish Code Coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage.xml
          flags: unittests
          name: codecov-taskflow

  # ============================================
  # Docker Build
  # ============================================
  docker-build:
    name: Build Docker Images
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.event_name == 'push'
    
    strategy:
      matrix:
        service: [task-service, user-service, identity-service, api-gateway]
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v2
      
      - name: Build Image
        run: |
          docker build -t ${{ matrix.service }}:${{ github.sha }} \
            -f src/Services/${{ matrix.service }}/Dockerfile .
      
      - name: Save Image
        run: docker save ${{ matrix.service }}:${{ github.sha }} -o ${{ matrix.service }}.tar
      
      - name: Upload Image Artifact
        uses: actions/upload-artifact@v3
        with:
          name: docker-images
          path: ${{ matrix.service }}.tar

  # ============================================
  # Deploy to AWS
  # ============================================
  deploy-aws:
    name: Deploy to AWS
    needs: docker-build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main' && env.CLOUD_PROVIDER == 'AWS'
    environment: production-aws
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Configure AWS Credentials
        uses: aws-actions/configure-aws-credentials@v2
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: ${{ secrets.AWS_REGION }}
      
      - name: Login to Amazon ECR
        id: login-ecr
        uses: aws-actions/amazon-ecr-login@v1
      
      - name: Download Docker Images
        uses: actions/download-artifact@v3
        with:
          name: docker-images
      
      - name: Load and Push Images to ECR
        env:
          ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
          ECR_REPOSITORY: taskflow
          IMAGE_TAG: ${{ github.sha }}
        run: |
          for service in task-service user-service identity-service api-gateway; do
            docker load -i ${service}.tar
            docker tag ${service}:${IMAGE_TAG} ${ECR_REGISTRY}/${ECR_REPOSITORY}-${service}:${IMAGE_TAG}
            docker tag ${service}:${IMAGE_TAG} ${ECR_REGISTRY}/${ECR_REPOSITORY}-${service}:latest
            docker push ${ECR_REGISTRY}/${ECR_REPOSITORY}-${service}:${IMAGE_TAG}
            docker push ${ECR_REGISTRY}/${ECR_REPOSITORY}-${service}:latest
          done
      
      - name: Setup Terraform
        uses: hashicorp/setup-terraform@v2
      
      - name: Terraform Init
        run: |
          cd infrastructure/terraform/environments/aws
          terraform init
      
      - name: Terraform Apply
        run: |
          cd infrastructure/terraform/environments/aws
          terraform apply -auto-approve \
            -var="image_tag=${{ github.sha }}" \
            -var-file="production.tfvars"
      
      - name: Update ECS Services
        run: |
          aws ecs update-service --cluster taskflow-cluster --service task-service --force-new-deployment
          aws ecs update-service --cluster taskflow-cluster --service user-service --force-new-deployment
          aws ecs update-service --cluster taskflow-cluster --service identity-service --force-new-deployment

  # ============================================
  # Deploy to Azure
  # ============================================
  deploy-azure:
    name: Deploy to Azure
    needs: docker-build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main' && env.CLOUD_PROVIDER == 'Azure'
    environment: production-azure
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Azure Login
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      
      - name: Login to Azure Container Registry
        run: |
          az acr login --name ${{ secrets.AZURE_REGISTRY_NAME }}
      
      - name: Download Docker Images
        uses: actions/download-artifact@v3
        with:
          name: docker-images
      
      - name: Push Images to ACR
        env:
          REGISTRY: ${{ secrets.AZURE_REGISTRY_NAME }}.azurecr.io
          IMAGE_TAG: ${{ github.sha }}
        run: |
          for service in task-service user-service identity-service api-gateway; do
            docker load -i ${service}.tar
            docker tag ${service}:${IMAGE_TAG} ${REGISTRY}/${service}:${IMAGE_TAG}
            docker tag ${service}:${IMAGE_TAG} ${REGISTRY}/${service}:latest
            docker push ${REGISTRY}/${service}:${IMAGE_TAG}
            docker push ${REGISTRY}/${service}:latest
          done
      
      - name: Setup Terraform
        uses: hashicorp/setup-terraform@v2
      
      - name: Terraform Init
        run: |
          cd infrastructure/terraform/environments/azure
          terraform init
      
      - name: Terraform Apply
        run: |
          cd infrastructure/terraform/environments/azure
          terraform apply -auto-approve \
            -var="image_tag=${{ github.sha }}" \
            -var-file="production.tfvars"
      
      - name: Update Container Apps
        run: |
          az containerapp update \
            --name task-service \
            --resource-group taskflow-rg \
            --image ${{ secrets.AZURE_REGISTRY_NAME }}.azurecr.io/task-service:${{ github.sha }}

  # ============================================
  # Deploy to GCP
  # ============================================
  deploy-gcp:
    name: Deploy to GCP
    needs: docker-build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main' && env.CLOUD_PROVIDER == 'GCP'
    environment: production-gcp
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Authenticate to Google Cloud
        uses: google-github-actions/auth@v1
        with:
          credentials_json: ${{ secrets.GCP_SA_KEY }}
      
      - name: Set up Cloud SDK
        uses: google-github-actions/setup-gcloud@v1
      
      - name: Configure Docker for Artifact Registry
        run: |
          gcloud auth configure-docker ${{ secrets.GCP_REGION }}-docker.pkg.dev
      
      - name: Download Docker Images
        uses: actions/download-artifact@v3
        with:
          name: docker-images
      
      - name: Push Images to Artifact Registry
        env:
          REGISTRY: ${{ secrets.GCP_REGION }}-docker.pkg.dev/${{ secrets.GCP_PROJECT_ID }}/taskflow
          IMAGE_TAG: ${{ github.sha }}
        run: |
          for service in task-service user-service identity-service api-gateway; do
            docker load -i ${service}.tar
            docker tag ${service}:${IMAGE_TAG} ${REGISTRY}/${service}:${IMAGE_TAG}
            docker tag ${service}:${IMAGE_TAG} ${REGISTRY}/${service}:latest
            docker push ${REGISTRY}/${service}:${IMAGE_TAG}
            docker push ${REGISTRY}/${service}:latest
          done
      
      - name: Setup Terraform
        uses: hashicorp/setup-terraform@v2
      
      - name: Terraform Init
        run: |
          cd infrastructure/terraform/environments/gcp
          terraform init
      
      - name: Terraform Apply
        run: |
          cd infrastructure/terraform/environments/gcp
          terraform apply -auto-approve \
            -var="image_tag=${{ github.sha }}" \
            -var-file="production.tfvars"
      
      - name: Deploy to Cloud Run
        run: |
          gcloud run deploy task-service \
            --image ${{ secrets.GCP_REGION }}-docker.pkg.dev/${{ secrets.GCP_PROJECT_ID }}/taskflow/task-service:${{ github.sha }} \
            --region ${{ secrets.GCP_REGION }} \
            --platform managed

  # ============================================
  # Smoke Tests (Post-Deployment)
  # ============================================
  smoke-tests:
    name: Smoke Tests
    needs: [deploy-aws, deploy-azure, deploy-gcp]
    runs-on: ubuntu-latest
    if: always() && (needs.deploy-aws.result == 'success' || needs.deploy-azure.result == 'success' || needs.deploy-gcp.result == 'success')
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Run Smoke Tests
        run: |
          chmod +x ./scripts/smoke-tests.sh
          ./scripts/smoke-tests.sh ${{ secrets.DEPLOYMENT_URL }}
      
      - name: Notify Slack on Failure
        if: failure()
        uses: slackapi/slack-github-action@v1
        with:
          payload: |
            {
              "text": "❌ Deployment smoke tests failed for TaskFlow"
            }
        env:
          SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}
```

### Azure DevOps Pipeline

```yaml
# azure-pipelines.yml
trigger:
  branches:
    include:
      - main
      - develop

pool:
  vmImage: 'ubuntu-latest'

variables:
  dotnetVersion: '8.0.x'
  buildConfiguration: 'Release'

stages:
  - stage: Build
    displayName: 'Build and Test'
    jobs:
      - job: BuildAndTest
        steps:
          - task: UseDotNet@2
            inputs:
              version: $(dotnetVersion)
          
          - script: dotnet restore
            displayName: 'Restore packages'
          
          - script: dotnet build --configuration $(buildConfiguration)
            displayName: 'Build solution'
          
          - script: dotnet test --configuration $(buildConfiguration) --logger trx --collect:"XPlat Code Coverage"
            displayName: 'Run tests'
          
          - task: PublishTestResults@2
            inputs:
              testResultsFormat: 'VSTest'
              testResultsFiles: '**/*.trx'
          
          - task: PublishCodeCoverageResults@1
            inputs:
              codeCoverageTool: 'Cobertura'
              summaryFileLocation: '$(Agent.TempDirectory)/**/coverage.cobertura.xml'

  - stage: Deploy
    displayName: 'Deploy to Azure'
    dependsOn: Build
    condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
    jobs:
      - deployment: DeployToAzure
        environment: 'production'
        strategy:
          runOnce:
            deploy:
              steps:
                - task: Docker@2
                  inputs:
                    containerRegistry: 'taskflow-acr'
                    repository: 'task-service'
                    command: 'buildAndPush'
                    Dockerfile: 'src/Services/Task/TaskFlow.Task.API/Dockerfile'
                    tags: |
                      $(Build.BuildId)
                      latest
                
                - task: AzureCLI@2
                  inputs:
                    azureSubscription: 'taskflow-subscription'
                    scriptType: 'bash'
                    scriptLocation: 'inlineScript'
                    inlineScript: |
                      az containerapp update \
                        --name task-service \
                        --resource-group taskflow-rg \
                        --image taskflowacr.azurecr.io/task-service:$(Build.BuildId)
```

---

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific category
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
```

**Test Coverage: 87%**

---

## 📚 Documentation

### Architecture
- [Clean Architecture](docs/architecture/clean-architecture.md)
- [Microservices Patterns](docs/architecture/microservices-patterns.md)
- [Event-Driven Design](docs/architecture/event-driven.md)

### Patterns
- [CQRS Implementation](docs/patterns/cqrs.md)
- [Idempotency Strategies](docs/patterns/idempotency.md)
- [API Versioning](docs/patterns/api-versioning.md)

### Deployment
- [Local Development](docs/deployment/local.md)
- [AWS Deployment](docs/deployment/aws.md)
- [Azure Deployment](docs/deployment/azure.md)
- [GCP Deployment](docs/deployment/gcp.md)

---

## 🤝 Contributing

Contributions welcome! See [CONTRIBUTING.md](CONTRIBUTING.md)

---

## 📊 Roadmap

- ✅ Core microservices (4 services)
- ✅ Clean Architecture + DDD
- ✅ CQRS with MediatR
- ✅ Event-driven with MassTransit
- ✅ Idempotency (5 strategies)
- ✅ API versioning (4 strategies)
- ✅ Multi-cloud deployment (AWS, Azure, GCP)
- ✅ Docker containerization
- ✅ 87% test coverage
- ✅ Terraform IaC
- ✅ CI/CD (GitHub Actions, Azure DevOps)
- 🚧 Kubernetes deployment
- 🚧 Service mesh (Istio/Linkerd)
- 📋 GraphQL API gateway
- 📋 Distributed tracing (Jaeger)

---

## 📄 License

MIT License - Copyright (c) 2025 mahfuj

See [LICENSE](LICENSE) for details.

---

## 🌟 Support

If this project helps you, please ⭐ **star** it on GitHub!

**Built with ❤️ for the microservices community**

---

## 📞 Contact & Support

- **Issues:** [GitHub Issues](https://github.com/mahfuj/TaskFlow-Microservices/issues)
- **Discussions:** [GitHub Discussions](https://github.com/mahfuj/TaskFlow-Microservices/discussions)
- **Documentation:** [Full Documentation](docs/)

---

**Keywords:** microservices, .NET 8, ASP.NET Core, Clean Architecture, CQRS, DDD, Event-Driven, Docker, Kubernetes, AWS, Azure, GCP, Terraform, CI/CD, Idempotency, API Versioning, gRPC, MassTransit, PostgreSQL, Redis, Enterprise Architecture
