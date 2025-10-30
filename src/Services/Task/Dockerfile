# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["src/Services/Task/TaskFlow.Task.API/TaskFlow.Task.API.csproj", "Services/Task/TaskFlow.Task.API/"]
COPY ["src/Services/Task/TaskFlow.Task.Application/TaskFlow.Task.Application.csproj", "Services/Task/TaskFlow.Task.Application/"]
COPY ["src/Services/Task/TaskFlow.Task.Domain/TaskFlow.Task.Domain.csproj", "Services/Task/TaskFlow.Task.Domain/"]
COPY ["src/Services/Task/TaskFlow.Task.Infrastructure/TaskFlow.Task.Infrastructure.csproj", "Services/Task/TaskFlow.Task.Infrastructure/"]
COPY ["src/BuildingBlocks/TaskFlow.BuildingBlocks.Common/TaskFlow.BuildingBlocks.Common.csproj", "BuildingBlocks/TaskFlow.BuildingBlocks.Common/"]

# Restore dependencies
RUN dotnet restore "Services/Task/TaskFlow.Task.API/TaskFlow.Task.API.csproj"

# Copy all source code
COPY src/Services/Task/ Services/Task/
COPY src/BuildingBlocks/ BuildingBlocks/

# Build the application
WORKDIR "/src/Services/Task/TaskFlow.Task.API"
RUN dotnet build "TaskFlow.Task.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "TaskFlow.Task.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application
COPY --from=publish /app/publish .

# Change ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080
EXPOSE 8081

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "TaskFlow.Task.API.dll"]
