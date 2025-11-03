# API Gateway Configuration - Quick Summary

## Problem Solved

**Issue**: Gemini messed with appsettings in API Gateway, causing potential conflicts across microservices.

**Solution**: Established API Gateway as the **single source of truth** for:
- ✅ Infrastructure technology choices (MassTransit/Rebus/MediatR)
- ✅ EventBus mode (InMemory/Persistent/Hybrid)
- ✅ Service hosting locations (URLs/ports)
- ✅ Messaging provider (RabbitMQ/AWS SQS/Azure Service Bus)

---

## What Was Created

### 1. Base Configuration (`appsettings.json`)

**New Section**: `Infrastructure` - Defines architectural choices
```json
{
  "Infrastructure": {
    "MessagingTechnology": "MassTransit",  // Single source of truth!
    "EventBusMode": "Hybrid"               // Prevents conflicts!
  }
}
```

**Complete Configuration Includes**:
- ✅ Infrastructure (MessagingTechnology, EventBusMode)
- ✅ Messaging (RabbitMQ/MassTransit config)
- ✅ Consul (Service Discovery)
- ✅ ReverseProxy (YARP routing to 5 microservices)
- ✅ CORS, Rate Limiting, Jaeger, Seq

### 2. Environment-Specific Configurations

| File | Environment | EventBus Mode | Service URLs |
|------|-------------|---------------|--------------|
| `appsettings.Development.json` | Local dev | InMemory (fast) | localhost:5001-5005 |
| `appsettings.Staging.json` | Staging | Hybrid (testing) | *.staging.internal |
| `appsettings.Production.json` | Production | Persistent (reliable) | *.prod.internal + replicas |

### 3. Cloud-Provider Configurations

| File | Cloud | Messaging Provider | Service URLs |
|------|-------|-------------------|--------------|
| `appsettings.Aws.Production.json` | AWS | **AWS SQS** | *.aws.internal |
| `appsettings.Azure.Production.json` | Azure | **Azure Service Bus** | *.azure.internal |
| `appsettings.Gcp.Production.json` | GCP | RabbitMQ (on GCP) | *.gcp.internal |

### 4. Comprehensive Documentation

**Created**: `docs/API_GATEWAY_CONFIGURATION.md`

**Contents**:
- Configuration structure and hierarchy
- Infrastructure options explained
- Environment-specific configurations
- Cloud provider configurations
- How downstream services consume these settings
- Troubleshooting guide
- Best practices

---

## Key Features

### 🎯 Single Source of Truth

**Before** (Problem):
```
❌ User Service: Uses MassTransit
❌ Catalog Service: Uses Rebus (CONFLICT!)
❌ Order Service: Uses MediatR (CONFLICT!)
```

**After** (Solution):
```
✅ API Gateway: Defines "MassTransit" + "Hybrid"
✅ All services read from gateway config
✅ No conflicts, consistent architecture
```

### 🌍 Multi-Cloud Support

**Single Configuration Change**:
```bash
# Switch from AWS to Azure
export CLOUD_PROVIDER=Azure
# Done! Messaging changes from SQS → Azure Service Bus
```

### 🔄 Environment Flexibility

| Environment | EventBus | Outbox | Rate Limit |
|-------------|----------|--------|------------|
| Development | InMemory | Disabled | 100/min |
| Staging | Hybrid | Enabled | 200/min |
| Production | Persistent | Enabled | 500/min |

### 📍 Service Routing (YARP)

**5 Microservices Configured**:
1. User Service: `/api/v1/users/*`, `/api/v1/identity/*`
2. Catalog Service: `/api/v1/catalog/*`, `/api/v1/products/*`
3. Order Service: `/api/v1/orders/*`
4. Notification Service: `/api/v1/notifications/*`
5. Task Service: `/api/v1/tasks/*`

**Features**:
- Path transformation (external `/api/v1/*` → internal `/api/*`)
- Health checks (every 10 seconds)
- Load balancing (Production: multiple replicas)

---

## How Downstream Services Use This

### Option 1: Read from Gateway Configuration Service

```csharp
// User.API/Program.cs
builder.Configuration.AddJsonFile(
    "http://api-gateway/config/infrastructure.json",
    optional: false
);

var messagingTech = builder.Configuration["Infrastructure:MessagingTechnology"];
// Use "MassTransit" to configure services
```

### Option 2: Environment Variables (Simpler)

**Set in docker-compose.yml**:
```yaml
environment:
  - MESSAGING_TECHNOLOGY=MassTransit
  - EVENTBUS_MODE=Hybrid
```

**Read in downstream service**:
```csharp
var messagingTech = Environment.GetEnvironmentVariable("MESSAGING_TECHNOLOGY");
```

---

## Configuration Loading Order

```
1. appsettings.json (base)
2. appsettings.{Environment}.json
3. appsettings.{CloudProvider}.{Environment}.json
4. Environment variables
5. Command-line arguments
```

