# ⚡ Quick Switch Guide - Change Messaging Provider in 30 Seconds

---

## 🎯 Goal: Switch from RabbitMQ to AWS SQS

### ✅ Method 1: Using .env File (EASIEST!)

**Step 1**: Open `.env` file

**Step 2**: Find this line:
```bash
MESSAGING_PROVIDER="RabbitMQ"
```

**Step 3**: Change to:
```bash
MESSAGING_PROVIDER="AmazonSQS"
```

**Step 4**: Add AWS region:
```bash
AWS_REGION="us-east-1"
```

**Step 5**: Restart your service
```bash
docker-compose restart order-service
# or
dotnet run
```

**Done!** ✅ Your service now uses AWS SQS!

---

## 🎯 Goal: Switch from RabbitMQ to Azure Service Bus

### ✅ Method 2: Using appsettings.json

**Step 1**: Open `appsettings.json` or `appsettings.Production.json`

**Step 2**: Find the Messaging section:
```json
{
  "Messaging": {
    "Provider": "RabbitMQ",    // <-- CHANGE THIS LINE
    "Host": "localhost",
    ...
  }
}
```

**Step 3**: Change Provider and add ConnectionString:
```json
{
  "Messaging": {
    "Provider": "AzureServiceBus",    // <-- Changed!
    "ConnectionString": "Endpoint=sb://taskflow.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR_KEY_HERE"
  }
}
```

**Step 4**: Restart your service

**Done!** ✅ Your service now uses Azure Service Bus!

---

## 🎯 All Provider Examples Side-by-Side

### RabbitMQ (Self-Hosted)
```bash
# .env
MESSAGING_PROVIDER="RabbitMQ"
RABBITMQ_HOST="localhost"
RABBITMQ_PORT="5672"
RABBITMQ_USER="guest"
RABBITMQ_PASSWORD="guest"
```

### AWS SQS (Amazon Cloud)
```bash
# .env
MESSAGING_PROVIDER="AmazonSQS"
AWS_REGION="us-east-1"
# Optional: AWS_ACCESS_KEY and AWS_SECRET_KEY (or use IAM role)
```

### Azure Service Bus (Microsoft Cloud)
```bash
# .env
MESSAGING_PROVIDER="AzureServiceBus"
AZURE_SERVICEBUS_CONNECTION_STRING="Endpoint=sb://taskflow.servicebus.windows.net/..."
```

### In-Memory (Testing)
```bash
# .env
MESSAGING_PROVIDER="InMemory"
```

---

## 📋 Complete .env File Example

Copy this to your `.env` file:

```bash
# ======================
# Messaging Configuration
# ======================
# Choose ONE provider: RabbitMQ, AmazonSQS, AzureServiceBus, InMemory

# Option 1: RabbitMQ (Uncomment to use)
MESSAGING_PROVIDER="RabbitMQ"
RABBITMQ_HOST="localhost"
RABBITMQ_PORT="5672"
RABBITMQ_VHOST="/"
RABBITMQ_USER="guest"
RABBITMQ_PASSWORD="guest"

# Option 2: AWS SQS (Uncomment to use)
# MESSAGING_PROVIDER="AmazonSQS"
# AWS_REGION="us-east-1"
# AWS_ACCESS_KEY="your-access-key"  # Optional if using IAM
# AWS_SECRET_KEY="your-secret-key"  # Optional if using IAM

# Option 3: Azure Service Bus (Uncomment to use)
# MESSAGING_PROVIDER="AzureServiceBus"
# AZURE_SERVICEBUS_CONNECTION_STRING="Endpoint=sb://..."

# Option 4: In-Memory for Testing (Uncomment to use)
# MESSAGING_PROVIDER="InMemory"
```

---

## 📋 Complete appsettings.json Example

Copy this to your `appsettings.json`:

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
    "AwsAccessKey": "",
    "AwsSecretKey": "",

    "ConnectionString": "",

    "Retry": {
      "RetryCount": 3,
      "InitialIntervalSeconds": 5,
      "IntervalIncrementSeconds": 5
    },
    "UseMessageScheduler": true
  }
}
```

**To switch providers**: Change ONLY the `"Provider"` value!

---

## 🧪 Testing Locally with Different Providers

### Test with RabbitMQ:
```bash
# Start RabbitMQ in Docker
docker run -d -p 5672:5672 -p 15672:15672 rabbitmq:3-management

# Set .env
MESSAGING_PROVIDER="RabbitMQ"

# Run your service
dotnet run
```

### Test with In-Memory:
```bash
# Set .env
MESSAGING_PROVIDER="InMemory"

# Run your service (no external dependencies!)
dotnet run
```

### Test with AWS SQS (requires AWS account):
```bash
# Set .env
MESSAGING_PROVIDER="AmazonSQS"
AWS_REGION="us-east-1"

# Configure AWS credentials
aws configure

# Run your service
dotnet run
```

---

## 🔄 Migration Checklist

When switching providers in production:

- [ ] **Update .env or appsettings.json** with new provider
- [ ] **Add required NuGet packages** (already included in BuildingBlocks.Messaging)
- [ ] **Configure cloud provider** (create queues/topics)
- [ ] **Set up secrets** (connection strings, access keys)
- [ ] **Test in staging** environment first
- [ ] **Deploy to production**
- [ ] **Monitor message flow**
- [ ] **Decommission old provider** (optional)

---

## ❓ FAQ

**Q: Do I need to change my code?**
A: ❌ NO! Just change configuration.

**Q: Which method should I use - .env or appsettings.json?**
A:
- Use **.env** for Docker deployments
- Use **appsettings.json** for simple deployments
- Use **environment variables** for Kubernetes

**Q: Can I use different providers for different services?**
A: ✅ YES! Each service has its own configuration.

**Q: Can I switch while the service is running?**
A: ❌ NO. You must restart the service for changes to take effect.

**Q: How do I know which provider is being used?**
A: Check the startup logs - MassTransit logs which transport is configured.

---

## 🎯 Summary

**Three simple steps to switch providers:**

1️⃣ **Edit `.env`** or **`appsettings.json`**
2️⃣ **Change the `Provider` value**
3️⃣ **Restart the service**

**No code changes required!** ✅
