# 📚 Swagger API Documentation Aggregation - Complete Guide

## 🎯 Overview

The TaskFlow API Gateway successfully aggregates Swagger/OpenAPI documentation from all downstream microservices into a unified interface. This allows developers to:

- View all microservice APIs in one place
- Test endpoints directly from the API Gateway
- Understand the complete system architecture
- Maintain consistent API documentation

---

## 🌐 Access URLs

### Main Swagger UI (Aggregated View)
**Primary URL**: http://localhost:5000/swagger

This provides a dropdown menu to switch between different services:
- 🌐 API Gateway (All Services)
- 🔐 Identity Service
- 👤 User Service
- ✅ Task Service
- 👑 Admin Service
- 📧 Notification Service

### Individual Swagger JSON Endpoints

#### Via API Gateway (Aggregated)
- **Gateway**: http://localhost:5000/swagger/gateway/swagger.json
- **Identity**: http://localhost:5000/swagger/identity/swagger.json
- **User**: http://localhost:5000/swagger/user/swagger.json
- **Task**: http://localhost:5000/swagger/task/swagger.json
- **Admin**: http://localhost:5000/swagger/admin/swagger.json
- **Notification**: http://localhost:5000/swagger/notification/swagger.json

#### Direct Service Access (For Development)
- **Identity**: http://localhost:5006/swagger
- **User**: http://localhost:5001/swagger
- **Task**: http://localhost:5005/swagger
- **Admin**: http://localhost:5007/swagger
- **Notification**: http://localhost:5004/swagger

---

## 🏗️ Architecture

### How It Works

```
┌─────────────────────────────────────────────────────────────┐
│                     API Gateway (Port 5000)                  │
│  ┌───────────────────────────────────────────────────────┐  │
│  │          Swagger UI (http://localhost:5000/swagger)   │  │
│  └───────────────────────────────────────────────────────┘  │
│                             │                                │
│                             ▼                                │
│  ┌───────────────────────────────────────────────────────┐  │
│  │      SwaggerForwardingMiddleware                      │  │
│  │  - Intercepts /swagger/{service}/swagger.json         │  │
│  │  - Forwards request to downstream service             │  │
│  │  - Returns aggregated documentation                   │  │
│  └───────────────────────────────────────────────────────┘  │
│                             │                                │
└─────────────────────────────┼────────────────────────────────┘
                              │
              ┌───────────────┴───────────────┐
              │                               │
              ▼                               ▼
   ┌──────────────────┐          ┌──────────────────┐
   │  Identity Service│          │   User Service   │
   │   (Port 5006)    │          │   (Port 5001)    │
   │  /swagger/v1/... │          │  /swagger/v1/... │
   └──────────────────┘          └──────────────────┘
              │                               │
              ▼                               ▼
   ┌──────────────────┐          ┌──────────────────┐
   │   Task Service   │          │   Admin Service  │
   │   (Port 5005)    │          │   (Port 5007)    │
   │  /swagger/v1/... │          │  /swagger/v1/... │
   └──────────────────┘          └──────────────────┘
              │
              ▼
   ┌──────────────────┐
   │ Notification Svc │
   │   (Port 5004)    │
   │  /swagger/v1/... │
   └──────────────────┘
```

### Key Components

#### 1. SwaggerForwardingMiddleware
Located: `src/ApiGateway/TaskFlow.Gateway/Middleware/SwaggerForwardingMiddleware.cs`

**Responsibilities**:
- Intercepts Swagger JSON requests
- Forwards requests to downstream microservices
- Handles connection failures gracefully
- Provides fallback placeholder documentation

**Service Endpoint Mapping**:
```csharp
private static readonly Dictionary<string, string> ServiceEndpoints = new()
{
    { "identity", "http://identity-service:8080" },
    { "user", "http://user-service:8080" },
    { "task", "http://task-service:8080" },
    { "admin", "http://admin-service:8080" },
    { "notification", "http://notif-service:8080" }
};
```

#### 2. SwaggerConfiguration
Located: `src/ApiGateway/TaskFlow.Gateway/Configuration/SwaggerConfiguration.cs`

**Responsibilities**:
- Defines Swagger documents for each service
- Configures Swagger UI with multiple endpoints
- Sets up JWT authentication in Swagger
- Customizes documentation appearance

#### 3. Downstream Service Configuration
All microservices have been updated to:
- Enable Swagger in all environments (not just Development)
- Configure OpenAPI metadata (title, version, description)
- Expose Swagger JSON at `/swagger/v1/swagger.json`

---

## 🚀 Usage Guide

### 1. Start All Services

```bash
docker-compose up -d
```

Wait for all services to be healthy:
```bash
docker-compose ps
```

### 2. Access Swagger UI

Open your browser and navigate to:
```
http://localhost:5000/swagger
```

