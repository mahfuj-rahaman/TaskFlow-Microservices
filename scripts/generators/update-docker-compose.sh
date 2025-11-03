#!/bin/bash

###############################################################################
# Docker Compose Auto-Configuration Script
###############################################################################
# Automatically adds microservice to docker-compose.yml with:
# - Auto-scaling configuration (deploy.replicas)
# - Health checks
# - Service dependencies
# - Environment variables
# - Network configuration
# - Consul service discovery registration
###############################################################################

set -e

# Color codes
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

SERVICE_NAME=$1

if [[ -z "$SERVICE_NAME" ]]; then
    echo -e "${YELLOW}[DOCKER]${NC} Usage: $0 <ServiceName>"
    exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$(dirname "$SCRIPT_DIR")")"
COMPOSE_FILE="$PROJECT_ROOT/docker-compose.yml"

SERVICE_NAME_LOWER=$(echo "$SERVICE_NAME" | tr '[:upper:]' '[:lower:]')

echo -e "${BLUE}[DOCKER]${NC} Updating docker-compose.yml for $SERVICE_NAME..."

# Check if service already exists
if grep -q "${SERVICE_NAME_LOWER}-service:" "$COMPOSE_FILE"; then
    echo -e "${YELLOW}[DOCKER]${NC} Service already exists in docker-compose.yml"
    exit 0
fi

# Create backup
cp "$COMPOSE_FILE" "${COMPOSE_FILE}.bak"

# Add service configuration
add_service() {
    echo -e "${BLUE}[DOCKER]${NC} Adding service definition..."

    # Find the last service definition
    local insert_line=$(grep -n "^  [a-z-]*-service:" "$COMPOSE_FILE" | tail -1 | cut -d: -f1)

    if [[ -z "$insert_line" ]]; then
        # No services found, add after consul
        insert_line=$(grep -n "^  consul:" "$COMPOSE_FILE" | cut -d: -f1)
    fi

    # Prepare service definition
    cat > /tmp/service-def.yml <<EOF

  # ${SERVICE_NAME} Service
  ${SERVICE_NAME_LOWER}-service:
    build:
      context: .
      dockerfile: src/Services/${SERVICE_NAME}/Dockerfile
    container_name: taskflow-${SERVICE_NAME_LOWER}-service
    restart: unless-stopped
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy
      consul:
        condition: service_started
    environment:
      - ASPNETCORE_ENVIRONMENT=\${ENVIRONMENT:-Production}
      - ConnectionStrings__${SERVICE_NAME}Db=Host=postgres;Port=5432;Database=taskflow_${SERVICE_NAME_LOWER};Username=\${POSTGRES_USER:-postgres};Password=\${POSTGRES_PASSWORD:-postgres}
      - DatabaseProvider=PostgreSQL
      - Redis__ConnectionString=redis:6379,password=\${REDIS_PASSWORD:-redis123}
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Username=\${RABBITMQ_USER:-rabbitmq}
      - RabbitMQ__Password=\${RABBITMQ_PASSWORD:-rabbitmq}
      - Consul__Host=http://consul:8500
      - Consul__ServiceName=${SERVICE_NAME_LOWER}-service
      - Consul__ServicePort=80
    networks:
      - taskflow-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
    deploy:
      mode: replicated
      replicas: 2  # Start with 2 replicas
      resources:
        limits:
          cpus: '0.5'
          memory: 512M
        reservations:
          cpus: '0.25'
          memory: 256M
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3
      update_config:
        parallelism: 1
        delay: 10s
        failure_action: rollback
        order: start-first
    labels:
      - "com.taskflow.service=${SERVICE_NAME_LOWER}"
      - "com.taskflow.autoscale=true"
      - "com.taskflow.min-replicas=1"
      - "com.taskflow.max-replicas=10"
EOF

    # Insert service definition
    if [[ -n "$insert_line" ]]; then
        # Get all content after the insert point
        tail -n +"$((insert_line + 1))" "$COMPOSE_FILE" > /tmp/compose-tail.yml

        # Get all content before and including the insert point
        head -n "$insert_line" "$COMPOSE_FILE" > /tmp/compose-head.yml

        # Combine: head + new service + tail
        cat /tmp/compose-head.yml /tmp/service-def.yml /tmp/compose-tail.yml > "$COMPOSE_FILE"
    else
        # Append to end
        cat /tmp/service-def.yml >> "$COMPOSE_FILE"
    fi

    rm -f /tmp/service-def.yml /tmp/compose-head.yml /tmp/compose-tail.yml

    echo -e "${GREEN}[DOCKER]${NC} Service definition added"
}

