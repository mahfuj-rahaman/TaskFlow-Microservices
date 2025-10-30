# Docker Quick Start Guide

Quick commands to get TaskFlow running with Docker.

## Prerequisites

- Docker Desktop installed and running
- At least 4GB RAM available

## Local Development (Fastest Start)

```bash
# 1. Copy environment file
cp .env.example .env

# 2. Start everything
docker-compose -f docker-compose.yml -f docker-compose.local.yml up --build

# 3. Access services
# - API: http://localhost:5001
# - Swagger: http://localhost:5001/swagger
# - Health: http://localhost:5001/health
# - RabbitMQ UI: http://localhost:15672 (rabbitmq/rabbitmq)

# 4. Stop (Ctrl+C, then)
docker-compose -f docker-compose.yml -f docker-compose.local.yml down
```

## Common Commands

### Start in background
```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml up -d
```

### View logs
```bash
# All services
docker-compose -f docker-compose.yml -f docker-compose.local.yml logs -f

# Task service only
docker-compose -f docker-compose.yml -f docker-compose.local.yml logs -f task-service
```

### Check status
```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml ps
```

### Stop services
```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml down
```

### Stop and remove data
```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml down -v
```

### Rebuild after code changes
```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml up --build
```

## Test API

```bash
# Create a task
curl -X POST http://localhost:5001/api/v1/tasks \
  -H "Content-Type: application/json" \
  -d '{
    "title": "My First Task",
    "description": "Testing the API",
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "priority": "High",
    "dueDate": "2025-12-31T23:59:59Z"
  }'

# Get all tasks
curl http://localhost:5001/api/v1/tasks

# Health check
curl http://localhost:5001/health
```

## Environment Profiles

### Local (default)
```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml up
```
- Ports exposed for all services
- Debug logging enabled
- Hot reload ready

### Development
```bash
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up
```
- Environment variables from .env
- Development logging
- Automatic restarts

### Production
```bash
# Set passwords first!
export POSTGRES_PASSWORD=your_secure_password
export REDIS_PASSWORD=your_secure_password
export RABBITMQ_PASSWORD=your_secure_password

docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```
- No ports exposed (except API)
- Warning-level logging
- Resource limits
- Service replication

## Troubleshooting

### Ports already in use
```bash
# Windows
netstat -ano | findstr :5001

# Change ports in docker-compose.local.yml:
ports:
  - "5003:8080"  # Instead of 5001
```

### Service won't start
```bash
# Check logs
docker-compose -f docker-compose.yml -f docker-compose.local.yml logs task-service

# Restart specific service
docker-compose -f docker-compose.yml -f docker-compose.local.yml restart task-service
```

### Database issues
```bash
# Check postgres health
docker inspect taskflow-postgres | findstr Health

# Connect to database
docker exec -it taskflow-postgres psql -U postgres -d taskflow_task

# Recreate database
docker-compose -f docker-compose.yml -f docker-compose.local.yml down -v
docker-compose -f docker-compose.yml -f docker-compose.local.yml up
```

### Clear everything and restart
```bash
# Nuclear option - removes all containers, networks, and volumes
docker-compose -f docker-compose.yml -f docker-compose.local.yml down -v
docker system prune -a
docker-compose -f docker-compose.yml -f docker-compose.local.yml up --build
```

## Service URLs

| Service | URL | Credentials |
|---------|-----|-------------|
| Task API | http://localhost:5001 | - |
| Swagger UI | http://localhost:5001/swagger | - |
| Health Check | http://localhost:5001/health | - |
| RabbitMQ UI | http://localhost:15672 | rabbitmq/rabbitmq |
| PostgreSQL | localhost:5432 | postgres/postgres |
| Redis | localhost:6379 | password: redis123 |

## Files Created

- `docker-compose.yml` - Base configuration
- `docker-compose.local.yml` - Local development overrides
- `docker-compose.dev.yml` - Development environment overrides
- `docker-compose.prod.yml` - Production environment overrides
- `src/Services/Task/Dockerfile` - Task service image definition
- `.dockerignore` - Build exclusions
- `.env.example` - Environment variables template
- `scripts/init-databases.sql` - Database initialization
- `DOCKER.md` - Detailed documentation
- `DOCKER-TEST.md` - Comprehensive testing guide

## Next Steps

1. Start Docker Desktop
2. Run the local development commands above
3. Open http://localhost:5001/swagger
4. Create and test some tasks
5. Check out `DOCKER.md` for advanced features
6. See `DOCKER-TEST.md` for comprehensive testing

## Need Help?

- Detailed docs: See `DOCKER.md`
- Testing guide: See `DOCKER-TEST.md`
- Main README: See `README.md`
