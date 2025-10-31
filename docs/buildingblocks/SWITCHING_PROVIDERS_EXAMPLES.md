# 🔄 How to Switch Messaging Providers - Complete Examples

**Last Updated**: 2025-11-01

---

## 📋 Overview

You can switch messaging providers using **THREE methods**:
1. **.env file** (Recommended for Docker)
2. **appsettings.json** (Direct configuration)
3. **Environment variables** (Kubernetes, Cloud)

---

## Method 1: Using .env File (Recommended for Docker) ⭐

### Step 1: Update `.env` file

**.env** (Development with RabbitMQ):
```bash
# ======================
# Messaging Configuration
# ======================
MESSAGING_PROVIDER="RabbitMQ"
RABBITMQ_HOST="localhost"
RABBITMQ_PORT="5672"
RABBITMQ_VHOST="/"
RABBITMQ_USER="guest"
RABBITMQ_PASSWORD="guest"
```

**.env** (Production with AWS SQS):
```bash
# ======================
# Messaging Configuration
# ======================
MESSAGING_PROVIDER="AmazonSQS"
AWS_REGION="us-east-1"
# AWS_ACCESS_KEY="your-access-key"  # Optional if using IAM roles
# AWS_SECRET_KEY="your-secret-key"  # Optional if using IAM roles
```

**.env** (Production with Azure Service Bus):
```bash
# ======================
# Messaging Configuration
# ======================
MESSAGING_PROVIDER="AzureServiceBus"
AZURE_SERVICEBUS_CONNECTION_STRING="Endpoint=sb://taskflow.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key-here"
```

**.env** (Testing with In-Memory):
```bash
# ======================
# Messaging Configuration
# ======================
MESSAGING_PROVIDER="InMemory"
```

---

### Step 2: Update `appsettings.json` to read from environment

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
    "AwsRegion": "us-east-1",
    "ConnectionString": "",
    "Retry": {
      "RetryCount": 3,
      "InitialIntervalSeconds": 5,
      "IntervalIncrementSeconds": 5
    }
  }
}
```

**appsettings.Development.json** (overrides with env vars):
```json
{
  "Messaging": {
    "Provider": "${MESSAGING_PROVIDER}",
    "Host": "${RABBITMQ_HOST}",
    "Port": "${RABBITMQ_PORT}",
    "VirtualHost": "${RABBITMQ_VHOST}",
    "Username": "${RABBITMQ_USER}",
    "Password": "${RABBITMQ_PASSWORD}",
    "AwsRegion": "${AWS_REGION}",
    "AwsAccessKey": "${AWS_ACCESS_KEY}",
    "AwsSecretKey": "${AWS_SECRET_KEY}",
    "ConnectionString": "${AZURE_SERVICEBUS_CONNECTION_STRING}"
  }
}
```

---

### Step 3: Configure Program.cs to load .env

**Program.cs**:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Load .env file
DotNetEnv.Env.Load();

// Or use this approach
builder.Configuration.AddEnvironmentVariables();

// Add messaging (reads from appsettings which reads from .env)
builder.Services.AddMassTransitMessaging(
    builder.Configuration,
    cfg =>
    {
        cfg.AddConsumer<OrderCreatedEventConsumer>();
        cfg.AddConsumer<UserRegisteredEventConsumer>();
    });

var app = builder.Build();
app.Run();
```

---

### Step 4: Switch providers by editing `.env`

**To switch from RabbitMQ to AWS SQS:**

Just change this line in `.env`:
```bash
MESSAGING_PROVIDER="AmazonSQS"  # Changed from RabbitMQ
```

**That's it!** Restart the service and it now uses AWS SQS! 🎉

---

## Method 2: Using appsettings.json Directly (Simple)

### Scenario 1: Development with RabbitMQ

**appsettings.Development.json**:
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

