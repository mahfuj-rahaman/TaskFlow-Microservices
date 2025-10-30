# Docker Testing Checklist

This guide provides step-by-step instructions to test the Docker setup for TaskFlow microservices.

## Prerequisites

Before testing, ensure:

- [ ] Docker Desktop is installed and running
- [ ] Docker version 24.0+ (`docker --version`)
- [ ] Docker Compose version 2.20+ (`docker-compose --version`)
- [ ] At least 4GB RAM available for Docker
- [ ] Ports 5432, 6379, 5672, 15672, 5001, 5002 are available

## Test 1: Validate Docker Configuration

### Check Docker is running

```bash
docker info
```

Expected: Docker info displayed without errors

### Validate Compose files

```bash
# Validate base configuration
docker-compose -f docker-compose.yml config

# Validate local configuration
docker-compose -f docker-compose.yml -f docker-compose.local.yml config

# Validate dev configuration
docker-compose -f docker-compose.yml -f docker-compose.dev.yml config

# Validate prod configuration
docker-compose -f docker-compose.yml -f docker-compose.prod.yml config
```

Expected: No syntax errors, merged configuration displayed

## Test 2: Build Task Service Image

### Build the Dockerfile

```bash
cd D:\projects\TaskFlow-Microservices\main
docker build -t taskflow-task-service:test -f src/Services/Task/Dockerfile .
```

Expected output:
```
[+] Building X.Xs (XX/XX) FINISHED
=> [internal] load build definition from Dockerfile
=> [internal] load .dockerignore
=> [build 1/X] FROM mcr.microsoft.com/dotnet/sdk:8.0
=> [final X/X] COPY --from=publish /app/publish .
=> exporting to image
=> => naming to taskflow-task-service:test
```

### Verify image created

```bash
docker images | grep taskflow-task-service
```

Expected: Image listed with tag `test`

### Inspect image

```bash
docker inspect taskflow-task-service:test
```

Expected: Image details including:
- OS: linux
- Architecture: amd64
- Exposed ports: 8080, 8081
- User: appuser
- Entrypoint: ["dotnet", "TaskFlow.Task.API.dll"]

## Test 3: Local Environment Testing

### Step 1: Create .env file

```bash
cp .env.example .env
```

Edit `.env` with local values or use defaults.

### Step 2: Start services

```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml up -d
```

Expected:
```
[+] Running 5/5
✔ Network taskflow-network           Created
✔ Container taskflow-postgres        Started
✔ Container taskflow-redis           Started
✔ Container taskflow-rabbitmq        Started
✔ Container taskflow-task-service    Started
```

### Step 3: Check container status

```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml ps
```

Expected: All services with status "Up" and "healthy"

### Step 4: Wait for health checks

```bash
# Wait 30 seconds for health checks to pass
timeout /t 30

# Check health status
docker inspect --format='{{.State.Health.Status}}' taskflow-postgres
docker inspect --format='{{.State.Health.Status}}' taskflow-redis
docker inspect --format='{{.State.Health.Status}}' taskflow-rabbitmq
docker inspect --format='{{.State.Health.Status}}' taskflow-task-service
```

Expected: All return "healthy"

### Step 5: Test PostgreSQL connection

```bash
docker exec taskflow-postgres psql -U postgres -d taskflow_task -c "SELECT version();"
```

Expected: PostgreSQL version displayed

### Step 6: Test Redis connection

```bash
docker exec taskflow-redis redis-cli -a redis123 ping
```

Expected: `PONG`

### Step 7: Test RabbitMQ connection

```bash
docker exec taskflow-rabbitmq rabbitmq-diagnostics ping
```

Expected: Ping succeeded

### Step 8: Test Task Service API

```bash
# Test root endpoint
curl http://localhost:5001/

# Test health endpoint
curl http://localhost:5001/health

# Test Swagger UI
curl http://localhost:5001/swagger/index.html
```

Expected:
- Root: JSON with service info
- Health: "Healthy" status
- Swagger: HTML page

### Step 9: Test database migration

```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml logs task-service | grep -i migration
```

