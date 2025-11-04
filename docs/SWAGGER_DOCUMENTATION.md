# 📚 TaskFlow API Gateway - Swagger Documentation Guide

**Version**: 1.0
**Last Updated**: 2025-11-04
**Access**: `http://localhost:5000/swagger`

---

## 🎯 Overview

The TaskFlow API Gateway includes comprehensive **Swagger/OpenAPI** documentation that automatically aggregates and presents all downstream microservices in a unified interface.

### Key Features

✅ **Multi-Service Documentation** - All services in one place
✅ **Interactive Testing** - Try API endpoints directly from browser
✅ **JWT Authentication** - Built-in authorization flow
✅ **Custom Branding** - TaskFlow-themed UI
✅ **Postman Export** - Download collection for Postman
✅ **API Versioning** - Clear version management (v1)
✅ **Service Discovery** - Automatic downstream service routing

---

## 🚀 Quick Start

### 1. Start the Gateway

```bash
cd src/ApiGateway/TaskFlow.Gateway
dotnet run
```

### 2. Access Swagger UI

Open your browser and navigate to:
```
http://localhost:5000/swagger
```

### 3. Explore Available Services

You'll see 6 documentation groups:

- 🌐 **API Gateway** (All Services) - Complete overview
- 🔐 **Identity Service** - Authentication & user management
- 👤 **User Service** - User profiles
- ✅ **Task Service** - Task management
- 👑 **Admin Service** - Administrative operations
- 📧 **Notification Service** - Notifications

---

## 🔐 Authentication Flow

### Step 1: Register a User

1. Switch to **Identity Service** documentation
2. Find **POST /api/v1/auth/register** endpoint
3. Click **Try it out**
4. Use this sample request:

```json
{
  "username": "testuser",
  "email": "test@taskflow.com",
  "password": "Test@1234",
  "firstName": "Test",
  "lastName": "User"
}
```

5. Click **Execute**
6. You should receive **201 Created**

### Step 2: Login and Get JWT Token

1. Find **POST /api/v1/auth/login** endpoint
2. Click **Try it out**
3. Use this sample request:

```json
{
  "emailOrUsername": "testuser",
  "password": "Test@1234"
}
```