**Program.cs**:
```csharp
builder.Services.AddMassTransitMessaging(
    builder.Configuration,
    cfg => cfg.AddConsumer<OrderCreatedEventConsumer>());
```

**To switch to AWS SQS**, change only `appsettings.Production.json`:

```json
{
  "Messaging": {
    "Provider": "AmazonSQS",
    "AwsRegion": "us-east-1"
  }
}
```

---

### Scenario 2: Staging with Azure Service Bus

**appsettings.Staging.json**:
```json
{
  "Messaging": {
    "Provider": "AzureServiceBus",
    "ConnectionString": "Endpoint=sb://taskflow-staging.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-staging-key"
  }
}
```

---

### Scenario 3: Testing with In-Memory

**appsettings.Testing.json**:
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

---

## Method 3: Using Environment Variables (Kubernetes/Cloud)

### Kubernetes ConfigMap

**configmap.yaml**:
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: messaging-config
  namespace: taskflow
data:
  MESSAGING__PROVIDER: "AmazonSQS"
  MESSAGING__AWSREGION: "us-east-1"
  MESSAGING__RETRY__RETRYCOUNT: "5"
  MESSAGING__RETRY__INITIALINTERVALSECONDS: "10"
```

### Kubernetes Secret

**secret.yaml**:
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: messaging-secrets
  namespace: taskflow
type: Opaque
stringData:
  MESSAGING__AWSACCESSKEY: "your-access-key"
  MESSAGING__AWSSECRETKEY: "your-secret-key"
```

### Deployment

**deployment.yaml**:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: order-service
spec:
  template:
    spec:
      containers:
      - name: order-service
        image: taskflow/order-service:latest
        envFrom:
        - configMapRef:
            name: messaging-config
        - secretRef:
            name: messaging-secrets
```

**Result**: Service automatically uses AWS SQS! 🎉

---

## 🐳 Docker Compose Examples

### Example 1: RabbitMQ (Default)

**docker-compose.yml**:
```yaml
version: '3.8'

services:
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest

  order-service:
    image: taskflow/order-service
    environment:
      MESSAGING__PROVIDER: "RabbitMQ"
      MESSAGING__HOST: "rabbitmq"
      MESSAGING__PORT: 5672
      MESSAGING__USERNAME: "guest"
      MESSAGING__PASSWORD: "guest"
    depends_on:
      - rabbitmq
```

---

### Example 2: AWS SQS (Production)

**docker-compose.production.yml**:
```yaml
version: '3.8'

services:
  order-service:
    image: taskflow/order-service
    environment:
      MESSAGING__PROVIDER: "AmazonSQS"
      MESSAGING__AWSREGION: "us-east-1"
      # Uses IAM role from EC2/ECS - no keys needed!
```

---

### Example 3: Azure Service Bus

**docker-compose.azure.yml**:
```yaml
version: '3.8'

services:
  order-service:
    image: taskflow/order-service
    environment:
      MESSAGING__PROVIDER: "AzureServiceBus"
      MESSAGING__CONNECTIONSTRING: "${AZURE_SB_CONNECTION}"
    env_file:
      - .env.azure
```

**.env.azure**:
```bash
AZURE_SB_CONNECTION="Endpoint=sb://taskflow.servicebus.windows.net/..."
```

---

### Example 4: Multi-Service with Different Providers

**docker-compose.multi.yml**:
```yaml
version: '3.8'

services:
  # User service uses RabbitMQ
  user-service:
    image: taskflow/user-service
    environment:
      MESSAGING__PROVIDER: "RabbitMQ"
      MESSAGING__HOST: "rabbitmq"

  # Order service uses AWS SQS
  order-service:
    image: taskflow/order-service
    environment:
      MESSAGING__PROVIDER: "AmazonSQS"
      MESSAGING__AWSREGION: "us-east-1"

  # Notification service uses Azure Service Bus
  notification-service:
    image: taskflow/notification-service
    environment:
      MESSAGING__PROVIDER: "AzureServiceBus"
      MESSAGING__CONNECTIONSTRING: "${AZURE_SB_CONNECTION}"
