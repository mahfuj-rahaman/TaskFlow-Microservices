# CI/CD Pipeline and Secret Management Guide

## Overview

TaskFlow uses **GitHub Actions** for CI/CD with **Docker** for building and deploying microservices. Secrets are managed through **GitHub Secrets** and injected at runtime via environment variables.

This guide covers:
1. CI/CD Pipeline Architecture
2. Secret Management Strategy
3. Docker Build and Deployment
4. Multi-Cloud Deployments
5. Local Development with Secrets

---

## CI/CD Pipeline Architecture

### Pipeline Stages

```
┌─────────────────────────────────────────────────────────────┐
│                    GitHub Push/PR                           │
└──────────────────┬──────────────────────────────────────────┘
                   │
        ┌──────────┴──────────┐
        │                     │
        ▼                     ▼
┌───────────────┐     ┌──────────────────┐
│  Build & Test │     │  Security Scan   │
│  - Restore    │     │  - Trivy         │
│  - Build      │     │  - CodeQL        │
│  - Unit Tests │     │  - SARIF Upload  │
│  - Integ Tests│     └──────────────────┘
└───────┬───────┘
        │
        ▼
┌─────────────────────┐
│  Build Docker Images│
│  - Multi-stage      │
│  - Push to GHCR     │
│  - Tag: latest, SHA │
└─────────┬───────────┘
          │
          ▼
┌──────────────────────────────────────────┐
│         Determine Environment             │
│  - main → production (AWS)               │
│  - staging → staging (Docker Compose)    │
│  - develop → development (local)         │
└──────────┬───────────────────────────────┘
           │
    ┌──────┴───────┐
    │              │
    ▼              ▼
┌─────────┐  ┌─────────┐
│   AWS   │  │  Azure  │  (or GCP, Docker Compose)
│   ECS   │  │   AKS   │
│  /EKS   │  │         │
└─────────┘  └─────────┘
```

### Workflow Files

**Main Pipeline**: `.github/workflows/ci-cd-pipeline.yml`
- Triggers: push to main/develop/staging, PR, manual dispatch
- Jobs: Build, Test, Security Scan, Docker Build, Deploy
- Environments: Development, Staging, Production

---

## Secret Management Strategy

### Secret Storage Hierarchy

```
GitHub Secrets (Encrypted)
    ↓
GitHub Actions Environment Variables
    ↓
Docker Compose / Kubernetes Secrets
    ↓
Application Runtime (ASP.NET Core Configuration)
```

### Types of Secrets

#### 1. **Infrastructure Secrets** (Required for all environments)
- `POSTGRES_USER`
- `POSTGRES_PASSWORD`
- `REDIS_PASSWORD`
- `RABBITMQ_USERNAME`
- `RABBITMQ_PASSWORD`

#### 2. **Cloud Provider Secrets**

**AWS**:
- `AWS_ACCESS_KEY`
- `AWS_SECRET_KEY`
- `AWS_REGION`

**Azure**:
- `AZURE_SERVICE_BUS_CONNECTION_STRING`
- `AZURE_CLIENT_ID`
- `AZURE_CLIENT_SECRET`
- `AZURE_TENANT_ID`

**GCP**:
- `GCP_PROJECT_ID`
- `GCP_SERVICE_ACCOUNT_KEY`

#### 3. **Monitoring Secrets** (Optional)
- `SEQ_API_KEY`
- `DATADOG_API_KEY` (if using Datadog)

### Setting Up GitHub Secrets

#### Repository Secrets (for all environments)

```bash
# Go to: https://github.com/{owner}/{repo}/settings/secrets/actions

# Add the following secrets:
Settings → Secrets and variables → Actions → New repository secret
```

**Required Secrets**:
```
POSTGRES_USER=taskflow_admin
POSTGRES_PASSWORD=<strong-password>
REDIS_PASSWORD=<strong-password>
RABBITMQ_USERNAME=taskflow
RABBITMQ_PASSWORD=<strong-password>
```

#### Environment-Specific Secrets

**Production Environment**:
```bash
# Go to: Settings → Environments → production → Add secret

AWS_ACCESS_KEY=AKIA...
AWS_SECRET_KEY=wJalr...
AWS_REGION=us-east-1
SEQ_API_KEY=<api-key>
```

