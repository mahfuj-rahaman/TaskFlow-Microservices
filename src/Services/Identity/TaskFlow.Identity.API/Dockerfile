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
COPY ["src/Services/Identity/TaskFlow.Identity.Domain/TaskFlow.Identity.Domain.csproj", "src/Services/Identity/TaskFlow.Identity.Domain/"]
COPY ["src/Services/Identity/TaskFlow.Identity.Application/TaskFlow.Identity.Application.csproj", "src/Services/Identity/TaskFlow.Identity.Application/"]
COPY ["src/Services/Identity/TaskFlow.Identity.Infrastructure/TaskFlow.Identity.Infrastructure.csproj", "src/Services/Identity/TaskFlow.Identity.Infrastructure/"]
COPY ["src/Services/Identity/TaskFlow.Identity.API/TaskFlow.Identity.API.csproj", "src/Services/Identity/TaskFlow.Identity.API/"]

# Restore dependencies
RUN dotnet restore "src/Services/Identity/TaskFlow.Identity.API/TaskFlow.Identity.API.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/Services/Identity/TaskFlow.Identity.API"
RUN dotnet build "TaskFlow.Identity.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskFlow.Identity.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskFlow.Identity.API.dll"]