Expected: Migration logs showing successful database update

### Step 10: View logs

```bash
# All services
docker-compose -f docker-compose.yml -f docker-compose.local.yml logs

# Specific service
docker-compose -f docker-compose.yml -f docker-compose.local.yml logs task-service
```

Expected: No critical errors

### Step 11: Test API endpoints

```bash
# Create a task
curl -X POST http://localhost:5001/api/v1/tasks \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Task",
    "description": "Testing Docker deployment",
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "priority": "High",
    "dueDate": "2025-12-31T23:59:59Z"
  }'

# Get all tasks
curl http://localhost:5001/api/v1/tasks

# Get specific task (use ID from create response)
curl http://localhost:5001/api/v1/tasks/{task-id}
```

Expected: Successful responses with task data

### Step 12: Test RabbitMQ Management UI

Open browser: http://localhost:15672
- Username: `rabbitmq`
- Password: `rabbitmq`

Expected: RabbitMQ management dashboard loads

### Step 13: Stop services

```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml down
```

Expected: All containers stopped and removed

### Step 14: Clean up volumes (optional)

```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml down -v
```

Expected: All volumes removed

## Test 4: Development Environment Testing

### Start dev environment

```bash
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d
```

### Run same tests as local environment

Repeat steps 3-12 from Test 3

### Additional dev-specific checks

```bash
# Check environment variables
docker exec taskflow-task-service printenv | grep ASPNETCORE_ENVIRONMENT
```

Expected: `ASPNETCORE_ENVIRONMENT=Development`

### Stop dev environment

```bash
docker-compose -f docker-compose.yml -f docker-compose.dev.yml down
```

## Test 5: Production Environment Testing

### Set required environment variables

```bash
# Windows PowerShell
$env:POSTGRES_PASSWORD="secure_prod_password"
$env:REDIS_PASSWORD="secure_prod_password"
$env:RABBITMQ_PASSWORD="secure_prod_password"

# Windows CMD
set POSTGRES_PASSWORD=secure_prod_password
set REDIS_PASSWORD=secure_prod_password
set RABBITMQ_PASSWORD=secure_prod_password

# Linux/Mac
export POSTGRES_PASSWORD=secure_prod_password
export REDIS_PASSWORD=secure_prod_password
export RABBITMQ_PASSWORD=secure_prod_password
```

### Validate required variables

```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml config
```

Expected: No errors about missing required variables

### Start prod environment

```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### Production-specific checks

```bash
# Check no external database ports exposed
docker port taskflow-postgres

# Check environment is Production
docker exec taskflow-task-service printenv | grep ASPNETCORE_ENVIRONMENT

# Check sensitive logging disabled
docker exec taskflow-task-service printenv | grep EnableSensitiveDataLogging

# Check resource limits
docker inspect taskflow-task-service | grep -A 10 Resources
```

Expected:
- No ports exposed for postgres
- ASPNETCORE_ENVIRONMENT=Production
- EnableSensitiveDataLogging=false
- Resource limits configured

### Test service replication

```bash
# Scale task service
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --scale task-service=3

# Check replicas
docker-compose -f docker-compose.yml -f docker-compose.prod.yml ps task-service
```

Expected: 3 task-service containers running

### Stop prod environment

```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down
```

## Test 6: Network Testing

### Start services

```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml up -d
```

### Test inter-service connectivity

```bash
# Test task-service can reach postgres
docker exec taskflow-task-service ping -c 3 postgres

# Test task-service can reach redis
docker exec taskflow-task-service ping -c 3 redis

# Test task-service can reach rabbitmq
docker exec taskflow-task-service ping -c 3 rabbitmq
```

Expected: Successful pings

### Test network isolation

```bash
# List networks
docker network ls | grep taskflow

# Inspect network
docker network inspect taskflow-network
```

Expected: All containers connected to taskflow-network

## Test 7: Volume Persistence Testing

### Create test data

```bash
# Start services
docker-compose -f docker-compose.yml -f docker-compose.local.yml up -d

