# EventBus NuGet Packages Guide

## 📦 BuildingBlock Packages (Already Installed)

The EventBus BuildingBlock includes ALL database adapters, so these packages are already installed:

```xml
<ItemGroup>
  <!-- Core Dependencies -->
  <PackageReference Include="MassTransit" Version="8.2.5" />
  <PackageReference Include="MediatR" Version="12.4.1" />
  <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.2" />
  <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.2" />
  <PackageReference Include="Microsoft.Extensions.Hosting.Abstractions" Version="8.0.1" />

  <!-- EF Core for EfCoreEventStore and SqlEventStore -->
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="8.0.11" />

  <!-- MongoDB for MongoDbEventStore -->
  <PackageReference Include="MongoDB.Driver" Version="2.30.0" />

  <!-- Cassandra for CassandraEventStore -->
  <PackageReference Include="CassandraCSharpDriver" Version="3.21.0" />

  <!-- Redis for RedisEventStore -->
  <PackageReference Include="StackExchange.Redis" Version="2.8.16" />
</ItemGroup>
```

**✅ All packages use .NET 8.x compatible versions**

---

## 🎯 Service-Level Packages (What YOU Need to Install)

When you create a service, you only need to install packages for **YOUR database**:

### PostgreSQL Service

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.11
```

```csharp
// Program.cs
builder.Services.AddDbContext<YourDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddPersistentEventBusWithMediatRAndEfCore(EventBusMode.Hybrid);
```

### SQL Server Service

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.11
```

```csharp
// Program.cs
builder.Services.AddDbContext<YourDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddPersistentEventBusWithMediatRAndEfCore(EventBusMode.Hybrid);
```

### MySQL Service

```bash
dotnet add package Pomelo.EntityFrameworkCore.MySql --version 8.0.2
```

```csharp
// Program.cs
builder.Services.AddDbContext<YourDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddPersistentEventBusWithMediatRAndEfCore(EventBusMode.Hybrid);
```

### MongoDB Service

```bash
# No additional package needed! MongoDB.Driver is in BuildingBlock
```

```csharp
// Program.cs
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient("mongodb://localhost:27017"));

builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("your_db"));

builder.Services.AddPersistentEventBusWithMediatRAndMongoDB(EventBusMode.Hybrid);
```

### Cassandra Service

```bash
# No additional package needed! CassandraCSharpDriver is in BuildingBlock
```

```csharp
// Program.cs
builder.Services.AddSingleton<ICluster>(sp =>
    Cluster.Builder()
        .AddContactPoint("localhost")
        .WithPort(9042)
        .Build());

builder.Services.AddSingleton<ISession>(sp =>
    sp.GetRequiredService<ICluster>().Connect("your_keyspace"));

builder.Services.AddPersistentEventBusWithMediatRAndCassandra(EventBusMode.Hybrid);
```

### Redis Service

```bash
# No additional package needed! StackExchange.Redis is in BuildingBlock
```

```csharp
// Program.cs
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect("localhost:6379"));

builder.Services.AddPersistentEventBusWithMediatRAndRedis(EventBusMode.Hybrid);
```

---

## 📊 Package Matrix

| Database | Service Must Install | Already in BuildingBlock |
|----------|---------------------|--------------------------|
| **PostgreSQL** | `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.11 | ✅ EF Core 8.0.11 |
| **SQL Server** | `Microsoft.EntityFrameworkCore.SqlServer` 8.0.11 | ✅ EF Core 8.0.11 |
| **MySQL** | `Pomelo.EntityFrameworkCore.MySql` 8.0.2 | ✅ EF Core 8.0.11 |
| **SQLite** | `Microsoft.EntityFrameworkCore.Sqlite` 8.0.11 | ✅ EF Core 8.0.11 |
| **MongoDB** | ❌ None | ✅ MongoDB.Driver 2.30.0 |
| **Cassandra** | ❌ None | ✅ CassandraCSharpDriver 3.21.0 |
| **Redis** | ❌ None | ✅ StackExchange.Redis 2.8.16 |

---

## 🚀 Complete Service Setup Examples

### Example 1: Identity Service (PostgreSQL)

```bash
# Create project
dotnet new webapi -n TaskFlow.Identity.API

# Add BuildingBlocks reference
dotnet add reference ../../../BuildingBlocks/TaskFlow.BuildingBlocks.EventBus

# Add ONLY PostgreSQL provider
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.11
dotnet add package MediatR --version 12.4.1
```

**Program.cs**:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add DbContext with PostgreSQL
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("IdentityDb")));

// Add MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add EventBus with Hybrid mode
builder.Services.AddPersistentEventBusWithMediatRAndEfCore(EventBusMode.Hybrid);
```

**Packages Installed**:
- ✅ Npgsql.EntityFrameworkCore.PostgreSQL (your service)
- ✅ MediatR (your service)
- ✅ All other packages come from BuildingBlock reference

---

### Example 2: Analytics Service (MongoDB)