4. Click **Execute**
5. Copy the `accessToken` from the response:

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "abc123...",
  "expiresAt": "2025-11-04T12:30:00Z"
}
```

### Step 3: Authorize in Swagger

1. Click the **Authorize** 🔓 button (top right)
2. In the dialog, paste your JWT token (just the token, not "Bearer")
3. Click **Authorize**
4. Click **Close**
5. The lock icon should now be **locked** 🔒

### Step 4: Test Protected Endpoints

Now you can call authenticated endpoints:

1. Find **GET /api/v1/identity/appusers** (requires auth)
2. Click **Try it out**
3. Click **Execute**
4. You should receive **200 OK** with user list

---

## 📖 Available Endpoints

### Gateway Endpoints

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| GET | `/api/info` | Get API Gateway information | ❌ |
| GET | `/api/health` | Gateway health check | ❌ |
| GET | `/api/postman-collection` | Download Postman collection | ❌ |

### Identity Service Endpoints

**Authentication** (Public):
| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | `/api/v1/auth/register` | Register new user | ❌ |
| POST | `/api/v1/auth/login` | Login (get JWT token) | ❌ |
| POST | `/api/v1/auth/refresh-token` | Refresh access token | ❌ |
| POST | `/api/v1/auth/logout` | Logout (revoke token) | ✅ |
| POST | `/api/v1/auth/forgot-password` | Request password reset | ❌ |
| POST | `/api/v1/auth/reset-password` | Reset password | ❌ |
| POST | `/api/v1/auth/confirm-email` | Confirm email | ❌ |

**User Management** (Authenticated):
| Method | Path | Description | Auth |
|--------|------|-------------|------|
| GET | `/api/v1/identity/appusers` | List all users | ✅ |
| GET | `/api/v1/identity/appusers/{id}` | Get user by ID | ✅ |
| POST | `/api/v1/identity/appusers` | Create user | ✅ |
| PUT | `/api/v1/identity/appusers/{id}` | Update user | ✅ |
| DELETE | `/api/v1/identity/appusers/{id}` | Delete user | ✅ Admin |

### User Service Endpoints

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| GET | `/api/v1/users` | List users | ✅ |
| GET | `/api/v1/users/{id}` | Get user profile | ✅ |
| PUT | `/api/v1/users/{id}` | Update profile | ✅ |

### Task Service Endpoints

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| GET | `/api/v1/tasks` | List tasks | ✅ |
| POST | `/api/v1/tasks` | Create task | ✅ |
| GET | `/api/v1/tasks/{id}` | Get task | ✅ |
| PUT | `/api/v1/tasks/{id}` | Update task | ✅ |
| DELETE | `/api/v1/tasks/{id}` | Delete task | ✅ |

### Admin Service Endpoints

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| GET | `/api/v1/admin/users` | List all users (admin) | ✅ Admin |
| POST | `/api/v1/admin/users/{id}/activate` | Activate user | ✅ Admin |
| POST | `/api/v1/admin/users/{id}/deactivate` | Deactivate user | ✅ Admin |
| POST | `/api/v1/admin/users/{id}/lock` | Lock user account | ✅ Admin |
| POST | `/api/v1/admin/users/{id}/assign-role` | Assign role | ✅ SuperAdmin |

---

## 🎨 Swagger UI Features

### 1. Interactive API Testing

- **Try it out** - Execute requests directly from browser
- **Request/Response display** - See full HTTP details
- **Schema validation** - Auto-validate request bodies
- **Response samples** - Example responses for each status code

### 2. Authentication Support

- **Bearer token authorization** - Click lock icon to authorize
- **Token expiration handling** - Clear expiration messages
- **Automatic header injection** - Token added to all requests

### 3. Custom Styling

- **TaskFlow branding** - Custom colors and layout
- **Dark mode support** - Automatic detection
- **Responsive design** - Works on mobile and desktop
- **Custom scrollbars** - Branded appearance

### 4. Documentation Quality

- **Detailed descriptions** - Every endpoint documented
- **Request examples** - Sample payloads provided
- **Response codes** - All status codes explained
- **Schema definitions** - Complete model documentation

---

## 📥 Export & Integration

### Postman Collection

Download a ready-to-use Postman collection:

```bash
curl http://localhost:5000/api/postman-collection > taskflow-api.json
```

Then import in Postman:
1. Open Postman
2. Click **Import**
3. Select `taskflow-api.json`
4. The collection will include:
   - All endpoints
   - Pre-configured variables
   - Authentication setup
   - Sample requests

### OpenAPI Spec (JSON)

Download OpenAPI specifications for each service:

```bash
# Gateway spec
curl http://localhost:5000/swagger/gateway/swagger.json > gateway-openapi.json

# Identity service spec
curl http://localhost:5000/swagger/identity/swagger.json > identity-openapi.json

# User service spec
curl http://localhost:5000/swagger/user/swagger.json > user-openapi.json
```

Use these for:
- Code generation (clients, SDKs)
- API testing tools
- Documentation generators
- Contract testing

---

## 🔧 Configuration

### Swagger Settings

Located in `Program.cs`:

```csharp
builder.Services.AddSwaggerDocumentation(builder.Configuration);
```

### Customization Options

**1. Add New Service Documentation**

Edit `SwaggerConfiguration.cs`:

```csharp
options.SwaggerDoc("newservice", new OpenApiInfo
{
    Title = "New Service API",
    Version = "v1",
    Description = "Service description..."
});
```

**2. Modify JWT Settings**

Edit `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "TaskFlow.ApiGateway",
    "Audience": "TaskFlow.Services",
    "ExpirationMinutes": 15
  }
}
```

**3. Custom CSS**

Edit `wwwroot/swagger-custom.css` to change:
- Colors (`:root` variables)
- Fonts
- Layout
- Branding

**4. Environment-Specific Behavior**

Swagger is enabled by default in:
- ✅ Development
- ✅ Staging
- ❌ Production (disabled for security)

To enable in production:

```csharp
// In Program.cs, remove the environment check:
app.UseSwaggerDocumentation(app.Configuration);
```

---

## 🧪 Testing Workflows

### Workflow 1: User Registration & Profile Update

1. **Register** - POST `/api/v1/auth/register`
2. **Login** - POST `/api/v1/auth/login` (save token)
3. **Authorize** - Click 🔓 and paste token
4. **Get Profile** - GET `/api/v1/identity/appusers/{id}`
5. **Update Profile** - PUT `/api/v1/identity/appusers/{id}`

### Workflow 2: Task Management

1. **Login** - Get JWT token
2. **Authorize** - Apply token
3. **List Tasks** - GET `/api/v1/tasks`
4. **Create Task** - POST `/api/v1/tasks`
5. **Update Task** - PUT `/api/v1/tasks/{id}`
6. **Complete Task** - PUT `/api/v1/tasks/{id}` (update status)
7. **Delete Task** - DELETE `/api/v1/tasks/{id}`

### Workflow 3: Admin Operations

1. **Login as Admin** - Use admin credentials
2. **Authorize** - Apply admin token
3. **List All Users** - GET `/api/v1/admin/users`
4. **Deactivate User** - POST `/api/v1/admin/users/{id}/deactivate`
5. **Assign Role** - POST `/api/v1/admin/users/{id}/assign-role` (SuperAdmin only)

---

## 🎯 Best Practices

### For Developers

1. **Always Authorize First** - Click 🔓 before testing protected endpoints
2. **Check Response Codes** - Understand what each code means
3. **Use Schema Validation** - Swagger validates your requests
4. **Read Descriptions** - Every endpoint has detailed docs
5. **Test Edge Cases** - Try invalid data to see error handling

### For API Consumers

1. **Download OpenAPI Spec** - Use for code generation
2. **Export Postman Collection** - For team collaboration
3. **Bookmark Swagger URL** - Quick access to docs
4. **Check Examples** - Use provided sample payloads
5. **Understand Rate Limits** - 100 requests/minute

### For Administrators

1. **Disable in Production** - Unless behind authentication
2. **Monitor Usage** - Track who accesses Swagger
3. **Keep Docs Updated** - Reflect latest API changes
4. **Version Properly** - Use semantic versioning
5. **Secure JWT Secret** - Use strong keys in production

---

## 🐛 Troubleshooting

### Issue 1: Swagger Not Loading

**Symptoms**: 404 error at `/swagger`

**Solution**:
```bash
# Check environment
echo $ASPNETCORE_ENVIRONMENT

