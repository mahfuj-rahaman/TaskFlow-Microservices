# 🐳 TaskFlow Docker Compose - Complete Guide

**Version**: 1.0
**Last Updated**: 2025-11-04
**Docker Compose Version**: 3.8

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Prerequisites](#prerequisites)
4. [Visual Studio Integration](#visual-studio-integration)
5. [Command Line Usage](#command-line-usage)
6. [Service Configuration](#service-configuration)
7. [Troubleshooting](#troubleshooting)
8. [Production Deployment](#production-deployment)

---

## 🎯 Overview

The TaskFlow Docker Compose setup provides a complete, production-ready microservices stack that can be run with a single command or directly from Visual Studio.

### What's Included

**Infrastructure Services** (7):
- ✅ PostgreSQL 16 - Database
- ✅ Redis 7 - Cache
- ✅ RabbitMQ 3 - Message Broker
- ✅ Consul - Service Discovery
- ✅ Seq - Centralized Logging
- ✅ Jaeger - Distributed Tracing
- ✅ Prometheus (optional) - Metrics

**Microservices** (6):
- ✅ API Gateway - Entry point (Port 5000)
- ✅ Identity Service - Authentication (Port 5006)
- ✅ User Service - User profiles (Port 5001)
- ✅ Task Service - Task management (Port 5005)
- ✅ Admin Service - Administration (Port 5007)
- ✅ Notification Service - Notifications (Port 5004)

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         DOCKER NETWORK                           │
│                      (taskflow-network)                          │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                  INFRASTRUCTURE LAYER                     │  │
│  │                                                            │  │
│  │  [PostgreSQL] [Redis] [RabbitMQ] [Consul] [Seq] [Jaeger] │  │
│  │      :5432     :6379     :5672    :8500  :5341  :16686   │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              ▲                                   │
│                              │                                   │
│  ┌──────────────────────────┴───────────────────────────────┐  │
│  │                  MICROSERVICES LAYER                      │  │
│  │                                                            │  │
│  │  ┌──────────┐                                             │  │
│  │  │ Gateway  │──────┬──────┬──────┬──────┬──────┐         │  │
│  │  │  :5000   │      │      │      │      │      │         │  │
│  │  └──────────┘      ▼      ▼      ▼      ▼      ▼         │  │
│  │               ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐   │  │
│  │               │Ident│ │User │ │Task │ │Admin│ │Notif│   │  │
│  │               │:5006│ │:5001│ │:5005│ │:5007│ │:5004│   │  │
│  │               └─────┘ └─────┘ └─────┘ └─────┘ └─────┘   │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
                          ▲
                          │
                   HOST MACHINE
                   localhost:5000 → API Gateway
```

---

## 📦 Prerequisites

### Required

- **Docker Desktop** 4.25+ with WSL2 backend (Windows)
- **Docker Compose** 2.23+
- **Visual Studio 2022** 17.8+ (for VS integration)
- **12GB RAM** minimum (16GB recommended)
- **50GB disk space** for images and volumes

### Verify Installation

```bash
# Check Docker version
docker --version
# Should be: Docker version 24.0.0 or higher

# Check Docker Compose version
docker-compose --version
# Should be: Docker Compose version 2.23.0 or higher

# Check Docker is running
docker ps
# Should show running containers (or empty if none running)
```

---

## 🎨 Visual Studio Integration

### 🚀 Running from Visual Studio

#### Option 1: Using Docker Compose Project (Recommended)

1. **Open Solution**
   ```
   File → Open → Project/Solution
   Select: TaskFlow.sln
   ```

2. **Set Startup Project**
   ```
   Right-click on "docker-compose" in Solution Explorer
   → Set as Startup Project
   ```

3. **Run with Docker**
   ```
   Press F5 or Click "Docker Compose" button in toolbar
   ```

   Visual Studio will:
   - ✅ Build all Docker images
   - ✅ Start all services with dependencies
   - ✅ Attach debugger to containers
   - ✅ Open browser to Gateway (http://localhost:5000)

#### Option 2: Using Docker Compose Profile

1. **Select Launch Profile**
   ```
   Toolbar → Launch Dropdown
   → Select "Docker Compose"
   ```

2. **Debug Configuration**
   ```
   Project Properties → Debug
   → Configure environment variables
   → Set breakpoints in code
   ```

3. **Start Debugging**
   ```
   F5 - Start with debugging
   Ctrl+F5 - Start without debugging
   ```

### 🔧 Visual Studio Features

**Debugging**:
- ✅ Breakpoint support in all services
- ✅ Live code edit and continue
- ✅ Variable inspection
- ✅ Call stack navigation
- ✅ Hot reload (for supported changes)

**Container Management**:
- View running containers in VS
- See logs in Output window
- Restart individual services
- Exec into containers

**Environment Variables**:
- Edit in `docker-compose.override.yml`
- Use User Secrets for sensitive data
- Override per-service configuration

---

## 💻 Command Line Usage

### Quick Start

```bash
# 1. Clone repository
git clone https://github.com/mahfuj-rahaman/TaskFlow-Microservices.git
cd TaskFlow-Microservices

# 2. Create .env file (copy from example)
cp .env.example .env

# 3. Start entire stack
docker-compose up -d

# 4. Check logs
docker-compose logs -f

# 5. Access services
# Gateway: http://localhost:5000
# Swagger: http://localhost:5000/swagger
# Consul: http://localhost:8500
# RabbitMQ: http://localhost:15672
# Seq: http://localhost:5341
# Jaeger: http://localhost:16686
```

### Common Commands

#### Starting Services

```bash
# Start all services
docker-compose up -d

# Start specific service
docker-compose up -d identity-service

# Start with build
docker-compose up -d --build

# Start and follow logs
docker-compose up
```

#### Stopping Services

```bash
# Stop all services
docker-compose stop

# Stop specific service
docker-compose stop identity-service

# Stop and remove containers
docker-compose down

# Stop and remove volumes (⚠️ deletes data!)
docker-compose down -v
```

#### Viewing Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f identity-service

# Last 100 lines
docker-compose logs --tail=100 api-gateway

# Since timestamp
docker-compose logs --since 2025-11-04T10:00:00
```

#### Managing Services

```bash
# Restart service
docker-compose restart identity-service

# Rebuild service
docker-compose build identity-service

# Scale service (if configured)
docker-compose up -d --scale identity-service=3

# Execute command in container
docker-compose exec identity-service bash

# View service status
docker-compose ps
```

#### Database Operations

```bash
# Access PostgreSQL
docker-compose exec postgres psql -U postgres

# Run migration
docker-compose exec identity-service dotnet ef database update

# Backup database
docker-compose exec postgres pg_dump -U postgres taskflow_identity > backup.sql

# Restore database
docker-compose exec -T postgres psql -U postgres taskflow_identity < backup.sql
```

---

## ⚙️ Service Configuration

### Port Mapping

| Service | Host Port | Container Port | URL |
|---------|-----------|----------------|-----|
| **API Gateway** | 5000 | 8080 | http://localhost:5000 |
| **Identity Service** | 5006 | 8080 | http://localhost:5006 |
| **User Service** | 5001 | 8080 | http://localhost:5001 |
| **Task Service** | 5005 | 8080 | http://localhost:5005 |
| **Admin Service** | 5007 | 8080 | http://localhost:5007 |
| **Notification Service** | 5004 | 8080 | http://localhost:5004 |
| **PostgreSQL** | 5432 | 5432 | localhost:5432 |
| **Redis** | 6379 | 6379 | localhost:6379 |
| **RabbitMQ** | 5672 | 5672 | localhost:5672 |
| **RabbitMQ UI** | 15672 | 15672 | http://localhost:15672 |
| **Consul UI** | 8500 | 8500 | http://localhost:8500 |
| **Seq UI** | 5341 | 80 | http://localhost:5341 |
| **Jaeger UI** | 16686 | 16686 | http://localhost:16686 |

### Environment Variables

Edit `.env` file in root directory:

```bash
# Database
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres

# Cache
REDIS_PASSWORD=redis123

# Message Broker
RABBITMQ_USER=rabbitmq
RABBITMQ_PASSWORD=rabbitmq

# Security
JWT_SECRET_KEY=your-256-bit-secret-key

# Environment
ENVIRONMENT=Development
```

### Health Checks

All services have health checks configured:

```bash
# Check service health
docker-compose ps

# Services with "healthy" status are ready
# Example output:
# NAME                    STATUS          HEALTH
# taskflow-api-gateway    Up 2 minutes    healthy
# taskflow-postgres       Up 2 minutes    healthy
```

### Dependencies

Services start in correct order:

```
1. Infrastructure (Postgres, Redis, RabbitMQ, Consul)
   ↓
2. All infrastructure services healthy
   ↓
3. API Gateway and Microservices
   ↓
4. All services register with Consul
```

---

## 🐛 Troubleshooting

### Issue 1: Services Won't Start

**Symptoms**: `docker-compose up` fails

**Solutions**:
```bash
# Clean everything and restart
docker-compose down -v
docker system prune -a
docker-compose up -d --build

# Check for port conflicts
netstat -ano | findstr :5000

# Check Docker resources
docker system df
```

### Issue 2: Database Connection Errors

**Symptoms**: Services can't connect to PostgreSQL

**Solutions**:
```bash
# Check Postgres is healthy
docker-compose ps postgres

# View Postgres logs
docker-compose logs postgres

# Verify connection string
docker-compose exec identity-service env | grep ConnectionStrings

# Test connection manually
docker-compose exec postgres psql -U postgres -c "\l"
```

### Issue 3: Services Not Registering with Consul

**Symptoms**: Services not appearing in Consul UI

**Solutions**:
```bash
# Check Consul logs
docker-compose logs consul

# Verify Consul is reachable
docker-compose exec identity-service curl http://consul:8500/v1/status/leader

# Restart service
docker-compose restart identity-service
```

### Issue 4: Out of Memory

**Symptoms**: Services crashing or slow performance

**Solutions**:
```bash
# Increase Docker Desktop memory
# Settings → Resources → Memory → 12GB+

# Check memory usage
docker stats

# Stop unused services
docker-compose stop user-service task-service
```

### Issue 5: Volumes Permission Errors

**Symptoms**: Volume mount errors on Linux/WSL

**Solutions**:
```bash
# Fix permissions
sudo chown -R $USER:$USER .

# Or run with sudo
sudo docker-compose up -d
```

### Issue 6: Build Failures

**Symptoms**: Docker build fails

**Solutions**:
```bash
# Clear build cache
docker builder prune -a

# Rebuild from scratch
docker-compose build --no-cache identity-service

# Check Dockerfile
docker-compose config
```

---

## 🚀 Production Deployment

### Using docker-compose.prod.yml

```bash
# Start with production config
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Or set environment
export COMPOSE_FILE=docker-compose.yml:docker-compose.prod.yml
docker-compose up -d
```

### Production Checklist

- [ ] Change all default passwords
- [ ] Use strong JWT secret key (256-bit)
- [ ] Enable HTTPS/TLS
- [ ] Configure proper CORS origins
- [ ] Set up backup strategy
- [ ] Configure monitoring/alerting
- [ ] Use secrets management (Docker Secrets/Vault)
- [ ] Enable rate limiting
- [ ] Configure log rotation
- [ ] Set resource limits (CPU/Memory)
- [ ] Use private Docker registry
- [ ] Enable security scanning

### Resource Limits (Production)

Add to `docker-compose.prod.yml`:

```yaml
services:
  identity-service:
    deploy:
      resources:
        limits:
          cpus: '1.0'
          memory: 1G
        reservations:
          cpus: '0.5'
          memory: 512M
```

---

## 📊 Monitoring & Logs

### Accessing UIs

**Consul** - Service Discovery
```
URL: http://localhost:8500
Features: Service registry, health checks, KV store
```

**RabbitMQ Management**
```
URL: http://localhost:15672
Username: rabbitmq
Password: rabbitmq
Features: Queues, exchanges, message tracking
```

**Seq** - Structured Logs
```
URL: http://localhost:5341
Features: Log search, filtering, dashboards
```

**Jaeger** - Distributed Tracing
```
URL: http://localhost:16686
Features: Trace visualization, performance analysis
```

### Log Aggregation

All services log to Seq:

```bash
# View logs in Seq UI
http://localhost:5341

# Or via Docker
docker-compose logs -f identity-service
```

### Metrics

Coming soon: Prometheus + Grafana integration

---

## 🔐 Security Best Practices

### Development

- ✅ Use `.env` for configuration (not committed)
- ✅ User Secrets for sensitive data in VS
- ✅ Default passwords are okay (isolated environment)

### Production

- ✅ Use Docker Secrets or external secret manager
- ✅ Rotate all credentials
- ✅ Use TLS for all communication
- ✅ Enable network policies
- ✅ Regular security updates
- ✅ Implement backup/disaster recovery
- ✅ Monitor for vulnerabilities

---

## 📚 Additional Commands

### Docker Cleanup

```bash
# Remove stopped containers
docker-compose rm -f

# Remove unused images
docker image prune -a

# Remove unused volumes
docker volume prune

# Complete cleanup (⚠️ removes everything!)
docker system prune -a --volumes
```

### Performance Monitoring

```bash
# Resource usage
docker stats

# Specific service stats
docker stats taskflow-identity-service

# Container inspect
docker inspect taskflow-identity-service
```

### Network Troubleshooting

```bash
# List networks
docker network ls

# Inspect network
docker network inspect taskflow-network

# Test connectivity
docker-compose exec identity-service ping postgres
docker-compose exec identity-service curl http://api-gateway:8080/api/health
```

---

## 🎉 Summary

**Visual Studio Usage**:
1. Open `TaskFlow.sln`
2. Set `docker-compose` as Startup Project
3. Press F5
4. Debug with breakpoints

**Command Line Usage**:
1. `docker-compose up -d`
2. Access http://localhost:5000
3. View logs with `docker-compose logs -f`

**Key URLs**:
- Gateway: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- Consul: http://localhost:8500
- RabbitMQ: http://localhost:15672
- Seq: http://localhost:5341
- Jaeger: http://localhost:16686

**Stop Everything**:
```bash
docker-compose down
```

**Clean Everything**:
```bash
docker-compose down -v
```

---

**Version**: 1.0
**Last Updated**: 2025-11-04
**Status**: ✅ Production Ready