**Staging Environment**:
```bash
# Go to: Settings → Environments → staging → Add secret

RABBITMQ_USERNAME=staging_user
RABBITMQ_PASSWORD=<staging-password>
```

### Secret Injection Flow

#### GitHub Actions → Docker Compose

```yaml
# .github/workflows/ci-cd-pipeline.yml
deploy:
  steps:
    - name: Deploy with secrets
      env:
        POSTGRES_USER: ${{ secrets.POSTGRES_USER }}
        POSTGRES_PASSWORD: ${{ secrets.POSTGRES_PASSWORD }}
        RABBITMQ_USERNAME: ${{ secrets.RABBITMQ_USERNAME }}
        RABBITMQ_PASSWORD: ${{ secrets.RABBITMQ_PASSWORD }}
        # Cloud-specific
        AWS_ACCESS_KEY: ${{ secrets.AWS_ACCESS_KEY }}
        AWS_SECRET_KEY: ${{ secrets.AWS_SECRET_KEY }}
      run: |
        ./scripts/deploy-with-secrets.sh production aws
```

#### Docker Compose → Container

```yaml
# docker-compose.ci.yml
services:
  api-gateway:
    environment:
      - Messaging__Username=${RABBITMQ_USERNAME}  # ← From GitHub Secret
      - Messaging__Password=${RABBITMQ_PASSWORD}  # ← From GitHub Secret
      - Messaging__AwsAccessKey=${AWS_ACCESS_KEY} # ← From GitHub Secret
```

#### Container → Application

```csharp
// Program.cs
builder.Configuration.AddEnvironmentVariables();

var messagingOptions = builder.Configuration
    .GetSection("Messaging")
    .Get<MessagingOptions>();

// messagingOptions.Username = value from RABBITMQ_USERNAME
// messagingOptions.Password = value from RABBITMQ_PASSWORD
```

---

## Docker Build and Deployment

### Multi-Stage Dockerfile

**Location**: `src/ApiGateway/TaskFlow.Gateway/Dockerfile`

```dockerfile
# Stage 1: Build (SDK image, 1.5GB)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# ... restore, build ...

# Stage 2: Publish
FROM build AS publish
# ... publish ...

# Stage 3: Runtime (ASP.NET runtime, 200MB)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# ... copy published files ...
USER non-root-user  # Security best practice
```

**Benefits**:
- Small final image (~200MB vs 1.5GB)
- Fast deployment
- Security: non-root user
- Layer caching for faster builds

### Building Images

#### Locally

```bash
# Build API Gateway
docker build -t taskflow-api-gateway:latest \
  -f src/ApiGateway/TaskFlow.Gateway/Dockerfile .

# Build with build args
docker build \
  --build-arg BUILD_DATE=$(date -u +"%Y-%m-%dT%H:%M:%SZ") \
  --build-arg VCS_REF=$(git rev-parse --short HEAD) \
  -t taskflow-api-gateway:$(git rev-parse --short HEAD) \
  -f src/ApiGateway/TaskFlow.Gateway/Dockerfile .
```

#### GitHub Actions (Automated)

```yaml
- name: Build and push Docker image
  uses: docker/build-push-action@v5
  with:
    context: .
    file: src/ApiGateway/TaskFlow.Gateway/Dockerfile
    push: true
    tags: |
      ghcr.io/${{ github.repository_owner }}/taskflow-api-gateway:latest
      ghcr.io/${{ github.repository_owner }}/taskflow-api-gateway:${{ github.sha }}
    cache-from: type=registry,ref=ghcr.io/.../buildcache
    cache-to: type=registry,ref=ghcr.io/.../buildcache,mode=max
```

**Features**:
- Automatic push to GitHub Container Registry (GHCR)
- Multi-tag: `latest`, `<git-sha>`, `<branch>-<sha>`
- Build cache for faster builds (5min → 2min)

### Deployment with Docker Compose

#### Using Deployment Script

```bash
# Development
./scripts/deploy-with-secrets.sh development

# Staging
./scripts/deploy-with-secrets.sh staging

# Production with AWS
./scripts/deploy-with-secrets.sh production aws

# Production with Azure
./scripts/deploy-with-secrets.sh production azure
```