# Update init database script
update_init_db_script() {
    echo -e "${BLUE}[DOCKER]${NC} Updating database initialization script..."

    local init_script="$PROJECT_ROOT/scripts/init-databases.sql"

    if [[ ! -f "$init_script" ]]; then
        cat > "$init_script" <<EOF
-- TaskFlow Microservices Database Initialization
-- This script creates databases for all microservices

-- Set default user (postgres)
\\c postgres

EOF
    fi

    # Check if database already created
    if grep -q "taskflow_${SERVICE_NAME_LOWER}" "$init_script"; then
        echo -e "${YELLOW}[DOCKER]${NC} Database already exists in init script"
        return
    fi

    # Add database creation
    cat >> "$init_script" <<EOF

-- Create ${SERVICE_NAME} Database
CREATE DATABASE taskflow_${SERVICE_NAME_LOWER};
GRANT ALL PRIVILEGES ON DATABASE taskflow_${SERVICE_NAME_LOWER} TO postgres;

EOF

    echo -e "${GREEN}[DOCKER]${NC} Database initialization script updated"
}

# Create Dockerfile for service
create_dockerfile() {
    echo -e "${BLUE}[DOCKER]${NC} Creating Dockerfile..."

    local service_dir="$PROJECT_ROOT/src/Services/$SERVICE_NAME"
    local dockerfile="$service_dir/Dockerfile"

    if [[ -f "$dockerfile" ]]; then
        echo -e "${YELLOW}[DOCKER]${NC} Dockerfile already exists"
        return
    fi

    cat > "$dockerfile" <<'EOF'
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ["TaskFlow.sln", "./"]

# Copy project files
COPY ["src/BuildingBlocks/", "src/BuildingBlocks/"]
COPY ["src/Services/${SERVICE_NAME}/", "src/Services/${SERVICE_NAME}/"]

# Restore dependencies
RUN dotnet restore "src/Services/${SERVICE_NAME}/TaskFlow.${SERVICE_NAME}.API/TaskFlow.${SERVICE_NAME}.API.csproj"

# Build
WORKDIR "/src/src/Services/${SERVICE_NAME}/TaskFlow.${SERVICE_NAME}.API"
RUN dotnet build "TaskFlow.${SERVICE_NAME}.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "TaskFlow.${SERVICE_NAME}.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Create non-root user
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 80
EXPOSE 443

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "TaskFlow.${SERVICE_NAME}.API.dll"]
EOF

    # Replace ${SERVICE_NAME} placeholder
    sed -i "s/\${SERVICE_NAME}/$SERVICE_NAME/g" "$dockerfile"

    echo -e "${GREEN}[DOCKER]${NC} Dockerfile created"
}

# Create docker-compose scaling configuration
create_scaling_config() {
    echo -e "${BLUE}[DOCKER]${NC} Creating scaling configuration..."

    local scaling_file="$PROJECT_ROOT/docker/scaling.yml"
    mkdir -p "$PROJECT_ROOT/docker"

    if [[ ! -f "$scaling_file" ]]; then
        cat > "$scaling_file" <<EOF
# Docker Compose Scaling Configuration
# Usage: docker-compose -f docker-compose.yml -f docker/scaling.yml up -d
#
# This file contains scaling configurations for production environments
# Adjust replicas based on load requirements

version: '3.8'

services:
EOF
    fi

    # Check if service already has scaling config
    if grep -q "${SERVICE_NAME_LOWER}-service:" "$scaling_file"; then
        echo -e "${YELLOW}[DOCKER]${NC} Scaling config already exists"
        return
    fi

    cat >> "$scaling_file" <<EOF

  ${SERVICE_NAME_LOWER}-service:
    deploy:
      replicas: 3
      resources:
        limits:
          cpus: '1.0'
          memory: 1G
        reservations:
          cpus: '0.5'
          memory: 512M
EOF

    echo -e "${GREEN}[DOCKER]${NC} Scaling configuration created"
}

# Main execution
main() {
    add_service
    update_init_db_script
    create_dockerfile
    create_scaling_config

    echo -e "${GREEN}[DOCKER]${NC} Docker configuration updated successfully!"
    echo -e "${BLUE}[DOCKER]${NC} "
    echo -e "${BLUE}[DOCKER]${NC} Commands:"
    echo -e "${BLUE}[DOCKER]${NC}   - Build: docker-compose build ${SERVICE_NAME_LOWER}-service"
    echo -e "${BLUE}[DOCKER]${NC}   - Start: docker-compose up -d ${SERVICE_NAME_LOWER}-service"
    echo -e "${BLUE}[DOCKER]${NC}   - Scale: docker-compose up -d --scale ${SERVICE_NAME_LOWER}-service=5"
    echo -e "${BLUE}[DOCKER]${NC}   - Logs: docker-compose logs -f ${SERVICE_NAME_LOWER}-service"
}

main
