# Docker Setup Guide

This guide explains how to run the TaskFlow microservices using Docker and Docker Compose.

## Prerequisites

- Docker 24.0 or later
- Docker Compose 2.20 or later
- At least 4GB of available RAM

## Quick Start

### Local Development

1. Copy the environment template:
```bash
cp .env.example .env
```

2. Edit `.env` file with your local settings (or use defaults)

3. Start all services:
```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml up --build
```

4. Access the services:
   - Task API: http://localhost:5001
   - Swagger UI: http://localhost:5001/swagger
   - Health Check: http://localhost:5001/health
   - RabbitMQ Management: http://localhost:15672 (rabbitmq/rabbitmq)

5. Stop services:
```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml down
```

### Development Environment

```bash
# Start services
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up --build -d

# View logs
docker-compose -f docker-compose.yml -f docker-compose.dev.yml logs -f task-service

# Stop services
docker-compose -f docker-compose.yml -f docker-compose.dev.yml down
```

### Production Environment

```bash
# Set required environment variables
export POSTGRES_PASSWORD=your_secure_password
export REDIS_PASSWORD=your_secure_password
export RABBITMQ_PASSWORD=your_secure_password

# Start services
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Stop services
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down
```

## Architecture

### Services

1. **PostgreSQL** (postgres:16-alpine)
   - Port: 5432 (local/dev only)
   - Persistent volume: taskflow-postgres-data
   - Health check: pg_isready

2. **Redis** (redis:7-alpine)
   - Port: 6379 (local/dev only)
   - Persistent volume: taskflow-redis-data
   - Health check: redis-cli ping

3. **RabbitMQ** (rabbitmq:3-management-alpine)
   - Port: 5672 (AMQP)
   - Management Port: 15672 (local/dev only)
   - Persistent volume: taskflow-rabbitmq-data
   - Health check: rabbitmq-diagnostics ping

4. **Task Service** (.NET 8.0)
   - Port: 5001 (HTTP) / 5002 (HTTPS) in local
   - Port: 80 (HTTP) / 443 (HTTPS) in production
   - Health check: /health endpoint

### Networks

- **taskflow-network**: Bridge network for inter-service communication

### Volumes

- **taskflow-postgres-data**: PostgreSQL data persistence
- **taskflow-redis-data**: Redis data persistence
- **taskflow-rabbitmq-data**: RabbitMQ data persistence
- **taskflow-task-logs**: Task service logs

## Configuration

### Environment Variables

Key environment variables (see `.env.example` for full list):

- `POSTGRES_USER`: PostgreSQL username
- `POSTGRES_PASSWORD`: PostgreSQL password
- `POSTGRES_DB`: Database name
- `REDIS_PASSWORD`: Redis password
- `RABBITMQ_USER`: RabbitMQ username
- `RABBITMQ_PASSWORD`: RabbitMQ password
- `ASPNETCORE_ENVIRONMENT`: Application environment (Development/Production)

### File Structure

```
.
├── docker-compose.yml              # Base configuration
├── docker-compose.local.yml        # Local development overrides
├── docker-compose.dev.yml          # Development environment overrides
├── docker-compose.prod.yml         # Production environment overrides
├── .dockerignore                   # Docker build exclusions
├── .env.example                    # Environment variables template
├── scripts/
│   └── init-databases.sql         # Database initialization
└── src/
    └── Services/
        └── Task/
            └── Dockerfile         # Task service container
```

## Database Initialization

The PostgreSQL container automatically runs `scripts/init-databases.sql` on first startup, which:

1. Creates all required databases:
   - `taskflow_task`
   - `taskflow_user` (placeholder)
   - `taskflow_notification` (placeholder)
   - `taskflow_audit` (placeholder)

2. Enables PostgreSQL extensions:
   - `uuid-ossp`: UUID generation
   - `pg_trgm`: Text search optimization

3. Creates schemas and sets permissions

## Health Checks

All services have health checks configured:

```bash
# Check all services
docker-compose ps

# Check specific service health
docker inspect --format='{{json .State.Health}}' taskflow-task-service
```

## Troubleshooting

### Service won't start

1. Check logs:
```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml logs task-service
```

2. Check service health:
```bash
docker-compose ps
```

