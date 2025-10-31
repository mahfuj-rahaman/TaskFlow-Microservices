# Messaging Provider Switching Guide

**Version**: 1.0.0
**Last Updated**: 2025-11-01
**Purpose**: Guide for switching between messaging providers without vendor lock-in

---

## 🎯 Overview

The TaskFlow Messaging BuildingBlock supports **4 messaging providers** with **zero code changes** required to switch between them:

1. **RabbitMQ** - Self-hosted message broker
2. **AWS SQS** - Amazon Simple Queue Service
3. **Azure Service Bus** - Microsoft Azure messaging
4. **In-Memory** - For testing and development

## 🔄 How to Switch Providers

### 1. RabbitMQ (Default)

**Use Case**: Self-hosted, full control, on-premises deployment

**appsettings.json**:
```json
{
  "Messaging": {
    "Provider": "RabbitMQ",
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest",
    "Retry": {
      "RetryCount": 3,
      "InitialIntervalSeconds": 5,
      "IntervalIncrementSeconds": 5
    },
    "UseMessageScheduler": true
  }
}
```

**Production Configuration**:
```json
{
  "Messaging": {
    "Provider": "RabbitMQ",
    "Host": "rabbitmq-cluster.production.com",
    "Port": 5672,
    "VirtualHost": "/taskflow",
    "Username": "${RABBITMQ_USERNAME}",
    "Password": "${RABBITMQ_PASSWORD}",
    "Retry": {
      "RetryCount": 5,
      "InitialIntervalSeconds": 10,
      "IntervalIncrementSeconds": 10
    }
  }
}
```

**NuGet Package Required**:
```xml
<PackageReference Include="MassTransit.RabbitMQ" Version="8.2.5" />
```

---

### 2. AWS SQS

**Use Case**: AWS cloud deployment, serverless, managed service

**appsettings.json** (with IAM role):
```json
{
  "Messaging": {
    "Provider": "AmazonSQS",
    "AwsRegion": "us-east-1",
    "Retry": {
      "RetryCount": 3,
      "InitialIntervalSeconds": 5,
      "IntervalIncrementSeconds": 5
    }
  }
}
```

**appsettings.json** (with access keys):
```json
{
  "Messaging": {
    "Provider": "AmazonSQS",
    "AwsRegion": "us-west-2",
    "AwsAccessKey": "${AWS_ACCESS_KEY}",
    "AwsSecretKey": "${AWS_SECRET_KEY}",
    "Retry": {
      "RetryCount": 3,
      "InitialIntervalSeconds": 5,
      "IntervalIncrementSeconds": 5
    }
  }
}
```

**NuGet Package Required**:
```xml
<PackageReference Include="MassTransit.AmazonSQS" Version="8.2.5" />
```

**AWS Configuration**:
- Create SQS queues for each consumer
- Configure IAM roles/policies
- Set up Dead Letter Queues (DLQ)

---

### 3. Azure Service Bus

**Use Case**: Azure cloud deployment, enterprise messaging

**appsettings.json**:
```json
{
  "Messaging": {
    "Provider": "AzureServiceBus",
    "ConnectionString": "Endpoint=sb://taskflow.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=${AZURE_SB_KEY}",
    "Retry": {
      "RetryCount": 3,
      "InitialIntervalSeconds": 5,
      "IntervalIncrementSeconds": 5
    }
  }
}
```

**NuGet Package Required**:
```xml
<PackageReference Include="MassTransit.Azure.ServiceBus.Core" Version="8.2.5" />
```

**Azure Configuration**:
- Create Service Bus namespace
- Configure topics and subscriptions
- Set up connection strings

---

### 4. In-Memory (Testing)

**Use Case**: Unit tests, integration tests, local development

**appsettings.Development.json**:
```json
{
  "Messaging": {
    "Provider": "InMemory",
    "Retry": {
      "RetryCount": 1,
      "InitialIntervalSeconds": 1,
      "IntervalIncrementSeconds": 1
    }
  }
}
```

**NuGet Package Required**:
- No additional package needed (built into MassTransit)

**Benefits**:
- ✅ No external dependencies
- ✅ Fast test execution
- ✅ No infrastructure setup
- ✅ Perfect for CI/CD pipelines

---

## 📝 Code Usage (Same for All Providers)

### Program.cs

```csharp
// No code changes needed! Just change appsettings.json
builder.Services.AddMassTransitMessaging(
    builder.Configuration,
    cfg =>
    {
        // Register your consumers
        cfg.AddConsumer<OrderCreatedEventConsumer>();
        cfg.AddConsumer<UserRegisteredEventConsumer>();
    });
```

### Publishing Messages

```csharp
// Same code works with ALL providers
public class CreateOrderCommandHandler
{
    private readonly IPublishEndpoint _publishEndpoint;

    public async Task<Result> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        // Create order...

        // Publish event (works with RabbitMQ, SQS, Azure SB, or In-Memory)
        await _publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount
        }, ct);

        return Result.Success();
    }
}
```

### Consuming Messages

```csharp
// Same code works with ALL providers
public sealed class OrderCreatedEventConsumer : BaseConsumer<OrderCreatedEvent>
{
    public OrderCreatedEventConsumer(ILogger<OrderCreatedEventConsumer> logger)
        : base(logger)
    {
    }

    protected override async Task ConsumeMessage(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;

        // Handle the message (same for all providers)
        await _notificationService.SendOrderConfirmationAsync(
            message.CustomerId,
            message.OrderId);
    }
}
```