**What it does**:
1. ✅ Validates all required secrets are present
2. ✅ Sets environment variables based on environment/cloud
3. ✅ Generates `.env.<environment>` file (not committed)
4. ✅ Pulls latest Docker images
5. ✅ Starts services with docker-compose
6. ✅ Performs health checks

#### Manual Deployment

```bash
# Set secrets as environment variables
export POSTGRES_USER=taskflow_admin
export POSTGRES_PASSWORD=<password>
export RABBITMQ_USERNAME=taskflow
export RABBITMQ_PASSWORD=<password>
# ... other secrets ...

# Deploy
docker-compose -f docker-compose.yml -f docker-compose.ci.yml up -d
```

---

## Multi-Cloud Deployments

### AWS Deployment

**Infrastructure**:
- **ECS (Fargate)**: Serverless containers
- **EKS (Kubernetes)**: Managed Kubernetes
- **RDS**: PostgreSQL database
- **ElastiCache**: Redis
- **Amazon MQ**: RabbitMQ alternative
- **Secrets Manager**: Secret storage

**Deployment**:

```bash
# Prerequisites
aws configure  # Set AWS credentials

# Deploy to ECS
export DEPLOYMENT_TYPE=ecs
export AWS_REGION=us-east-1
./scripts/deploy-aws.sh

# Deploy to EKS
export DEPLOYMENT_TYPE=eks
./scripts/deploy-aws.sh
```

**Secrets in AWS**:
- Stored in **AWS Systems Manager Parameter Store** (encrypted)
- Injected into ECS tasks via task definition
- Injected into EKS pods via Kubernetes secrets

**Example Task Definition** (ECS):
```json
{
  "containerDefinitions": [{
    "name": "api-gateway",
    "image": "ghcr.io/.../taskflow-api-gateway:latest",
    "secrets": [
      {
        "name": "Messaging__Username",
        "valueFrom": "/taskflow/production/rabbitmq/username"
      },
      {
        "name": "Messaging__Password",
        "valueFrom": "/taskflow/production/rabbitmq/password"
      }
    ]
  }]
}
```

### Azure Deployment

**Infrastructure**:
- **Azure Container Apps**: Serverless containers
- **AKS (Kubernetes)**: Managed Kubernetes
- **Azure Database for PostgreSQL**: Managed PostgreSQL
- **Azure Cache for Redis**: Managed Redis
- **Azure Service Bus**: Messaging
- **Azure Key Vault**: Secret storage

**Secrets in Azure**:
- Stored in **Azure Key Vault**
- Injected via Managed Identity (no credentials needed!)

### GCP Deployment

**Infrastructure**:
- **Cloud Run**: Serverless containers
- **GKE (Kubernetes)**: Managed Kubernetes
- **Cloud SQL**: PostgreSQL
- **Memorystore**: Redis
- **Secret Manager**: Secret storage

---

## Local Development with Secrets

### Option 1: User Secrets (Recommended for local dev)

```bash
# Initialize user secrets (not committed to git)
dotnet user-secrets init --project src/ApiGateway/TaskFlow.Gateway

# Set secrets
dotnet user-secrets set "Messaging:Username" "local_user" \
  --project src/ApiGateway/TaskFlow.Gateway

dotnet user-secrets set "Messaging:Password" "local_password" \
  --project src/ApiGateway/TaskFlow.Gateway

# List secrets
dotnet user-secrets list --project src/ApiGateway/TaskFlow.Gateway
```

**Where are they stored?**
- Windows: `%APPDATA%\Microsoft\UserSecrets\<user-secrets-id>\secrets.json`
- Linux/macOS: `~/.microsoft/usersecrets/<user-secrets-id>/secrets.json`

### Option 2: Environment Variables

```bash
# Set for current session
export RABBITMQ_USERNAME=local_user
export RABBITMQ_PASSWORD=local_password

# Or create .env file (add to .gitignore!)
cat > .env.local <<EOF
RABBITMQ_USERNAME=local_user
RABBITMQ_PASSWORD=local_password
EOF

# Load in terminal
source .env.local
```

### Option 3: appsettings.Development.json (NOT for secrets!)

```json
{
  "Messaging": {
    "Host": "localhost",
    "Port": 5672,
    // ❌ DON'T PUT SECRETS HERE!
    // Use user-secrets or environment variables instead
  }
}
```

---

## Security Best Practices

### ✅ DO's