3. Verify dependencies are healthy:
```bash
docker inspect taskflow-postgres | grep Health
docker inspect taskflow-redis | grep Health
docker inspect taskflow-rabbitmq | grep Health
```

### Database connection issues

1. Ensure PostgreSQL is healthy:
```bash
docker exec taskflow-postgres pg_isready -U postgres
```

2. Check connection string in environment variables

3. Run migrations manually:
```bash
docker exec taskflow-task-service dotnet ef database update
```

### Port conflicts

If ports are already in use, modify the port mappings in the respective `docker-compose.*.yml` file:

```yaml
ports:
  - "5001:8080"  # Change 5001 to another port
```

### Clean restart

Remove all containers, volumes, and start fresh:

```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml down -v
docker-compose -f docker-compose.yml -f docker-compose.local.yml up --build
```

## Running Migrations

### Apply migrations

```bash
# Local environment
docker-compose -f docker-compose.yml -f docker-compose.local.yml exec task-service \
  dotnet ef database update

# Or build and run with migrations
docker-compose -f docker-compose.yml -f docker-compose.local.yml up --build
```

### Create new migration

```bash
# From host machine (requires .NET SDK)
cd src/Services/Task/TaskFlow.Task.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../TaskFlow.Task.API

# Or from container
docker-compose exec task-service \
  dotnet ef migrations add MigrationName --project /src/Services/Task/TaskFlow.Task.Infrastructure
```

## Performance Tuning

### Production Optimization

The `docker-compose.prod.yml` includes:

1. **Resource Limits**:
   - PostgreSQL: 2 CPU / 2GB RAM
   - Redis: 1 CPU / 512MB RAM
   - RabbitMQ: 1 CPU / 1GB RAM
   - Task Service: 2 CPU / 2GB RAM

2. **Service Scaling**:
```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --scale task-service=3
```

3. **Security**:
   - No exposed ports for databases
   - Non-root container user
   - Sensitive data logging disabled
   - Required environment variable validation

## Monitoring

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f task-service

# Last 100 lines
docker-compose logs --tail=100 task-service
```

### Resource Usage

```bash
# Real-time stats
docker stats

# Specific container
docker stats taskflow-task-service
```

## Backup and Restore

### Backup PostgreSQL

```bash
docker exec taskflow-postgres pg_dump -U postgres taskflow_task > backup.sql
```

### Restore PostgreSQL

```bash
docker exec -i taskflow-postgres psql -U postgres taskflow_task < backup.sql
```

### Backup Redis

```bash
docker exec taskflow-redis redis-cli --rdb /data/dump.rdb
docker cp taskflow-redis:/data/dump.rdb ./redis-backup.rdb
```

## Security Considerations

### Production Checklist

- [ ] Use strong passwords in `.env` file
- [ ] Never commit `.env` file to version control
- [ ] Use Docker secrets for sensitive data
- [ ] Enable TLS/SSL for all services
- [ ] Configure firewall rules
- [ ] Use private Docker registry
- [ ] Scan images for vulnerabilities
- [ ] Enable audit logging
- [ ] Implement rate limiting
- [ ] Use read-only file systems where possible

### Docker Secrets (Production)

Instead of environment variables, use Docker secrets:

```yaml
services:
  task-service:
    secrets:
      - postgres_password
      - redis_password

secrets:
  postgres_password:
    external: true
  redis_password:
    external: true
```

## CI/CD Integration

### Build and Push Images

```bash
# Build
docker build -t taskflow/task-service:latest -f src/Services/Task/Dockerfile .

# Tag for registry
docker tag taskflow/task-service:latest registry.example.com/taskflow/task-service:latest

# Push
docker push registry.example.com/taskflow/task-service:latest
```

### Automated Deployment

Example GitHub Actions workflow:

```yaml
name: Deploy
on:
  push:
    branches: [main]
jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Build and deploy
        run: |
          docker-compose -f docker-compose.yml -f docker-compose.prod.yml build
          docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [PostgreSQL Docker Image](https://hub.docker.com/_/postgres)
- [Redis Docker Image](https://hub.docker.com/_/redis)
- [RabbitMQ Docker Image](https://hub.docker.com/_/rabbitmq)
