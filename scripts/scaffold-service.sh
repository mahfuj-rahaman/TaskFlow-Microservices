#!/bin/bash

################################################################################
# TaskFlow Service Scaffolding Script
#
# This script creates a complete microservice structure with all necessary
# boilerplate code according to Clean Architecture and DDD principles.
#
# Usage: ./scaffold-service.sh <ServiceName>
# Example: ./scaffold-service.sh Identity
#
# What this script does:
# 1. Creates service folder structure under src/Services/
# 2. Creates all 4 Clean Architecture layer projects (.csproj files)
# 3. Sets up proper project references to BuildingBlocks
# 4. Adds all projects to the solution (.sln)
# 5. Creates basic folder structure (no business logic)
# 6. Creates Program.cs with minimal configuration
# 7. Creates appsettings.json
# 8. Creates Dockerfile
#
# What this script DOES NOT do:
# - Implement any business logic
# - Create domain entities
# - Create controllers
# - Create database contexts
# - These will be added later based on feature specifications
################################################################################

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Function to print colored output
print_success() { echo -e "${GREEN}✓${NC} $1"; }
print_info() { echo -e "${CYAN}ℹ${NC} $1"; }
print_warning() { echo -e "${YELLOW}⚠${NC} $1"; }
print_error() { echo -e "${RED}✗${NC} $1"; }
print_section() { echo -e "\n${BLUE}══════════════════════════════════════════════════════════${NC}"; echo -e "${BLUE}▶${NC} $1"; echo -e "${BLUE}══════════════════════════════════════════════════════════${NC}"; }

