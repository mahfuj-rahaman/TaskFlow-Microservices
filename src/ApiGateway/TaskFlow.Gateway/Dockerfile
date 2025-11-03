# =============================================================================
# Multi-stage Dockerfile for TaskFlow API Gateway
# =============================================================================
# Stage 1: Build
# Stage 2: Publish
# Stage 3: Runtime (optimized for production)
# =============================================================================

# -----------------------------------------------------------------------------
# Stage 1: Build
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ["TaskFlow.sln", "./"]

# Copy all project files (for dependency resolution)
COPY ["src/ApiGateway/TaskFlow.Gateway/TaskFlow.Gateway.csproj", "src/ApiGateway/TaskFlow.Gateway/"]
COPY ["src/BuildingBlocks/TaskFlow.BuildingBlocks.Domain/TaskFlow.BuildingBlocks.Domain.csproj", "src/BuildingBlocks/TaskFlow.BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/TaskFlow.BuildingBlocks.Application/TaskFlow.BuildingBlocks.Application.csproj", "src/BuildingBlocks/TaskFlow.BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/TaskFlow.BuildingBlocks.Infrastructure/TaskFlow.BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/TaskFlow.BuildingBlocks.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/ApiGateway/TaskFlow.Gateway/TaskFlow.Gateway.csproj"

# Copy all source code
COPY . .

# Build the project
WORKDIR "/src/src/ApiGateway/TaskFlow.Gateway"
RUN dotnet build "TaskFlow.Gateway.csproj" -c Release -o /app/build --no-restore

# -----------------------------------------------------------------------------
# Stage 2: Publish
# -----------------------------------------------------------------------------
FROM build AS publish
RUN dotnet publish "TaskFlow.Gateway.csproj" -c Release -o /app/publish --no-build --no-restore /p:UseAppHost=false

# -----------------------------------------------------------------------------
# Stage 3: Runtime
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r taskflow && useradd -r -g taskflow taskflow

# Copy published app
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R taskflow:taskflow /app

# Switch to non-root user
USER taskflow

# Expose port
EXPOSE 8080
EXPOSE 8081

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080;https://+:8081
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "TaskFlow.Gateway.dll"]