```

**Result**: Each service uses different messaging! 🎉

---

## 🔄 Complete Migration Example

### Scenario: Migrate from RabbitMQ to AWS SQS

#### Before (RabbitMQ):

**.env**:
```bash
MESSAGING_PROVIDER="RabbitMQ"
RABBITMQ_HOST="rabbitmq.production.com"
RABBITMQ_PORT="5672"
RABBITMQ_USER="taskflow"
RABBITMQ_PASSWORD="secret123"
```

#### After (AWS SQS):

**.env**:
```bash
MESSAGING_PROVIDER="AmazonSQS"
AWS_REGION="us-east-1"
# Using IAM role - no keys needed!
```

#### Migration Steps:

1. **Deploy new version** with updated `.env`
2. **Run both** RabbitMQ and SQS temporarily
3. **Drain RabbitMQ queues**
4. **Switch traffic** to SQS
5. **Decommission** RabbitMQ

**Code changes needed**: **ZERO!** ✅

---

## 📊 Quick Reference Table

| Provider | Key .env Variable | Required Additional Variables |
|----------|------------------|-------------------------------|
| **RabbitMQ** | `MESSAGING_PROVIDER="RabbitMQ"` | `RABBITMQ_HOST`, `RABBITMQ_USER`, `RABBITMQ_PASSWORD` |
| **AWS SQS** | `MESSAGING_PROVIDER="AmazonSQS"` | `AWS_REGION` (optional: `AWS_ACCESS_KEY`, `AWS_SECRET_KEY`) |
| **Azure Service Bus** | `MESSAGING_PROVIDER="AzureServiceBus"` | `AZURE_SERVICEBUS_CONNECTION_STRING` |
| **In-Memory** | `MESSAGING_PROVIDER="InMemory"` | None |

---

## ✅ Best Practices

### 1. Use .env for Local Development
```bash
# .env
MESSAGING_PROVIDER="InMemory"  # Fast, no dependencies
```

### 2. Use appsettings.{Environment}.json for Deployed Environments
```json
// appsettings.Production.json
{
  "Messaging": {
    "Provider": "AmazonSQS",
    "AwsRegion": "us-east-1"
  }
}
```

### 3. Use Environment Variables for Secrets
```bash
# Never commit this!
export AWS_ACCESS_KEY="AKIA..."
export AWS_SECRET_KEY="secret..."
```

### 4. Use Cloud-Native Secrets Management
- **AWS**: AWS Secrets Manager
- **Azure**: Azure Key Vault
- **Kubernetes**: Sealed Secrets

---

## 🧪 Testing Different Providers

### Unit Tests (In-Memory):
```csharp
var options = new MessagingOptions { Provider = MessagingProvider.InMemory };
services.AddMassTransitMessaging(options);
```

### Integration Tests (RabbitMQ):
```bash
# .env.test
MESSAGING_PROVIDER="RabbitMQ"
RABBITMQ_HOST="localhost"  # Testcontainer
```

### Staging (Azure Service Bus):
```bash
# .env.staging
MESSAGING_PROVIDER="AzureServiceBus"
AZURE_SERVICEBUS_CONNECTION_STRING="Endpoint=sb://..."
```

### Production (AWS SQS):
```bash
# .env.production
MESSAGING_PROVIDER="AmazonSQS"
AWS_REGION="us-east-1"
```

---

## 🎯 Summary

✅ **Three ways to switch:**
1. Edit `.env` file (easiest)
2. Edit `appsettings.{Environment}.json` (clearest)
3. Set environment variables (most secure)

✅ **Zero code changes required!**
✅ **Same code works with all 4 providers**
✅ **Switch by changing one line**

**Next**: Update your `.env` file and restart your service! 🚀