# Validate arguments
if [ $# -ne 1 ]; then
    print_error "Usage: $0 <ServiceName>"
    print_info "Example: $0 Identity"
    print_info "Example: $0 Payment"
    exit 1
fi

SERVICE_NAME=$1
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
SERVICES_DIR="$PROJECT_ROOT/src/Services"
SERVICE_DIR="$SERVICES_DIR/$SERVICE_NAME"

# Banner
echo ""
echo "╔═══════════════════════════════════════════════════════════════╗"
echo "║         TaskFlow Service Scaffolding System                   ║"
echo "║         Create complete microservice boilerplate              ║"
echo "╚═══════════════════════════════════════════════════════════════╝"
echo ""

print_info "Service Name: $SERVICE_NAME"
print_info "Service Directory: $SERVICE_DIR"
print_info "Project Root: $PROJECT_ROOT"

# Check if service already exists
if [ -d "$SERVICE_DIR" ]; then
    print_warning "Service directory already exists: $SERVICE_DIR"
    read -p "Do you want to recreate it? This will DELETE the existing directory! (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        print_warning "Removing existing service directory..."
        rm -rf "$SERVICE_DIR"
        print_success "Removed existing directory"
    else
        print_error "Aborting. Service already exists."
        exit 1
    fi
fi

################################################################################
# Step 1: Create Directory Structure
################################################################################

print_section "Step 1: Creating Directory Structure"

mkdir -p "$SERVICE_DIR"
mkdir -p "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Domain"
mkdir -p "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Application"
mkdir -p "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Infrastructure"
mkdir -p "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.API"

# Create folder structure for Domain layer
mkdir -p "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Domain/Entities"
mkdir -p "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Domain/Events"
mkdir -p "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Domain/Exceptions"
mkdir -p "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Domain/Enums"

# Create folder structure for Application layer
mkdir -p "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Application/Features"
mkdir -p "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Application/Common/Interfaces"
mkdir -p "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Application/Common/Mappings"
mkdir -p "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Application/Common/Behaviors"

# Create folder structure for Infrastructure layer
mkdir -p "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Infrastructure/Persistence/Configurations"
mkdir -p "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Infrastructure/Persistence/Repositories"
mkdir -p "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Infrastructure/Services"

# Create folder structure for API layer
mkdir -p "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.API/Controllers"
mkdir -p "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.API/Middleware"

print_success "Directory structure created"

################################################################################
# Step 2: Create Project Files (.csproj)
################################################################################

print_section "Step 2: Creating Project Files"

# Domain Project
print_info "Creating TaskFlow.$SERVICE_NAME.Domain.csproj..."
cat > "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Domain/TaskFlow.$SERVICE_NAME.Domain.csproj" << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MediatR" Version="12.4.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\BuildingBlocks\TaskFlow.BuildingBlocks.Common\TaskFlow.BuildingBlocks.Common.csproj" />
  </ItemGroup>

</Project>
EOF
print_success "Created Domain project"

# Application Project
print_info "Creating TaskFlow.$SERVICE_NAME.Application.csproj..."
cat > "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Application/TaskFlow.$SERVICE_NAME.Application.csproj" << EOF
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="11.9.2" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.2" />
    <PackageReference Include="Mapster" Version="7.4.0" />
    <PackageReference Include="Mapster.DependencyInjection" Version="1.0.1" />
    <PackageReference Include="MediatR" Version="12.4.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\\TaskFlow.$SERVICE_NAME.Domain\\TaskFlow.$SERVICE_NAME.Domain.csproj" />
    <ProjectReference Include="..\\..\\..\\BuildingBlocks\\TaskFlow.BuildingBlocks.CQRS\\TaskFlow.BuildingBlocks.CQRS.csproj" />
  </ItemGroup>

</Project>
EOF
print_success "Created Application project"

# Infrastructure Project
print_info "Creating TaskFlow.$SERVICE_NAME.Infrastructure.csproj..."
cat > "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Infrastructure/TaskFlow.$SERVICE_NAME.Infrastructure.csproj" << EOF
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MassTransit" Version="8.2.5" />
    <PackageReference Include="MassTransit.RabbitMQ" Version="8.2.5" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.11">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.11" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.11" />
    <PackageReference Include="Polly" Version="8.4.2" />
    <PackageReference Include="StackExchange.Redis" Version="2.8.16" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\\TaskFlow.$SERVICE_NAME.Application\\TaskFlow.$SERVICE_NAME.Application.csproj" />
    <ProjectReference Include="..\\..\\..\\BuildingBlocks\\TaskFlow.BuildingBlocks.EventBus\\TaskFlow.BuildingBlocks.EventBus.csproj" />
    <ProjectReference Include="..\\..\\..\\BuildingBlocks\\TaskFlow.BuildingBlocks.Messaging\\TaskFlow.BuildingBlocks.Messaging.csproj" />
  </ItemGroup>

</Project>
EOF
print_success "Created Infrastructure project"

# API Project
print_info "Creating TaskFlow.$SERVICE_NAME.API.csproj..."
cat > "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.API/TaskFlow.$SERVICE_NAME.API.csproj" << EOF
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>
    <DockerfileContext>..\\..\\..\\..\\</DockerfileContext>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Asp.Versioning.Mvc.ApiExplorer" Version="8.1.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.10" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.21" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
    <PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
    <PackageReference Include="Serilog.Sinks.Seq" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.8.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\\TaskFlow.$SERVICE_NAME.Infrastructure\\TaskFlow.$SERVICE_NAME.Infrastructure.csproj" />
  </ItemGroup>

</Project>
EOF
print_success "Created API project"

################################################################################
# Step 3: Create Basic Program.cs
################################################################################

print_section "Step 3: Creating Program.cs"

cat > "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.API/Program.cs" << 'EOF'
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration["Seq:ServerUrl"] ?? "http://seq:5341")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Health Checks
builder.Services.AddHealthChecks();

// TODO: Add service-specific dependencies here
// - DbContext
// - Repositories
// - MediatR
// - FluentValidation
// - Mapster
// - Authentication/Authorization

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

