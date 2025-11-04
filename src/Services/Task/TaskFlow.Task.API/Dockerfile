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
COPY ["src/Services/Task/TaskFlow.Task.Domain/TaskFlow.Task.Domain.csproj", "src/Services/Task/TaskFlow.Task.Domain/"]
COPY ["src/Services/Task/TaskFlow.Task.Application/TaskFlow.Task.Application.csproj", "src/Services/Task/TaskFlow.Task.Application/"]
COPY ["src/Services/Task/TaskFlow.Task.Infrastructure/TaskFlow.Task.Infrastructure.csproj", "src/Services/Task/TaskFlow.Task.Infrastructure/"]
COPY ["src/Services/Task/TaskFlow.Task.API/TaskFlow.Task.API.csproj", "src/Services/Task/TaskFlow.Task.API/"]

# Restore dependencies
ENV NUGET_PACKAGES_DOWNLOAD_TIMEOUT=300
RUN dotnet restore "src/Services/Task/TaskFlow.Task.API/TaskFlow.Task.API.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/Services/Task/TaskFlow.Task.API"
RUN dotnet build "TaskFlow.Task.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskFlow.Task.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskFlow.Task.API.dll"]
