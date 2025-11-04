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
COPY ["src/Services/Notif/TaskFlow.Notif.Domain/TaskFlow.Notif.Domain.csproj", "src/Services/Notif/TaskFlow.Notif.Domain/"]
COPY ["src/Services/Notif/TaskFlow.Notif.Application/TaskFlow.Notif.Application.csproj", "src/Services/Notif/TaskFlow.Notif.Application/"]
COPY ["src/Services/Notif/TaskFlow.Notif.Infrastructure/TaskFlow.Notif.Infrastructure.csproj", "src/Services/Notif/TaskFlow.Notif.Infrastructure/"]
COPY ["src/Services/Notif/TaskFlow.Notif.API/TaskFlow.Notif.API.csproj", "src/Services/Notif/TaskFlow.Notif.API/"]

# Restore dependencies
RUN dotnet restore "src/Services/Notif/TaskFlow.Notif.API/TaskFlow.Notif.API.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/Services/Notif/TaskFlow.Notif.API"
RUN dotnet build "TaskFlow.Notif.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskFlow.Notif.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskFlow.Notif.API.dll"]
