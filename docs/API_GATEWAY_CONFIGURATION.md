# API Gateway Configuration Guide

## Overview

The API Gateway serves as the **single source of truth** for the entire TaskFlow microservices architecture. It defines:

1. **Infrastructure choices** (Messaging technology, EventBus mode)
2. **Service locations** (URLs and ports)
3. **Routing rules** (Path-based routing to downstream services)
4. **Cross-cutting concerns** (CORS, Rate Limiting, Tracing, Logging)

This prevents configuration conflicts across downstream services and ensures consistency.

---

## Configuration Structure

### File Hierarchy

```
appsettings.json                          # Base configuration (Docker/Production defaults)
├── appsettings.Development.json          # Local development overrides
├── appsettings.Staging.json              # Staging environment overrides
├── appsettings.Production.json           # Production environment overrides
└── Cloud-specific overrides:
    ├── appsettings.Aws.Production.json   # AWS-specific settings
    ├── appsettings.Azure.Production.json # Azure-specific settings
    └── appsettings.Gcp.Production.json   # GCP-specific settings
```

### Loading Order

Configuration files are loaded in this order (later files override earlier ones):

1. `appsettings.json` (base)
2. `appsettings.{Environment}.json` (e.g., Development, Staging, Production)
3. `appsettings.{CloudProvider}.{Environment}.json` (e.g., Aws.Production)
4. Environment variables
5. Command-line arguments

**Example**: In AWS Production:
```
appsettings.json
→ appsettings.Production.json
→ appsettings.Aws.Production.json
→ Environment variables
```

---

## Infrastructure Configuration

### Section: `Infrastructure`

This section defines the **architectural choices** for the entire system.

```json
{
  "Infrastructure": {
    "MessagingTechnology": "MassTransit",  // Options: MassTransit, Rebus, MediatR
    "EventBusMode": "Hybrid"               // Options: InMemory, Persistent, Hybrid
  }
}
```

#### Messaging Technology Options

| Technology | Description | Use Case |
|------------|-------------|----------|
| **MassTransit** | Enterprise-grade message bus with support for RabbitMQ, Azure Service Bus, AWS SQS | Production microservices |
| **Rebus** | Lightweight message bus with RabbitMQ, Azure Service Bus, SQL Server | Medium-scale systems |
| **MediatR** | In-process messaging (no external broker) | Monoliths, testing, local development |

#### EventBus Mode Options

| Mode | Description | Use Case |
|------|-------------|----------|
| **InMemory** | Fast, immediate delivery. Events lost on crash. | Development, testing |
| **Persistent** | Outbox pattern. Guaranteed at-least-once delivery. | Production (critical events) |
| **Hybrid** | Both immediate + persistent. Best of both worlds. | Production (recommended) |

---

## Messaging Configuration

### Section: `Messaging`

Configuration for MassTransit with multi-cloud support.

```json
{
  "Messaging": {
    "Provider": "RabbitMQ",                // RabbitMQ, AmazonSQS, AzureServiceBus, InMemory
    "Host": "rabbitmq",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest",
    "Retry": {
      "RetryCount": 3,
      "InitialIntervalSeconds": 5,
      "IntervalIncrementSeconds": 5
    },
    "Outbox": {
      "Enabled": true,
      "QueryDelay": 100,
      "QueryLimit": 100
    },
    "UseMessageScheduler": true
  }
}
```

### Provider-Specific Settings

**RabbitMQ**:
- `Host`, `Port`, `VirtualHost`, `Username`, `Password`

**AWS SQS**:
- `AwsRegion`, `AwsAccessKey`, `AwsSecretKey`

**Azure Service Bus**:
- `ConnectionString`

---

## Service Discovery

### Section: `Consul`

Configuration for Consul service discovery.

```json
{
  "Consul": {
    "Host": "http://consul:8500",
    "ServiceName": "api-gateway",
    "ServiceId": "api-gateway-1",
    "ServiceAddress": "http://api-gateway",
    "ServicePort": 80,
    "HealthCheckInterval": "00:00:10",
    "DeregisterAfter": "00:01:00"
  }
}
```