# Create a task via API
curl -X POST http://localhost:5001/api/v1/tasks \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Persistence Test",
    "description": "Testing data persistence",
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "priority": "High"
  }'

# Note the task ID from response
```

### Stop and remove containers

```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml down
```

### Restart services

```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml up -d
```

### Verify data persisted

```bash
# Get all tasks
curl http://localhost:5001/api/v1/tasks
```

Expected: Previously created task still exists

## Test 8: Performance Testing

### Start services with resource monitoring

```bash
docker-compose -f docker-compose.yml -f docker-compose.local.yml up -d

# Monitor resources
docker stats
```

### Load testing (optional, requires Apache Bench)

```bash
# Install Apache Bench
# Windows: Download from Apache website
# Linux: sudo apt-get install apache2-utils
# Mac: brew install httpd

# Run load test
ab -n 1000 -c 10 http://localhost:5001/health
```

Expected: Services handle load without crashing

## Test 9: Logging Testing

### Check log output

```bash
# Start with logs visible
docker-compose -f docker-compose.yml -f docker-compose.local.yml up

# Or view logs after start
docker-compose -f docker-compose.yml -f docker-compose.local.yml logs -f task-service
```

Expected: Logs showing:
- Application startup
- Database migrations
- Health check passes
- API requests

## Test 10: Cleanup Testing

### Remove all resources

```bash
# Stop and remove containers, networks, volumes
docker-compose -f docker-compose.yml -f docker-compose.local.yml down -v

# Remove images
docker rmi taskflow-task-service:test

# Verify cleanup
docker ps -a | grep taskflow
docker volume ls | grep taskflow
docker network ls | grep taskflow
docker images | grep taskflow
```

Expected: No TaskFlow resources remaining

## Common Issues and Solutions

### Issue 1: Port already in use

**Error**: `Bind for 0.0.0.0:5432 failed: port is already allocated`

**Solution**:
```bash
# Find process using port
netstat -ano | findstr :5432

# Kill process or change port in docker-compose.local.yml
```

### Issue 2: Database connection failed

**Error**: `Connection refused` or `could not connect to server`

**Solution**:
```bash
# Check postgres health
docker inspect taskflow-postgres | grep Health

# Check logs
docker logs taskflow-postgres

# Restart postgres
docker restart taskflow-postgres
```

### Issue 3: Service unhealthy

**Error**: Service shows as "unhealthy" in docker ps

**Solution**:
```bash
# Check health check logs
docker inspect --format='{{json .State.Health}}' taskflow-task-service | jq

# Check service logs
docker logs taskflow-task-service

# Increase health check timeout in docker-compose.yml
```

### Issue 4: Build fails

**Error**: Build errors during `docker build`

**Solution**:
```bash
# Check .dockerignore excludes correct files
# Verify all project files exist
# Check Dockerfile paths are correct

# Build with no cache
docker build --no-cache -t taskflow-task-service:test -f src/Services/Task/Dockerfile .
```

### Issue 5: Migration fails

**Error**: Database migration errors in logs

**Solution**:
```bash
# Run migrations manually
docker exec taskflow-task-service dotnet ef database update

# Or recreate database
docker-compose down -v
docker-compose up -d
```

## Success Criteria

All tests pass when:

- ✅ All Docker configuration files are valid
- ✅ Docker image builds successfully
- ✅ All containers start and become healthy
- ✅ Database connections work
- ✅ API endpoints respond correctly
- ✅ Data persists across container restarts
- ✅ Services can communicate with each other
- ✅ Health checks pass consistently
- ✅ No critical errors in logs
- ✅ Resource usage is within acceptable limits

## Next Steps

After successful testing:

1. Configure CI/CD pipeline for automated builds
2. Set up monitoring and alerting
3. Configure backup and restore procedures
4. Implement security hardening for production
5. Set up load balancing and scaling
6. Configure SSL/TLS certificates
7. Implement centralized logging
8. Set up distributed tracing

## Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Best Practices](https://docs.docker.com/compose/compose-file/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [TaskFlow Docker Guide](./DOCKER.md)