1. **Use GitHub Secrets for all sensitive data**
   ```yaml
   env:
     PASSWORD: ${{ secrets.PASSWORD }}  # ✅ Good
   ```

2. **Use environment-specific secrets**
   - Production secrets in `production` environment
   - Staging secrets in `staging` environment

3. **Rotate secrets regularly**
   - Change passwords every 90 days
   - Update in GitHub Secrets + deployed environments

4. **Use strong passwords**
   - Minimum 20 characters
   - Mix of letters, numbers, symbols
   - Generate with: `openssl rand -base64 32`

5. **Limit secret access**
   - Use GitHub Environment Protection Rules
   - Require reviewers for production deployments

6. **Use Managed Identities (when possible)**
   - Azure: Managed Identity
   - AWS: IAM Roles
   - GCP: Workload Identity

7. **Encrypt secrets at rest**
   - GitHub: Encrypted by default
   - AWS: Parameter Store with KMS
   - Azure: Key Vault
   - GCP: Secret Manager

### ❌ DON'Ts

1. **Never commit secrets to git**
   ```bash
   # Add to .gitignore
   .env
   .env.*
   secrets.json
   appsettings.Secrets.json
   ```

2. **Never log secrets**
   ```csharp
   // ❌ DON'T
   _logger.LogInformation($"Password: {password}");

   // ✅ DO
   _logger.LogInformation("Authentication configured");
   ```

3. **Never use default passwords in production**
   ```yaml
   # ❌ DON'T
   POSTGRES_PASSWORD: postgres

   # ✅ DO
   POSTGRES_PASSWORD: ${{ secrets.POSTGRES_PASSWORD }}
   ```

4. **Never hardcode secrets in code**
   ```csharp
   // ❌ DON'T
   var password = "hardcoded_password";

   // ✅ DO
   var password = _configuration["Messaging:Password"];
   ```

---

## Troubleshooting

### Secrets Not Loaded

**Problem**: Application can't connect to database/RabbitMQ

**Solution**:
1. Check secret is set in GitHub: `Settings → Secrets`
2. Check secret is passed to script:
   ```yaml
   env:
     POSTGRES_PASSWORD: ${{ secrets.POSTGRES_PASSWORD }}
   ```
3. Check secret is passed to container:
   ```yaml
   services:
     api-gateway:
       environment:
         - ConnectionStrings__Db=...Password=${POSTGRES_PASSWORD}
   ```
4. Check in container:
   ```bash
   docker exec -it taskflow-api-gateway env | grep POSTGRES
   ```

### GitHub Actions Deployment Fails

**Problem**: "Missing required secrets" error

**Solution**:
1. Go to repository → `Settings → Environments → <environment>`
2. Add missing secrets
3. Re-run workflow

### Docker Build Fails

**Problem**: "Cannot find project file"

**Solution**:
- Ensure `context: .` (repository root) in `docker build`
- Check Dockerfile COPY paths are correct

### Health Checks Fail

**Problem**: Service marked as unhealthy

**Solution**:
1. Check logs: `docker logs taskflow-api-gateway`
2. Check health endpoint: `curl http://localhost:8080/health`
3. Increase `start_period` in healthcheck (service might need more time to start)

---

## Summary

### CI/CD Pipeline
✅ GitHub Actions for automation
✅ Multi-stage Docker builds
✅ Automated testing
✅ Security scanning
✅ Multi-environment deployment

### Secret Management
✅ GitHub Secrets for storage
✅ Environment variables for injection
✅ No secrets in git
✅ Cloud provider secret managers (AWS SSM, Azure Key Vault, GCP Secret Manager)

### Deployment
✅ Docker Compose for dev/staging
✅ ECS/EKS for AWS
✅ AKS/Container Apps for Azure
✅ GKE/Cloud Run for GCP

### Next Steps
1. ✅ Set up GitHub Secrets
2. ✅ Configure GitHub Environments (development, staging, production)
3. ✅ Push code to trigger pipeline
4. ✅ Monitor deployment
5. ✅ Set up secret rotation schedule

---

**Last Updated**: 2025-11-03
**Maintainer**: TaskFlow Team
**Related Docs**:
- [API_GATEWAY_CONFIGURATION.md](API_GATEWAY_CONFIGURATION.md)
- [CLAUDE.md](../CLAUDE.md)
- [DOCKER.md](../DOCKER.md)
