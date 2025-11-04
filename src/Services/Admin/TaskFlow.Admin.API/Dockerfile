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
COPY ["src/Services/Admin/TaskFlow.Admin.Domain/TaskFlow.Admin.Domain.csproj", "src/Services/Admin/TaskFlow.Admin.Domain/"]
COPY ["src/Services/Admin/TaskFlow.Admin.Application/TaskFlow.Admin.Application.csproj", "src/Services/Admin/TaskFlow.Admin.Application/"]
COPY ["src/Services/Admin/TaskFlow.Admin.Infrastructure/TaskFlow.Admin.Infrastructure.csproj", "src/Services/Admin/TaskFlow.Admin.Infrastructure/"]
COPY ["src/Services/Admin/TaskFlow.Admin.API/TaskFlow.Admin.API.csproj", "src/Services/Admin/TaskFlow.Admin.API/"]

# Restore dependencies
RUN dotnet restore "src/Services/Admin/TaskFlow.Admin.API/TaskFlow.Admin.API.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/Services/Admin/TaskFlow.Admin.API"
RUN dotnet build "TaskFlow.Admin.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskFlow.Admin.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskFlow.Admin.API.dll"]