### 3. Explore Service Documentation

1. **Select a Service**: Use the dropdown in the top-right corner
2. **View Endpoints**: Expand endpoint sections to see details
3. **Authorize**: Click "Authorize" button to add JWT token
4. **Test Endpoints**: Use "Try it out" to execute requests

### 4. Authentication Flow

For endpoints requiring authentication:

1. **Register a User** (if needed):
   - Select "Identity Service" from dropdown
   - Find `POST /api/v1/auth/register`
   - Click "Try it out"
   - Fill in the request body
   - Execute

2. **Login**:
   - Select "Identity Service"
   - Find `POST /api/v1/auth/login`
   - Provide credentials
   - Copy the `accessToken` from response

3. **Authorize**:
   - Click the "Authorize" button (🔒 icon)
   - Enter the token (without "Bearer" prefix)
   - Click "Authorize"

4. **Test Protected Endpoints**:
   - All subsequent requests will include the JWT token
   - Navigate to other services and test their endpoints

---

## 🔧 Configuration Files Modified

### API Gateway

#### `Program.cs`
Added:
- `HttpClientFactory` for downstream requests
- `SwaggerForwardingMiddleware` in pipeline

#### `SwaggerConfiguration.cs`
Configured:
- Multiple Swagger documents (gateway, identity, user, task, admin, notification)
- JWT Bearer authentication
- Service-specific route filtering

#### `SwaggerForwardingMiddleware.cs` (New)
Created custom middleware to:
- Forward Swagger JSON requests to downstream services
- Handle connection failures
- Transform paths if needed

### Downstream Services

All services (`Identity`, `User`, `Task`, `Admin`, `Notif`) updated:

**Program.cs changes**:
```csharp
// Before
builder.Services.AddSwaggerGen();

// After
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskFlow {Service} Service API",
        Version = "v1",
        Description = "{Service} Service - {Description}"
    });
});

// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "{Service} Service API v1");
    c.RoutePrefix = "swagger";
});
```

---

## 📊 Service Details

### Identity Service
- **Description**: Authentication and user management
- **Port**: 5006
- **Key Features**:
  - User registration with email confirmation
  - JWT-based authentication
  - Password reset flow
  - Role-based authorization (User, Admin, SuperAdmin)

### User Service
- **Description**: User profile and preferences management
- **Port**: 5001
- **Key Features**:
  - User profile CRUD operations
  - Profile settings management
  - User preferences
  - Avatar upload

### Task Service
- **Description**: Task management and tracking
- **Port**: 5005
- **Key Features**:
  - Task CRUD operations
  - Task assignment
  - Priority management
  - Status tracking

### Admin Service
- **Description**: Administrative operations and system management
- **Port**: 5007
- **Key Features**:
  - User activation/deactivation
  - Role assignment
  - System configuration
  - Audit logs

### Notification Service
- **Description**: Notification delivery and management
- **Port**: 5004
- **Key Features**:
  - Email notifications
  - Push notifications
  - Notification preferences
  - Notification history

---

## 🧪 Testing the Aggregation

### Test All Endpoints

```bash
# Test Gateway's own documentation
curl http://localhost:5000/swagger/gateway/swagger.json

# Test Identity Service (via gateway)
curl http://localhost:5000/swagger/identity/swagger.json

# Test User Service (via gateway)
curl http://localhost:5000/swagger/user/swagger.json

# Test Task Service (via gateway)
curl http://localhost:5000/swagger/task/swagger.json

# Test Admin Service (via gateway)
curl http://localhost:5000/swagger/admin/swagger.json

# Test Notification Service (via gateway)
curl http://localhost:5000/swagger/notification/swagger.json
```

All endpoints should return HTTP 200 with valid OpenAPI JSON.

### Test Service Health

```bash
# Check if all services are running
docker-compose ps

# View gateway logs
docker-compose logs -f api-gateway

# Test direct service access
curl http://localhost:5006/swagger/v1/swagger.json  # Identity
curl http://localhost:5001/swagger/v1/swagger.json  # User
curl http://localhost:5005/swagger/v1/swagger.json  # Task
curl http://localhost:5007/swagger/v1/swagger.json  # Admin
curl http://localhost:5004/swagger/v1/swagger.json  # Notification
```

---

## 🎨 Customization

### Add New Service to Aggregation

1. **Update SwaggerForwardingMiddleware.cs**:
```csharp
private static readonly Dictionary<string, string> ServiceEndpoints = new()
{
    // ... existing services ...
    { "newservice", "http://newservice-service:8080" }
};
```

