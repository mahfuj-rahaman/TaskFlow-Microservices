# 🚀 Visual Studio Quick Start Guide

**TaskFlow Microservices - Docker Compose Edition**

---

## ⚡ 1-Minute Setup

### Step 1: Open Solution
```
1. Launch Visual Studio 2022
2. File → Open → Project/Solution
3. Navigate to: TaskFlow-Microservices\main
4. Select: TaskFlow.sln
5. Click "Open"
```

### Step 2: Set Docker Compose as Startup
```
1. In Solution Explorer, find "docker-compose" project
2. Right-click on "docker-compose"
3. Select "Set as Startup Project"
4. The project will be BOLD when selected
```

### Step 3: Run with Docker
```
Press F5
(or click "Docker Compose" button in toolbar)
```

**That's it!** Visual Studio will:
- ✅ Build all Docker images
- ✅ Start infrastructure (Postgres, Redis, RabbitMQ, Consul, Seq, Jaeger)
- ✅ Start all 6 microservices
- ✅ Open browser to http://localhost:5000
- ✅ Attach debugger for breakpoint support

---

## 🎯 What You'll See

### During Startup

**Output Window** (`View → Output`):
```
Building docker-compose...
Starting taskflow-postgres...
Starting taskflow-redis...
Starting taskflow-rabbitmq...
Starting taskflow-consul...
Starting taskflow-seq...
Starting taskflow-jaeger...
Starting taskflow-api-gateway...
Starting taskflow-identity-service...
Starting taskflow-user-service...
Starting taskflow-task-service...
Starting taskflow-admin-service...
Starting taskflow-notif-service...
All services started successfully!
```

**Container Window** (`View → Other Windows → Containers`):
```
✅ taskflow-postgres          (healthy)
✅ taskflow-redis             (healthy)
✅ taskflow-rabbitmq          (healthy)
✅ taskflow-consul            (healthy)
✅ taskflow-seq               (running)
✅ taskflow-jaeger            (running)
✅ taskflow-api-gateway       (healthy)
✅ taskflow-identity-service  (healthy)
✅ taskflow-user-service      (healthy)
✅ taskflow-task-service      (healthy)
✅ taskflow-admin-service     (healthy)
✅ taskflow-notif-service     (healthy)
```

**Browser Opens Automatically**:
```
http://localhost:5000
→ API Gateway landing page
```

---

## 🐛 Debugging

### Setting Breakpoints

**Example: Debug Identity Service Login**

1. **Open the Login Handler**:
   ```
   Solution Explorer →
   TaskFlow.Identity.Application →
   Features → Authentication →
   LoginCommandHandler.cs
   ```

2. **Set Breakpoint**:
   ```
   Click in the left margin next to line (red dot appears)
   Example: Line with "var user = await..."
   ```

3. **Start Debugging**:
   ```
   Press F5 (if not already running)
   ```

4. **Trigger the Endpoint**:
   ```
   Open browser: http://localhost:5000/swagger
   Navigate to: POST /api/v1/auth/login
   Click "Try it out"
   Enter credentials:
   {
     "emailOrUsername": "testuser",
     "password": "Test@1234"
   }
   Click "Execute"
   ```

5. **Breakpoint Hits!**:
   ```
   Visual Studio switches to debugger
   Code execution pauses at your breakpoint
   Inspect variables in Locals window
   Step through code with F10 (step over) or F11 (step into)
   Continue with F5
   ```

### Debug Multiple Services

**Scenario: Debug request flow across services**

1. Set breakpoints in:
   - Gateway controller
   - Identity service handler
   - User service handler

2. Make API call through Gateway

3. Visual Studio will hit breakpoints in order:
   - First: Gateway (receives request)
   - Second: Identity (processes authentication)
   - Third: User (retrieves profile)

4. Step through entire microservices flow!

### View Logs While Debugging

**Option 1: Container Logs in Visual Studio**
```
View → Other Windows → Containers
Right-click service → View Logs
```

**Option 2: Seq UI**
```
Browser: http://localhost:5341
All service logs in one place with search/filter
```

**Option 3: Output Window**
```
View → Output
Select "Show output from: Docker"
```

---

## 🔧 Common Tasks

### Restart a Service

**Method 1: In Visual Studio**
```
View → Other Windows → Containers
Right-click service → Restart
```