try
{
    Log.Information("Starting application...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}
EOF
print_success "Created Program.cs"

################################################################################
# Step 4: Create appsettings.json
################################################################################

print_section "Step 4: Creating appsettings.json"

cat > "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.API/appsettings.json" << EOF
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=TaskFlow_${SERVICE_NAME};Username=postgres;Password=postgres"
  },
  "Seq": {
    "ServerUrl": "http://localhost:5341"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  },
  "Redis": {
    "Configuration": "localhost:6379"
  },
  "JwtSettings": {
    "Secret": "your-secret-key-min-32-characters-long",
    "Issuer": "TaskFlow",
    "Audience": "TaskFlow",
    "ExpirationInMinutes": 1440
  }
}
EOF
print_success "Created appsettings.json"

cat > "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.API/appsettings.Development.json" << 'EOF'
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  }
}
EOF
print_success "Created appsettings.Development.json"

################################################################################
# Step 5: Create Dockerfile
################################################################################

print_section "Step 5: Creating Dockerfile"

cat > "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.API/Dockerfile" << EOF
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy BuildingBlocks
COPY ["src/BuildingBlocks/TaskFlow.BuildingBlocks.Common/TaskFlow.BuildingBlocks.Common.csproj", "src/BuildingBlocks/TaskFlow.BuildingBlocks.Common/"]
COPY ["src/BuildingBlocks/TaskFlow.BuildingBlocks.CQRS/TaskFlow.BuildingBlocks.CQRS.csproj", "src/BuildingBlocks/TaskFlow.BuildingBlocks.CQRS/"]
COPY ["src/BuildingBlocks/TaskFlow.BuildingBlocks.EventBus/TaskFlow.BuildingBlocks.EventBus.csproj", "src/BuildingBlocks/TaskFlow.BuildingBlocks.EventBus/"]
COPY ["src/BuildingBlocks/TaskFlow.BuildingBlocks.Messaging/TaskFlow.BuildingBlocks.Messaging.csproj", "src/BuildingBlocks/TaskFlow.BuildingBlocks.Messaging/"]

# Copy service projects
COPY ["src/Services/$SERVICE_NAME/TaskFlow.$SERVICE_NAME.Domain/TaskFlow.$SERVICE_NAME.Domain.csproj", "src/Services/$SERVICE_NAME/TaskFlow.$SERVICE_NAME.Domain/"]
COPY ["src/Services/$SERVICE_NAME/TaskFlow.$SERVICE_NAME.Application/TaskFlow.$SERVICE_NAME.Application.csproj", "src/Services/$SERVICE_NAME/TaskFlow.$SERVICE_NAME.Application/"]
COPY ["src/Services/$SERVICE_NAME/TaskFlow.$SERVICE_NAME.Infrastructure/TaskFlow.$SERVICE_NAME.Infrastructure.csproj", "src/Services/$SERVICE_NAME/TaskFlow.$SERVICE_NAME.Infrastructure/"]
COPY ["src/Services/$SERVICE_NAME/TaskFlow.$SERVICE_NAME.API/TaskFlow.$SERVICE_NAME.API.csproj", "src/Services/$SERVICE_NAME/TaskFlow.$SERVICE_NAME.API/"]

# Restore dependencies
RUN dotnet restore "src/Services/$SERVICE_NAME/TaskFlow.$SERVICE_NAME.API/TaskFlow.$SERVICE_NAME.API.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/Services/$SERVICE_NAME/TaskFlow.$SERVICE_NAME.API"
RUN dotnet build "TaskFlow.$SERVICE_NAME.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskFlow.$SERVICE_NAME.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskFlow.$SERVICE_NAME.API.dll"]
EOF
print_success "Created Dockerfile"

################################################################################
# Step 6: Create .gitkeep files
################################################################################

print_section "Step 6: Creating .gitkeep files for empty directories"