2. **Update SwaggerConfiguration.cs**:
```csharp
// Add service constant
public static class Services
{
    // ... existing services ...
    public const string NewService = "newservice";
}

// Add Swagger document
options.SwaggerDoc(Services.NewService, new OpenApiInfo
{
    Title = "New Service API",
    Version = "v1",
    Description = "Description of new service"
});

// Add Swagger endpoint
options.SwaggerEndpoint("/swagger/newservice/swagger.json", "🆕 New Service");

// Add route filtering
return docName switch
{
    // ... existing mappings ...
    Services.NewService => routePath.Contains("/newservice"),
    _ => false
};
```

3. **Configure new service to expose Swagger**:
   - Follow the pattern in existing services
   - Enable Swagger in all environments
   - Add OpenAPI metadata

---

## 🐛 Troubleshooting

### Issue: Service Not Showing in Dropdown

**Possible Causes**:
- Service is not running
- Service doesn't have Swagger enabled
- Network connectivity issue

**Solutions**:
```bash
# Check service status
docker-compose ps

# Check service logs
docker-compose logs -f {service-name}

# Restart service
docker-compose restart {service-name}

# Test direct access
curl http://localhost:{port}/swagger/v1/swagger.json
```

### Issue: "Could not fetch API documentation"

This is expected if:
- Service is still starting up (wait 30-60 seconds)
- Service crashed (check logs)
- Service doesn't have controllers/endpoints yet

**Check**:
```bash
# View gateway logs for detailed error
docker-compose logs api-gateway | grep -i swagger

# Test downstream service directly
curl http://localhost:{port}/health
curl http://localhost:{port}/swagger/v1/swagger.json
```

### Issue: Changes Not Reflected

**Solution**:
```bash
# Rebuild and restart services
docker-compose build api-gateway {service-name}
docker-compose up -d api-gateway {service-name}

# Force rebuild from scratch
docker-compose build --no-cache api-gateway {service-name}
docker-compose up -d api-gateway {service-name}
```

---

## 📝 Development Workflow

### Adding New API Endpoints

1. **Create endpoint in service** (e.g., User Service):
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(Guid id)
{
    // Implementation
}
```

2. **Add XML comments** (optional, for better docs):
```csharp
/// <summary>
/// Get user by ID
/// </summary>
/// <param name="id">User unique identifier</param>
/// <returns>User details</returns>
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(Guid id)
{
    // Implementation
}
```

3. **Rebuild service**:
```bash
docker-compose build user-service
docker-compose up -d user-service
```

4. **Verify in Swagger UI**:
   - Navigate to http://localhost:5000/swagger
   - Select "User Service"
   - New endpoint should appear automatically

### Testing New Endpoints

1. Open Swagger UI
2. Select appropriate service
3. Find your endpoint
4. Click "Try it out"
5. Fill in parameters
6. Execute and verify response

---

## 🎯 Best Practices

### 1. Consistent Documentation
- Always add OpenAPI attributes to controllers
- Use descriptive titles and descriptions
- Document request/response models
- Include example values

### 2. Version Management
- Keep API versions in URLs (`/api/v1/...`)
- Document breaking changes
- Maintain backward compatibility when possible

### 3. Security
- Always test authentication flows
- Document required authorization levels
- Use HTTPS in production
- Rotate JWT secrets regularly

### 4. Performance
- Use lightweight DTOs
- Implement pagination for list endpoints
- Cache responses when appropriate
- Monitor response times

---

## 📚 Additional Resources

### Related Documentation
- [API Gateway Configuration](./src/ApiGateway/TaskFlow.Gateway/appsettings.json)
- [Docker Compose Setup](./docker-compose.yml)
- [Service Discovery (Consul)](http://localhost:8500)
- [Log Aggregation (Seq)](http://localhost:5341)
- [Distributed Tracing (Jaeger)](http://localhost:16686)

### Swagger/OpenAPI Specifications
- [OpenAPI 3.0 Specification](https://swagger.io/specification/)
- [Swashbuckle Documentation](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [ASP.NET Core Web API Help Pages](https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger)

---

## ✅ Verification Checklist

After setup, verify:

- [ ] API Gateway Swagger UI loads at http://localhost:5000/swagger
- [ ] Dropdown shows all 6 service options
- [ ] Each service's Swagger JSON is accessible
- [ ] Authentication flow works (register → login → authorize)
- [ ] Protected endpoints require JWT token
- [ ] All microservices are running and healthy
- [ ] Direct service Swagger UIs are accessible (ports 5001, 5004, 5005, 5006, 5007)
- [ ] YARP reverse proxy routes requests correctly
- [ ] No errors in gateway logs related to Swagger

---

## 🎉 Success!

Your TaskFlow API Gateway now provides a unified Swagger documentation interface that:

✅ Aggregates all microservice APIs in one place
✅ Allows testing endpoints directly from the browser
✅ Supports JWT authentication
✅ Provides detailed API documentation
✅ Handles service failures gracefully
✅ Updates automatically when services change

**Main URL**: http://localhost:5000/swagger

Happy API exploring! 🚀