**Method 2: In Terminal**
```
Tools → Command Line → Developer PowerShell
docker-compose restart identity-service
```

### View Service Logs

**Method 1: Container Window**
```
View → Other Windows → Containers
Right-click service → View Logs
```

**Method 2: Terminal**
```
docker-compose logs -f identity-service
```

### Rebuild a Service

**When to rebuild**:
- After changing Dockerfile
- After adding new NuGet packages
- After major code changes

**How to rebuild**:
```
1. Stop debugging (Shift+F5)
2. Terminal: docker-compose build identity-service
3. Or rebuild all: docker-compose build
4. Start debugging (F5)
```

### Access Database

**Using Visual Studio SQL Server Object Explorer**:
```
View → SQL Server Object Explorer
Add Connection:
  Server: localhost,5432
  Auth: SQL Server Authentication
  User: postgres
  Password: postgres
  Database: taskflow_identity
```

**Using Terminal**:
```
docker-compose exec postgres psql -U postgres
\l                    # List databases
\c taskflow_identity  # Connect to database
\dt                   # List tables
SELECT * FROM "AppUsers";
```

### Stop All Services

**Method 1: Visual Studio**
```
Click "Stop" button (Shift+F5)
or
Debug → Stop Debugging
```

**Method 2: Terminal**
```
docker-compose stop
```

**Method 3: Stop and Remove Containers**
```
docker-compose down
```

---

## 📊 Monitoring & Admin Panels

### Service Access Points

Open these URLs in your browser:

**API & Documentation**:
- 🌐 API Gateway: http://localhost:5000
- 📚 Swagger Docs: http://localhost:5000/swagger
- 📋 API Info: http://localhost:5000/api/info

**Infrastructure UIs**:
- 🔍 **Consul** - Service Discovery: http://localhost:8500
  - View registered services
  - Check health status
  - View service metadata

- 🐰 **RabbitMQ** - Message Broker: http://localhost:15672
  - Username: `rabbitmq`
  - Password: `rabbitmq`
  - View queues, exchanges, messages
  - Monitor message rates

- 📝 **Seq** - Centralized Logs: http://localhost:5341
  - Search all service logs
  - Filter by service, level, timestamp
  - Create dashboards
  - Set up alerts

- 📊 **Jaeger** - Distributed Tracing: http://localhost:16686
  - Trace requests across services
  - View service dependencies
  - Analyze performance bottlenecks
  - See request timelines

**Direct Service Access** (for testing):
- Identity: http://localhost:5006
- User: http://localhost:5001
- Task: http://localhost:5005
- Admin: http://localhost:5007
- Notification: http://localhost:5004

---

## 🎨 Visual Studio Features

### Solution Explorer Views

**docker-compose Project**:
```
docker-compose (Startup Project - BOLD)
  ├── docker-compose.yml          (Base configuration)
  ├── docker-compose.override.yml (Development overrides)
  ├── .dockerignore               (Files to exclude)
  └── Properties
      └── launchSettings.json     (VS launch configuration)
```

**Services in Solution**:
```
Solution 'TaskFlow'
├── src
│   ├── ApiGateway
│   │   └── TaskFlow.Gateway
│   ├── Services
│   │   ├── Identity
│   │   │   ├── TaskFlow.Identity.Domain
│   │   │   ├── TaskFlow.Identity.Application
│   │   │   ├── TaskFlow.Identity.Infrastructure
│   │   │   └── TaskFlow.Identity.API
│   │   ├── User
│   │   ├── Task
│   │   ├── Admin
│   │   └── Notif
│   └── BuildingBlocks
└── docker-compose (★ Startup Project)
```

### Container Management

**View → Other Windows → Containers**:
```
Shows all running containers with:
- Status (Running, Stopped, Healthy)
- CPU usage
- Memory usage
- Port mappings
- Logs (right-click → View Logs)
- Terminal access (right-click → Open Terminal)
```

### Docker Images

**View → Other Windows → Containers → Images tab**:
```
Shows built Docker images:
- taskflow-api-gateway
- taskflow-identity-service
- taskflow-user-service
- taskflow-task-service
- taskflow-admin-service
- taskflow-notif-service
```

---

## ⚙️ Configuration

### Environment Variables

**Edit docker-compose.override.yml**:
```yaml
services:
  identity-service:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__IdentityDb=...
      - JwtSettings__SecretKey=...
```