---

## 🔀 Migration Scenarios

### Scenario 1: Development → Production (RabbitMQ → RabbitMQ)

**Step 1**: Update `appsettings.Production.json`
```json
{
  "Messaging": {
    "Host": "rabbitmq-prod.company.com",
    "VirtualHost": "/production",
    "Username": "${RABBITMQ_USER}",
    "Password": "${RABBITMQ_PASS}"
  }
}
```

**Step 2**: Deploy (no code changes needed)

---

### Scenario 2: On-Premises → AWS Cloud (RabbitMQ → SQS)

**Step 1**: Update `appsettings.json`
```json
{
  "Messaging": {
    "Provider": "AmazonSQS",  // Changed from RabbitMQ
    "AwsRegion": "us-east-1"
  }
}
```

**Step 2**: Add NuGet package
```bash
dotnet add package MassTransit.AmazonSQS
```

**Step 3**: Deploy (no code changes needed)

**Step 4**: Clean up old RabbitMQ infrastructure

---

### Scenario 3: Multi-Cloud Strategy (Azure + AWS)

**User Service** (on Azure):
```json
{
  "Messaging": {
    "Provider": "AzureServiceBus",
    "ConnectionString": "Endpoint=sb://..."
  }
}
```

**Order Service** (on AWS):
```json
{
  "Messaging": {
    "Provider": "AmazonSQS",
    "AwsRegion": "us-east-1"
  }
}
```

**Result**: Each service uses its cloud provider's native messaging, but **same code**!

---

## 📊 Provider Comparison

| Feature | RabbitMQ | AWS SQS | Azure Service Bus | In-Memory |
|---------|----------|---------|-------------------|-----------|
| **Cost** | Infrastructure only | Pay per message | Pay per message | Free |
| **Setup Complexity** | Medium | Low | Low | None |
| **Throughput** | Very High | High | High | Very High |
| **Latency** | Very Low | Medium | Low | Very Low |
| **Message Size** | 128 MB | 256 KB | 256 KB | Unlimited |
| **Message Ordering** | Yes (per queue) | FIFO queues | Yes (sessions) | Yes |
| **Dead Letter Queue** | Yes | Yes | Yes | No |
| **Delayed Messages** | Yes (plugin) | Yes | Yes | Yes |
| **Best For** | On-premises, full control | AWS ecosystem | Azure ecosystem | Testing |

---

## ⚙️ Environment-Specific Configuration

### Using Environment Variables

**appsettings.json**:
```json
{
  "Messaging": {
    "Provider": "${MESSAGING_PROVIDER:RabbitMQ}",
    "Host": "${RABBITMQ_HOST:localhost}",
    "AwsRegion": "${AWS_REGION:us-east-1}",
    "ConnectionString": "${AZURE_SB_CONNECTION_STRING}"
  }
}
```

**Docker Compose**:
```yaml
environment:
  - MESSAGING_PROVIDER=RabbitMQ
  - RABBITMQ_HOST=rabbitmq
  - RABBITMQ_PORT=5672
```

**Kubernetes ConfigMap**:
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: messaging-config
data:
  MESSAGING_PROVIDER: "AmazonSQS"
  AWS_REGION: "us-east-1"
```

---

## 🧪 Testing Strategy

### Unit Tests (In-Memory)

```csharp
[Fact]
public async Task OrderCreated_ShouldPublishEvent()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddMassTransitMessaging(new MessagingOptions
    {
        Provider = MessagingProvider.InMemory
    });

    // Act & Assert
    // Test your message handling...
}
```

### Integration Tests (RabbitMQ via Testcontainers)

```csharp
[Fact]
public async Task OrderCreated_ShouldBeConsumed()
{
    // Start RabbitMQ container
    await using var container = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management")
        .Build();

    await container.StartAsync();

    // Configure with RabbitMQ
    var options = new MessagingOptions
    {
        Provider = MessagingProvider.RabbitMQ,
        Host = container.Hostname,
        Port = container.GetMappedPublicPort(5672)
    };

    // Test...
}
```

---

## ✅ Best Practices

1. **Use Environment-Specific Config**
   - `appsettings.Development.json` → InMemory
   - `appsettings.Staging.json` → RabbitMQ
   - `appsettings.Production.json` → AWS SQS or Azure SB

2. **Monitor All Providers**
   - RabbitMQ → Prometheus exporter
   - AWS SQS → CloudWatch metrics
   - Azure SB → Azure Monitor

3. **Handle Provider-Specific Limits**
   - SQS: 256 KB message size limit
   - Azure SB: 256 KB message size (standard tier)
   - RabbitMQ: Configurable (128 MB default)

4. **Dead Letter Queue Strategy**
   - Configure DLQ for all environments
   - Monitor DLQ depth
   - Set up alerts

5. **Security**
   - Never commit credentials
   - Use environment variables
   - Rotate keys regularly
   - Use IAM roles when possible (AWS)

---

## 🎯 Summary

**Zero vendor lock-in achieved!**

✅ Switch providers by changing **configuration only**
✅ Same code works with **all providers**
✅ Easy migration path between clouds
✅ Multi-cloud deployments supported
✅ Test-friendly (In-Memory provider)

**Next Steps**: Choose your provider and update `appsettings.json`!