**Example**: AWS Production
```
appsettings.json
→ appsettings.Production.json
→ appsettings.Aws.Production.json
→ AWS_ACCESS_KEY env variable
```

---

## Quick Start

### 1. Choose Your Environment

```bash
# Development
export ASPNETCORE_ENVIRONMENT=Development

# Staging
export ASPNETCORE_ENVIRONMENT=Staging

# Production
export ASPNETCORE_ENVIRONMENT=Production
```

### 2. Choose Your Cloud Provider (Optional)

```bash
# AWS
export CLOUD_PROVIDER=Aws

# Azure
export CLOUD_PROVIDER=Azure

# GCP
export CLOUD_PROVIDER=Gcp
```

### 3. Set Secrets

```bash
# RabbitMQ
export RABBITMQ_USERNAME=admin
export RABBITMQ_PASSWORD=secure_password

# AWS (if using SQS)
export AWS_ACCESS_KEY=AKIA...
export AWS_SECRET_KEY=wJalr...

# Azure (if using Service Bus)
export AZURE_SERVICE_BUS_CONNECTION_STRING=Endpoint=sb://...
```

### 4. Run API Gateway

```bash
cd src/ApiGateway/TaskFlow.Gateway
dotnet run
```

The gateway will automatically load:
- `appsettings.json`
- `appsettings.Development.json` (or Staging/Production)
- `appsettings.Aws.Production.json` (if `CLOUD_PROVIDER=Aws`)

---

## Files Modified

### API Gateway
1. ✅ `src/ApiGateway/TaskFlow.Gateway/appsettings.json` (base config)
2. ✅ `src/ApiGateway/TaskFlow.Gateway/appsettings.Development.json`
3. ✅ `src/ApiGateway/TaskFlow.Gateway/appsettings.Staging.json`
4. ✅ `src/ApiGateway/TaskFlow.Gateway/appsettings.Production.json`
5. ✅ `src/ApiGateway/TaskFlow.Gateway/appsettings.Aws.Production.json`
6. ✅ `src/ApiGateway/TaskFlow.Gateway/appsettings.Azure.Production.json`
7. ✅ `src/ApiGateway/TaskFlow.Gateway/appsettings.Gcp.Production.json`

### Documentation
8. ✅ `docs/API_GATEWAY_CONFIGURATION.md` (comprehensive guide)
9. ✅ `API_GATEWAY_CONFIG_SUMMARY.md` (this file)

---

## Next Steps

### For API Gateway Team
1. ✅ Configuration structure established
2. ✅ Environment-specific configs created
3. ✅ Cloud provider configs created
4. ✅ Documentation written
5. ⏳ Test configuration loading in each environment
6. ⏳ Implement configuration endpoint (`GET /config/infrastructure`)

### For Downstream Services Team
1. ⏳ Update User.API to read from gateway config
2. ⏳ Update Catalog.API to read from gateway config
3. ⏳ Update Order.API to read from gateway config
4. ⏳ Update Notification.API to read from gateway config
5. ⏳ Remove conflicting local configurations
6. ⏳ Test end-to-end with gateway-driven config

---

## Benefits

### ✅ Consistency
- Single definition of MessagingTechnology prevents conflicts
- All services use the same EventBus mode

### ✅ Flexibility
- Change cloud provider with one environment variable
- Switch messaging technology without touching downstream services

### ✅ Maintainability
- Configuration centralized in API Gateway
- Easy to understand and modify

### ✅ Scalability
- Production supports multiple replicas with load balancing
- Environment-specific optimizations (InMemory in dev, Persistent in prod)

### ✅ Security
- Secrets via environment variables
- No hardcoded credentials

---

## Troubleshooting

**Q: Configuration not loading?**
→ Check `ASPNETCORE_ENVIRONMENT` and `CLOUD_PROVIDER` env vars

**Q: Services can't connect?**
→ Verify Consul is running and services are registered

**Q: Messaging not working?**
→ Check `Infrastructure.MessagingTechnology` matches `Messaging.Provider`

**Q: Routing not working?**
→ Check `ReverseProxy.Routes` and `Clusters` in appsettings

---

## Summary

**Problem**: Configuration chaos across microservices
**Solution**: API Gateway as single source of truth

**Result**:
- 🎯 One place to define infrastructure
- 🌍 Multi-cloud support (AWS, Azure, GCP)
- 🔄 Environment flexibility (Dev, Staging, Prod)
- 📍 Centralized service routing
- 🛡️ Cross-cutting concerns managed

**Status**: ✅ Configuration Complete, Ready for Integration Testing

---

**Last Updated**: 2025-11-03
**Related Docs**:
- [API_GATEWAY_CONFIGURATION.md](docs/API_GATEWAY_CONFIGURATION.md) - Full guide
- [CLAUDE.md](CLAUDE.md) - Project context
- [BuildingBlocks README](src/BuildingBlocks/README.md) - Framework-agnostic abstractions