**Or use .env file** (root directory):
```bash
POSTGRES_PASSWORD=my-secure-password
REDIS_PASSWORD=my-redis-password
JWT_SECRET_KEY=my-256-bit-secret-key
```

### User Secrets (Recommended for sensitive data)

**Right-click on service project** (e.g., TaskFlow.Identity.API):
```
Manage User Secrets
```

**Add secrets** (secrets.json):
```json
{
  "JwtSettings:SecretKey": "your-secret-key",
  "ConnectionStrings:IdentityDb": "your-connection-string"
}
```

Secrets are stored in:
```
%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json
```

---

## 🐛 Troubleshooting

### Issue 1: "docker-compose not found"

**Solution**:
```
Tools → Options → Container Tools → Docker Compose
Verify Docker Desktop is running
Restart Visual Studio
```

### Issue 2: Port Already in Use

**Symptoms**: `Port 5000 is already in use`

**Solution**:
```
# Find process using port
netstat -ano | findstr :5000

# Kill process
taskkill /PID <process_id> /F

# Or change port in docker-compose.override.yml
ports:
  - "5050:8080"  # Use 5050 instead
```

### Issue 3: Services Not Starting

**Symptoms**: Containers keep restarting

**Solution**:
```
1. View logs: Containers window → Right-click → View Logs
2. Check health: docker-compose ps
3. Rebuild: docker-compose build --no-cache
4. Clean start: docker-compose down -v && docker-compose up -d
```

### Issue 4: Breakpoints Not Hitting

**Solution**:
```
1. Ensure Debug configuration (not Release)
2. Rebuild container: docker-compose build identity-service
3. Verify debugger attached: Debug → Attach to Process → docker
4. Check Output window for debugger messages
```

### Issue 5: Out of Memory

**Solution**:
```
Docker Desktop → Settings → Resources → Memory
Increase to 12GB or higher
Restart Docker Desktop
```

---

## 📝 Pro Tips

### Tip 1: Faster Rebuilds
```
# Rebuild only changed service
docker-compose build identity-service

# Use cached layers
docker-compose build --parallel
```

### Tip 2: View All Logs at Once
```
# Terminal
docker-compose logs -f

# Or use Seq UI for searchable logs
http://localhost:5341
```

### Tip 3: Quick Database Reset
```
docker-compose down -v
docker-compose up -d postgres
# Wait 10 seconds for init
docker-compose up -d
```

### Tip 4: Hot Reload for Development
```
1. Keep container running (F5)
2. Edit code in service
3. Save (Ctrl+S)
4. Visual Studio auto-reloads (for supported changes)
```

### Tip 5: Debug Multiple Services Simultaneously
```
1. Set breakpoints in multiple services
2. Make API call through Gateway
3. Step through entire request flow
4. Use Call Stack window to navigate
```

---

## 🎯 Cheat Sheet

### Quick Commands

| Action | Keyboard | Menu |
|--------|----------|------|
| Start with Debug | F5 | Debug → Start Debugging |
| Start without Debug | Ctrl+F5 | Debug → Start Without Debugging |
| Stop Debugging | Shift+F5 | Debug → Stop Debugging |
| Restart | Ctrl+Shift+F5 | Debug → Restart |
| View Containers | - | View → Other Windows → Containers |
| View Output | Ctrl+Alt+O | View → Output |
| Open Terminal | Ctrl+` | View → Terminal |

### URLs Reference

```
Gateway:      http://localhost:5000
Swagger:      http://localhost:5000/swagger
Consul:       http://localhost:8500
RabbitMQ:     http://localhost:15672
Seq:          http://localhost:5341
Jaeger:       http://localhost:16686
```

### Docker Compose Commands

```bash
# Start all
docker-compose up -d

# Stop all
docker-compose stop

# Restart service
docker-compose restart identity-service

# View logs
docker-compose logs -f identity-service

# Rebuild
docker-compose build identity-service

# Clean everything
docker-compose down -v
```

---

## 🎉 You're Ready!

**Now you can**:
✅ Run entire microservices stack with F5
✅ Debug with breakpoints across services
✅ View logs in real-time
✅ Monitor services with admin UIs
✅ Hot reload code changes
✅ Manage containers in Visual Studio

**Press F5 and start building!** 🚀

---

**Last Updated**: 2025-11-04
**Visual Studio**: 2022 (17.8+)
**Docker Compose**: 2.23+