---

## Reverse Proxy & Routing

### Section: `ReverseProxy`

YARP (Yet Another Reverse Proxy) configuration for routing requests to downstream services.

### Routes

Routes define how incoming requests are matched and forwarded.

```json
{
  "ReverseProxy": {
    "Routes": {
      "user-route": {
        "ClusterId": "user-service",
        "Match": {
          "Path": "/api/v1/users/{**catch-all}"
        },
        "Transforms": [
          {
            "PathPattern": "/api/users/{**catch-all}"
          }
        ]
      }
    }
  }
}
```

**Explanation**:
- Incoming: `GET /api/v1/users/123`
- Forwarded to: `http://user-service/api/users/123`

### Clusters

Clusters define the destination services and health checks.

```json
{
  "Clusters": {
    "user-service": {
      "Destinations": {
        "primary": {
          "Address": "http://user-service"
        }
      },
      "HealthCheck": {
        "Active": {
          "Enabled": true,
          "Interval": "00:00:10",
          "Timeout": "00:00:05",
          "Policy": "ConsecutiveFailures",
          "Path": "/health"
        }
      }
    }
  }
}
```

### Load Balancing (Production)

In production, multiple replicas can be configured:

```json
{
  "user-service": {
    "Destinations": {
      "primary": {
        "Address": "http://user-service.taskflow-prod.internal"
      },
      "replica1": {
        "Address": "http://user-service-replica1.taskflow-prod.internal"
      },
      "replica2": {
        "Address": "http://user-service-replica2.taskflow-prod.internal"
      }
    },
    "LoadBalancingPolicy": "RoundRobin"  // Or: LeastRequests, Random
  }
}
```

---

## Cross-Cutting Concerns

### CORS Configuration

```json
{
  "Cors": {
    "AllowedOrigins": [ "https://taskflow.com" ],
    "AllowedMethods": [ "GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS" ],
    "AllowedHeaders": [ "*" ],
    "AllowCredentials": true
  }
}
```

### Rate Limiting

```json
{
  "RateLimiting": {
    "Enabled": true,
    "PermitLimit": 500,       // Requests per window
    "Window": "00:01:00",     // 1 minute
    "QueueLimit": 50          // Queue size
  }
}
```

### Distributed Tracing (Jaeger)

```json
{
  "Jaeger": {
    "AgentHost": "jaeger",
    "AgentPort": 6831,
    "ServiceName": "api-gateway"
  }
}
```

### Centralized Logging (Seq)

```json
{
  "Seq": {
    "ServerUrl": "http://seq:5341",
    "ApiKey": ""
  }
}
```

---

## Environment-Specific Configurations

### Development

**File**: `appsettings.Development.json`

```json
{
  "Infrastructure": {
    "EventBusMode": "InMemory"  // Fast, no persistence
  },
  "Messaging": {
    "Host": "localhost",
    "Outbox": {
      "Enabled": false  // Faster iteration
    }
  },
  "ReverseProxy": {
    "Clusters": {
      "user-service": {
        "Destinations": {
          "primary": {
            "Address": "http://localhost:5001"  // Local ports
          }
        }
      }
    }
  }
}
```

### Staging

**File**: `appsettings.Staging.json`

```json
{
  "Infrastructure": {
    "EventBusMode": "Hybrid"  // Test production-like behavior
  },
  "Messaging": {
    "Host": "rabbitmq-staging.internal",
    "VirtualHost": "/staging",
    "Outbox": {
      "Enabled": true
    }
  }
}
```

### Production

**File**: `appsettings.Production.json`

