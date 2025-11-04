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
COPY ["src/Services/User/TaskFlow.User.Domain/TaskFlow.User.Domain.csproj", "src/Services/User/TaskFlow.User.Domain/"]
COPY ["src/Services/User/TaskFlow.User.Application/TaskFlow.User.Application.csproj", "src/Services/User/TaskFlow.User.Application/"]
COPY ["src/Services/User/TaskFlow.User.Infrastructure/TaskFlow.User.Infrastructure.csproj", "src/Services/User/TaskFlow.User.Infrastructure/"]
COPY ["src/Services/User/TaskFlow.User.API/TaskFlow.User.API.csproj", "src/Services/User/TaskFlow.User.API/"]

# Restore dependencies
RUN dotnet restore "src/Services/User/TaskFlow.User.API/TaskFlow.User.API.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/Services/User/TaskFlow.User.API"
RUN dotnet build "TaskFlow.User.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskFlow.User.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskFlow.User.API.dll"]
