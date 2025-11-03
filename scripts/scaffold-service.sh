#!/bin/bash

###############################################################################
# TaskFlow Microservices - Master Service Scaffolding Script
###############################################################################
# Purpose: Generate complete production-ready microservice from feature.md
# Usage: ./scripts/scaffold-service.sh <feature_file.md>
# Example: ./scripts/scaffold-service.sh docs/features/identity_feature.md
#
# What this script does:
# 1. Parses feature.md specification
# 2. Generates complete Clean Architecture solution:
#    - Domain Layer (Entities, Events, Exceptions, Enums)
#    - Application Layer (Commands, Queries, DTOs, Handlers, Validators, Interfaces)
#    - Infrastructure Layer (Repositories, EF Configurations, Services)
#    - API Layer (Controllers, Program.cs, appsettings.json)
#    - Tests (Unit Tests, Integration Tests)
# 3. Auto-registers service in API Gateway
# 4. Generates Docker configuration with auto-scaling support
# 5. Updates docker-compose.yml
# 6. Creates database migration scripts
# 7. Generates Consul service discovery config
# 8. Creates health check endpoints
###############################################################################

set -e  # Exit on any error

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
MAGENTA='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Import common functions
source "$SCRIPT_DIR/common-functions.sh" 2>/dev/null || true

###############################################################################
# Helper Functions
###############################################################################

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_section() {
    echo ""
    echo -e "${MAGENTA}╔════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${MAGENTA}║${NC} ${CYAN}$1${NC}"
    echo -e "${MAGENTA}╚════════════════════════════════════════════════════════════╝${NC}"
    echo ""
}

# Parse feature.md file
parse_feature_spec() {
    local feature_file=$1

    if [[ ! -f "$feature_file" ]]; then
        log_error "Feature specification file not found: $feature_file"
        exit 1
    fi

    log_info "Parsing feature specification: $feature_file"

    # Extract feature name and service name from the file
    FEATURE_NAME=$(grep -m 1 "^# " "$feature_file" | sed 's/# \(.*\) Feature Specification/\1/' || echo "")
    SERVICE_NAME=$(grep -m 1 "^\*\*Service\*\*:" "$feature_file" | sed 's/\*\*Service\*\*: //' || echo "")

    # If not found in headers, try JSON format
    if [[ -z "$FEATURE_NAME" || -z "$SERVICE_NAME" ]]; then
        # Check if there's a corresponding JSON file
        local json_file="${feature_file%.md}_data.json"
        json_file="${json_file//_feature/}"

        if [[ -f "$json_file" ]]; then
            log_info "Found corresponding JSON specification: $json_file"
            FEATURE_NAME=$(jq -r '.featureName // .feature // ""' "$json_file")
            SERVICE_NAME=$(jq -r '.serviceName // .service // ""' "$json_file")
        fi
    fi

    if [[ -z "$FEATURE_NAME" || -z "$SERVICE_NAME" ]]; then
        log_error "Could not extract feature name or service name from specification"
        log_error "Feature name: $FEATURE_NAME"
        log_error "Service name: $SERVICE_NAME"
        exit 1
    fi

    log_success "Feature: $FEATURE_NAME"
    log_success "Service: $SERVICE_NAME"
}

# Check prerequisites
check_prerequisites() {
    log_section "Checking Prerequisites"

    local missing=0

    # Check for required tools
    if ! command -v dotnet &> /dev/null; then
        log_error ".NET SDK not found. Please install .NET 8.0 SDK"
        missing=1
    else
        log_success ".NET SDK found: $(dotnet --version)"
    fi

    if ! command -v jq &> /dev/null; then
        log_warning "jq not found. JSON parsing may be limited"
    else
        log_success "jq found"
    fi

    if ! command -v docker &> /dev/null; then
        log_warning "Docker not found. Container images cannot be built"
    else
        log_success "Docker found"
    fi

    # Check for BuildingBlocks
    if [[ ! -d "$PROJECT_ROOT/src/BuildingBlocks" ]]; then
        log_error "BuildingBlocks directory not found"
        missing=1
    else
        log_success "BuildingBlocks found"
    fi

    if [[ $missing -eq 1 ]]; then
        exit 1
    fi
}

# Create solution structure
create_solution_structure() {
    log_section "Creating Solution Structure"

    local service_dir="$PROJECT_ROOT/src/Services/$SERVICE_NAME"

    # Create main directories
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.Domain"
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.Application"
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.Infrastructure"
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.API"

    # Create subdirectories for Domain layer
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.Domain/Entities"
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.Domain/Events"
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.Domain/Exceptions"
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.Domain/Enums"
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.Domain/ValueObjects"

    # Create subdirectories for Application layer
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.Application/Features/$FEATURE_NAME/Commands"
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.Application/Features/$FEATURE_NAME/Queries"
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.Application/Features/$FEATURE_NAME/DTOs"
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.Application/Common/Interfaces"
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.Application/Common/Mappings"
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.Application/Common/Behaviors"

    # Create subdirectories for Infrastructure layer
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.Infrastructure/Persistence/Configurations"
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.Infrastructure/Persistence/Repositories"
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.Infrastructure/Persistence/Migrations"
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.Infrastructure/Services"

    # Create subdirectories for API layer
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.API/Controllers"
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.API/Middleware"
    mkdir -p "$service_dir/TaskFlow.$SERVICE_NAME.API/Filters"

    # Create test directories
    mkdir -p "$PROJECT_ROOT/tests/TaskFlow.$SERVICE_NAME.UnitTests/Domain"
    mkdir -p "$PROJECT_ROOT/tests/TaskFlow.$SERVICE_NAME.UnitTests/Application"
    mkdir -p "$PROJECT_ROOT/tests/TaskFlow.$SERVICE_NAME.IntegrationTests/Controllers"
    mkdir -p "$PROJECT_ROOT/tests/TaskFlow.$SERVICE_NAME.IntegrationTests/Infrastructure"

    log_success "Solution structure created"
}