# Should be "Development" or "Staging"
# If "Production", Swagger is disabled by default
```

### Issue 2: Authentication Not Working

**Symptoms**: 401 Unauthorized despite valid token

**Solution**:
1. Check token expiration (default: 15 minutes)
2. Ensure you clicked **Authorize** and closed dialog
3. Verify token format (don't include "Bearer" prefix)
4. Check JWT settings match Identity service

### Issue 3: Missing Service Documentation

**Symptoms**: Service endpoints not showing

**Solution**:
1. Ensure service is running
2. Check route filtering in `SwaggerConfiguration.cs`
3. Verify service is registered in reverse proxy config
4. Restart Gateway

### Issue 4: CORS Errors

**Symptoms**: Swagger UI can't call API

**Solution**:
```json
// In appsettings.json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:5000"],
    "AllowCredentials": true
  }
}
```

---

## 📊 Service Status Indicators

In Swagger UI, you'll see service statuses:

| Status | Meaning | Color |
|--------|---------|-------|
| ✅ Available | Service is healthy | Green |
| ⚠️ Degraded | Service slow response | Yellow |
| ❌ Unavailable | Service not responding | Red |
| 🔄 Starting | Service initializing | Blue |

Check `/api/info` endpoint for real-time status.

---

## 🔗 Related Resources

- **API Gateway Guide**: `docs/API_GATEWAY_GUIDE.md`
- **Identity Specification**: `docs/features/Identity_feature.md`
- **Integration Summary**: `docs/INTEGRATION_SUMMARY.md`
- **Swagger Configuration**: `src/ApiGateway/TaskFlow.Gateway/Configuration/SwaggerConfiguration.cs`

---

## 📞 Support

### Quick Links

- Swagger UI: `http://localhost:5000/swagger`
- API Info: `http://localhost:5000/api/info`
- Health Check: `http://localhost:5000/api/health`
- Postman Collection: `http://localhost:5000/api/postman-collection`

### Common URLs

**Development**:
- Gateway: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`
- Identity Service: `http://localhost:5006`

**Production**:
- Gateway: `https://api.taskflow.com`
- Swagger: `https://api.taskflow.com/swagger` (if enabled)

---

## 🎉 Summary

The TaskFlow API Gateway provides **production-ready Swagger documentation** with:

✅ **All services documented** in one unified interface
✅ **Interactive testing** with built-in request execution
✅ **JWT authentication** integrated into UI
✅ **Custom branding** with TaskFlow theme
✅ **Export capabilities** (Postman, OpenAPI)
✅ **Comprehensive examples** for every endpoint
✅ **Real-time service status** monitoring

**Access it now**: http://localhost:5000/swagger 🚀

---

**Version**: 1.0
**Last Updated**: 2025-11-04
**Status**: ✅ Production Ready