```json
{
  "Infrastructure": {
    "EventBusMode": "Persistent"  // Guaranteed delivery
  },
  "Messaging": {
    "Host": "rabbitmq.taskflow-prod.internal",
    "VirtualHost": "/production",
    "Retry": {
      "RetryCount": 5,
      "InitialIntervalSeconds": 10
    }
  },
  "ReverseProxy": {
    "Clusters": {
      "user-service": {
        "Destinations": {
          "primary": { "Address": "http://user-service.taskflow-prod.internal" },
          "replica1": { "Address": "http://user-service-replica1.taskflow-prod.internal" },
          "replica2": { "Address": "http://user-service-replica2.taskflow-prod.internal" }
        },
        "LoadBalancingPolicy": "RoundRobin"
      }
    }
  }
}
```

---

## Cloud Provider Configurations

### AWS Production

**File**: `appsettings.Aws.Production.json`

**Key Changes**:
- Messaging provider: **AWS SQS**
- Service addresses: `*.aws.internal`

```json
{
  "Messaging": {
    "Provider": "AmazonSQS",
    "AwsRegion": "us-east-1",
    "AwsAccessKey": "${AWS_ACCESS_KEY}",
    "AwsSecretKey": "${AWS_SECRET_KEY}"
  },
  "ReverseProxy": {
    "Clusters": {
      "user-service": {
        "Destinations": {
          "primary": { "Address": "http://user-service.aws.internal" }
        }
      }
    }
  }
}
```

### Azure Production

**File**: `appsettings.Azure.Production.json`

**Key Changes**:
- Messaging provider: **Azure Service Bus**
- Service addresses: `*.azure.internal`

```json
{
  "Messaging": {
    "Provider": "AzureServiceBus",
    "ConnectionString": "${AZURE_SERVICE_BUS_CONNECTION_STRING}"
  },
  "ReverseProxy": {
    "Clusters": {
      "user-service": {
        "Destinations": {
          "primary": { "Address": "http://user-service.azure.internal" }
        }
      }
    }
  }
}
```

### GCP Production

**File**: `appsettings.Gcp.Production.json`

**Key Changes**:
- Messaging provider: **RabbitMQ** (hosted on GCP)
- Service addresses: `*.gcp.internal`

```json
{
  "Messaging": {
    "Provider": "RabbitMQ",
    "Host": "rabbitmq.gcp.prod.internal"
  },
  "ReverseProxy": {
    "Clusters": {
      "user-service": {
        "Destinations": {
          "primary": { "Address": "http://user-service.gcp.internal" }
        }
      }
    }
  }
}
```

---

## How Downstream Services Use This Configuration

Downstream services (User, Catalog, Order, Notification) should **NOT** define their own infrastructure choices. Instead, they should:

### 1. Read from Shared Configuration

**Option A**: Shared Configuration Server (Recommended)
```csharp
// In User.API/Program.cs
builder.Configuration.AddJsonFile(
    "http://api-gateway/config/infrastructure.json",
    optional: false,
    reloadOnChange: true
);

var messagingTech = builder.Configuration["Infrastructure:MessagingTechnology"];
var eventBusMode = builder.Configuration["Infrastructure:EventBusMode"];

// Use these values to configure services
if (messagingTech == "MassTransit")
{
    builder.Services.AddMassTransitMessaging(builder.Configuration);
}
```

**Option B**: Environment Variables (Simpler)
```bash
# Set in docker-compose.yml or Kubernetes
MESSAGING_TECHNOLOGY=MassTransit
EVENTBUS_MODE=Hybrid
```

```csharp
// In User.API/Program.cs
var messagingTech = Environment.GetEnvironmentVariable("MESSAGING_TECHNOLOGY");
var eventBusMode = Environment.GetEnvironmentVariable("EVENTBUS_MODE");
```

### 2. Example: Configure Messaging in Downstream Service

```csharp
// User.API/Program.cs
var messagingOptions = builder.Configuration
    .GetSection(MessagingOptions.SectionName)
    .Get<MessagingOptions>();

if (messagingOptions.Provider == MessagingProvider.RabbitMQ)
{
    builder.Services.AddMassTransit(x =>
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(messagingOptions.Host, messagingOptions.Port, messagingOptions.VirtualHost, h =>
            {
                h.Username(messagingOptions.Username);
                h.Password(messagingOptions.Password);
            });
        });
    });
}
```