# Generate csproj files
generate_csproj_files() {
    log_section "Generating .csproj Files"

    local service_dir="$PROJECT_ROOT/src/Services/$SERVICE_NAME"

    # Domain project
    cat > "$service_dir/TaskFlow.$SERVICE_NAME.Domain/TaskFlow.$SERVICE_NAME.Domain.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="../../../BuildingBlocks/TaskFlow.BuildingBlocks.Common/TaskFlow.BuildingBlocks.Common.csproj" />
  </ItemGroup>

</Project>
EOF

    # Application project
    cat > "$service_dir/TaskFlow.$SERVICE_NAME.Application/TaskFlow.$SERVICE_NAME.Application.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="11.9.0" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.0" />
    <PackageReference Include="Mapster" Version="7.4.0" />
    <PackageReference Include="MediatR" Version="12.2.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../TaskFlow.$SERVICE_NAME.Domain/TaskFlow.$SERVICE_NAME.Domain.csproj" />
    <ProjectReference Include="../../../BuildingBlocks/TaskFlow.BuildingBlocks.Common/TaskFlow.BuildingBlocks.Common.csproj" />
    <ProjectReference Include="../../../BuildingBlocks/TaskFlow.BuildingBlocks.CQRS/TaskFlow.BuildingBlocks.CQRS.csproj" />
    <ProjectReference Include="../../../BuildingBlocks/TaskFlow.BuildingBlocks.Caching/TaskFlow.BuildingBlocks.Caching.csproj" />
  </ItemGroup>

</Project>
EOF

    # Infrastructure project
    cat > "$service_dir/TaskFlow.$SERVICE_NAME.Infrastructure/TaskFlow.$SERVICE_NAME.Infrastructure.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
    <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../TaskFlow.$SERVICE_NAME.Application/TaskFlow.$SERVICE_NAME.Application.csproj" />
    <ProjectReference Include="../../../BuildingBlocks/TaskFlow.BuildingBlocks.Common/TaskFlow.BuildingBlocks.Common.csproj" />
    <ProjectReference Include="../../../BuildingBlocks/TaskFlow.BuildingBlocks.EventBus/TaskFlow.BuildingBlocks.EventBus.csproj" />
    <ProjectReference Include="../../../BuildingBlocks/TaskFlow.BuildingBlocks.Messaging/TaskFlow.BuildingBlocks.Messaging.csproj" />
  </ItemGroup>

</Project>
EOF

    # API project
    cat > "$service_dir/TaskFlow.$SERVICE_NAME.API/TaskFlow.$SERVICE_NAME.API.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
    <PackageReference Include="Serilog.Sinks.Seq" Version="7.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="Consul" Version="1.7.14.1" />
    <PackageReference Include="StackExchange.Redis" Version="2.7.10" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../TaskFlow.$SERVICE_NAME.Infrastructure/TaskFlow.$SERVICE_NAME.Infrastructure.csproj" />
  </ItemGroup>

</Project>
EOF

    log_success "Project files generated"
}

# Call all generation steps
main() {
    log_section "TaskFlow Service Scaffolding"

    if [[ $# -lt 1 ]]; then
        log_error "Usage: $0 <feature_file.md>"
        log_error "Example: $0 docs/features/identity_feature.md"
        exit 1
    fi

    local feature_file="$1"

    check_prerequisites
    parse_feature_spec "$feature_file"
    create_solution_structure
    generate_csproj_files

    # Call modular generators
    "$SCRIPT_DIR/generators/generate-domain.sh" "$FEATURE_NAME" "$SERVICE_NAME" "$feature_file"
    "$SCRIPT_DIR/generators/generate-application.sh" "$FEATURE_NAME" "$SERVICE_NAME" "$feature_file"
    "$SCRIPT_DIR/generators/generate-infrastructure.sh" "$FEATURE_NAME" "$SERVICE_NAME" "$feature_file"
    "$SCRIPT_DIR/generators/generate-api.sh" "$FEATURE_NAME" "$SERVICE_NAME" "$feature_file"
    "$SCRIPT_DIR/generators/generate-docker.sh" "$FEATURE_NAME" "$SERVICE_NAME"
    "$SCRIPT_DIR/generators/generate-tests.sh" "$FEATURE_NAME" "$SERVICE_NAME" "$feature_file"

    # Update API Gateway
    "$SCRIPT_DIR/generators/update-api-gateway.sh" "$SERVICE_NAME"

    # Update docker-compose
    "$SCRIPT_DIR/generators/update-docker-compose.sh" "$SERVICE_NAME"

    log_section "Scaffolding Complete!"
    log_success "Service: $SERVICE_NAME"
    log_success "Feature: $FEATURE_NAME"
    log_info ""
    log_info "Next steps:"
    log_info "1. Review generated code in: src/Services/$SERVICE_NAME"
    log_info "2. Customize business logic as needed"
    log_info "3. Run: dotnet build"
    log_info "4. Run: dotnet test"
    log_info "5. Run: docker-compose up -d"
    log_info ""
    log_info "API Gateway automatically configured for service discovery"
    log_info "Auto-scaling configured in docker-compose.yml"
}

main "$@"