touch "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Domain/Entities/.gitkeep"
touch "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Domain/Events/.gitkeep"
touch "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Domain/Exceptions/.gitkeep"
touch "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Domain/Enums/.gitkeep"
touch "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Application/Features/.gitkeep"
touch "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Application/Common/Interfaces/.gitkeep"
touch "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Application/Common/Mappings/.gitkeep"
touch "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Application/Common/Behaviors/.gitkeep"
touch "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Infrastructure/Persistence/Configurations/.gitkeep"
touch "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Infrastructure/Persistence/Repositories/.gitkeep"
touch "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.Infrastructure/Services/.gitkeep"
touch "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.API/Controllers/.gitkeep"
touch "$SERVICE_DIR/TaskFlow.$SERVICE_NAME.API/Middleware/.gitkeep"

print_success "Created .gitkeep files"

################################################################################
# Step 7: Add Projects to Solution
################################################################################

print_section "Step 7: Adding Projects to Solution"

cd "$PROJECT_ROOT"

# Check if solution file exists
SLN_FILE=$(find . -maxdepth 1 -name "*.sln" | head -1)

if [ -z "$SLN_FILE" ]; then
    print_error "No solution file found in project root"
    exit 1
fi

print_info "Found solution: $SLN_FILE"

# Add all 4 projects to solution
print_info "Adding Domain project to solution..."
dotnet sln "$SLN_FILE" add "src/Services/$SERVICE_NAME/TaskFlow.$SERVICE_NAME.Domain/TaskFlow.$SERVICE_NAME.Domain.csproj"

print_info "Adding Application project to solution..."
dotnet sln "$SLN_FILE" add "src/Services/$SERVICE_NAME/TaskFlow.$SERVICE_NAME.Application/TaskFlow.$SERVICE_NAME.Application.csproj"

print_info "Adding Infrastructure project to solution..."
dotnet sln "$SLN_FILE" add "src/Services/$SERVICE_NAME/TaskFlow.$SERVICE_NAME.Infrastructure/TaskFlow.$SERVICE_NAME.Infrastructure.csproj"

print_info "Adding API project to solution..."
dotnet sln "$SLN_FILE" add "src/Services/$SERVICE_NAME/TaskFlow.$SERVICE_NAME.API/TaskFlow.$SERVICE_NAME.API.csproj"

print_success "All projects added to solution"

################################################################################
# Step 8: Restore and Build
################################################################################

print_section "Step 8: Restoring and Building Projects"

print_info "Restoring NuGet packages..."
dotnet restore "$SLN_FILE"

print_info "Building solution..."
dotnet build "$SLN_FILE" --no-restore

print_success "Build completed successfully"

################################################################################
# Summary
################################################################################

print_section "✨ Service Scaffolding Complete!"

echo ""
print_success "Service '$SERVICE_NAME' has been created successfully!"
echo ""
print_info "What was created:"
echo "  ✓ src/Services/$SERVICE_NAME/"
echo "    ├── TaskFlow.$SERVICE_NAME.Domain (with folder structure)"
echo "    ├── TaskFlow.$SERVICE_NAME.Application (with folder structure)"
echo "    ├── TaskFlow.$SERVICE_NAME.Infrastructure (with folder structure)"
echo "    └── TaskFlow.$SERVICE_NAME.API (with Program.cs, appsettings, Dockerfile)"
echo ""
print_info "All 4 projects have been added to the solution"
echo ""
print_warning "Next Steps:"
echo "  1. Review the feature specification: docs/features/<FeatureName>_feature.md"
echo "  2. Use ai-scaffold.sh or generate-from-spec.sh to generate business logic"
echo "  3. Implement domain entities in Domain/Entities/"
echo "  4. Implement use cases in Application/Features/"
echo "  5. Configure DbContext in Infrastructure/Persistence/"
echo "  6. Add controllers in API/Controllers/"
echo "  7. Run migrations: dotnet ef migrations add InitialCreate"
echo ""
print_success "Ready to start coding! 🚀"
echo ""