---

## Environment Variables

Use environment variables for sensitive data:

```bash
# RabbitMQ
RABBITMQ_USERNAME=admin
RABBITMQ_PASSWORD=secure_password

# AWS
AWS_ACCESS_KEY=AKIA...
AWS_SECRET_KEY=wJalr...

# Azure
AZURE_SERVICE_BUS_CONNECTION_STRING=Endpoint=sb://...

# Seq
SEQ_API_KEY=your-api-key
```

In `appsettings.json`, reference them:
```json
{
  "Messaging": {
    "Username": "${RABBITMQ_USERNAME}",
    "Password": "${RABBITMQ_PASSWORD}"
  }
}
```

---

## Testing Configuration

### Unit Tests

```csharp
[Fact]
public void Should_Load_Infrastructure_Configuration()
{
    var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile("appsettings.Development.json")
        .Build();

    var messagingTech = config["Infrastructure:MessagingTechnology"];
    var eventBusMode = config["Infrastructure:EventBusMode"];

    Assert.Equal("MassTransit", messagingTech);
    Assert.Equal("InMemory", eventBusMode);
}
```

### Integration Tests

Use `appsettings.Test.json`:
```json
{
  "Infrastructure": {
    "MessagingTechnology": "MediatR",  // In-memory for tests
    "EventBusMode": "InMemory"
  },
  "Messaging": {
    "Provider": "InMemory"
  }
}
```

---

## Best Practices

1. **Single Source of Truth**
   - API Gateway defines infrastructure choices
   - Downstream services read from gateway config

2. **Environment Separation**
   - Use environment-specific files for different configurations
   - Never hardcode environment-specific values in code

3. **Secrets Management**
   - Use environment variables for sensitive data
   - Consider Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault

4. **Configuration Validation**
   - Validate configuration on startup
   - Fail fast if required settings are missing

5. **Documentation**
   - Keep this document updated
   - Document any custom configuration sections

---

## Troubleshooting

### Configuration Not Loading

**Problem**: Changes to `appsettings.json` not reflected.

**Solution**:
1. Check file is copied to output directory (Copy Always or Copy if newer)
2. Verify environment name: `echo $ASPNETCORE_ENVIRONMENT`
3. Check configuration loading order in `Program.cs`

### Service Discovery Failing

**Problem**: API Gateway can't reach downstream services.

**Solution**:
1. Check Consul is running: `docker ps | grep consul`
2. Verify service registration: `curl http://localhost:8500/v1/agent/services`
3. Check cluster addresses in `ReverseProxy.Clusters`

### Messaging Not Working

**Problem**: Events not being published/consumed.

**Solution**:
1. Verify RabbitMQ is running: `docker ps | grep rabbitmq`
2. Check credentials: `Username`, `Password`
3. Verify provider matches: `Infrastructure.MessagingTechnology` == `Messaging.Provider`

---

## Summary

The API Gateway configuration is the **central control plane** for the TaskFlow microservices architecture:

- **Infrastructure choices** defined once, consumed by all services
- **Environment-specific** configurations for Dev, Staging, Production
- **Cloud-agnostic** with provider-specific overrides
- **Cross-cutting concerns** (CORS, rate limiting, tracing, logging) managed centrally

**Next Steps**:
1. Review `appsettings.json` for base configuration
2. Customize environment-specific files as needed
3. Update downstream services to read from gateway config
4. Test configuration loading in each environment

---

**Last Updated**: 2025-11-03
**Maintainer**: TaskFlow Team
**Related Docs**:
- [CLAUDE.md](../CLAUDE.md)
- [BuildingBlocks Configuration](../src/BuildingBlocks/README.md)
- [Docker Configuration](../DOCKER.md)