```bash
# Create project
dotnet new webapi -n TaskFlow.Analytics.API

# Add BuildingBlocks reference
dotnet add reference ../../../BuildingBlocks/TaskFlow.BuildingBlocks.EventBus

# Add ONLY MediatR (MongoDB.Driver already in BuildingBlock!)
dotnet add package MediatR --version 12.4.1
```

**Program.cs**:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(builder.Configuration.GetConnectionString("MongoDb")));

builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("analytics"));

// Add MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add EventBus with Hybrid mode
builder.Services.AddPersistentEventBusWithMediatRAndMongoDB(EventBusMode.Hybrid);
```

**Packages Installed**:
- ✅ MediatR (your service)
- ✅ MongoDB.Driver comes from BuildingBlock reference!

---

### Example 3: IoT Service (Cassandra)

```bash
# Create project
dotnet new webapi -n TaskFlow.IoT.API

# Add BuildingBlocks reference
dotnet add reference ../../../BuildingBlocks/TaskFlow.BuildingBlocks.EventBus

# Add ONLY MediatR (CassandraCSharpDriver already in BuildingBlock!)
dotnet add package MediatR --version 12.4.1
```

**Program.cs**:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Cassandra cluster
builder.Services.AddSingleton<ICluster>(sp =>
    Cluster.Builder()
        .AddContactPoint(builder.Configuration["Cassandra:ContactPoint"])
        .WithPort(9042)
        .Build());

builder.Services.AddSingleton<ISession>(sp =>
    sp.GetRequiredService<ICluster>().Connect("iot_keyspace"));

// Add MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add EventBus with Hybrid mode
builder.Services.AddPersistentEventBusWithMediatRAndCassandra(EventBusMode.Hybrid);
```

**Packages Installed**:
- ✅ MediatR (your service)
- ✅ CassandraCSharpDriver comes from BuildingBlock reference!

---

## 🎯 Key Takeaways

### BuildingBlock Has Everything
✅ All database drivers included in BuildingBlock
✅ No need to install MongoDB.Driver, CassandraCSharpDriver, or StackExchange.Redis in your service
✅ EF Core base packages included

### Services Only Install
1. **Database Provider** (only for SQL databases):
   - PostgreSQL: `Npgsql.EntityFrameworkCore.PostgreSQL`
   - SQL Server: `Microsoft.EntityFrameworkCore.SqlServer`
   - MySQL: `Pomelo.EntityFrameworkCore.MySql`

2. **MediatR** (if using MediatR for in-process events):
   - `MediatR` 12.4.1

### Benefits
✅ **Minimal packages in services** - only what you need
✅ **Shared drivers** - MongoDB/Cassandra/Redis drivers shared via BuildingBlock
✅ **Version consistency** - all .NET 8 compatible
✅ **Easy upgrades** - update BuildingBlock, all services get new versions

---

## 🔧 Troubleshooting

### Error: "Type 'MongoClient' not found"

**Cause**: You're referencing the EventBus BuildingBlock, so MongoClient is available!

**Solution**: Just use it, no need to install MongoDB.Driver:
```csharp
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient("mongodb://localhost:27017"));
```

### Error: "Type 'ICluster' not found"

**Cause**: You're referencing the EventBus BuildingBlock, so Cassandra types are available!

**Solution**: Add using statement:
```csharp
using Cassandra;

builder.Services.AddSingleton<ICluster>(sp =>
    Cluster.Builder().AddContactPoint("localhost").Build());
```

### Error: "Type 'IConnectionMultiplexer' not found"

**Cause**: You're referencing the EventBus BuildingBlock, so Redis types are available!

**Solution**: Add using statement:
```csharp
using StackExchange.Redis;

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect("localhost:6379"));
```

---

## 📚 Version Compatibility

All packages are tested with **.NET 8.0**:

| Package | Version | .NET Compatibility |
|---------|---------|-------------------|
| Microsoft.EntityFrameworkCore | 8.0.11 | .NET 8.0 |
| Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.11 | .NET 8.0 |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.11 | .NET 8.0 |
| Pomelo.EntityFrameworkCore.MySql | 8.0.2 | .NET 8.0 |
| MongoDB.Driver | 2.30.0 | .NET 6.0+ |
| CassandraCSharpDriver | 3.21.0 | .NET 6.0+ |
| StackExchange.Redis | 2.8.16 | .NET 6.0+ |
| MediatR | 12.4.1 | .NET 6.0+ |
| MassTransit | 8.2.5 | .NET 6.0+ |

✅ **All packages are compatible with .NET 8.0**

---

## 🎉 Summary

**BuildingBlock Installation** (done once):
```bash
# Already installed! Nothing to do.
```

**Service Installation** (per service):
```bash
# For SQL databases only:
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.11
# OR
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.11
# OR
dotnet add package Pomelo.EntityFrameworkCore.MySql --version 8.0.2

# For all services:
dotnet add package MediatR --version 12.4.1
```

**For NoSQL databases (MongoDB, Cassandra, Redis)**:
```bash
# Nothing! Drivers are in BuildingBlock! Just reference it.
```

That's it! 🎉
