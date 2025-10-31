#!/bin/bash

################################################################################
# Project Files (.csproj) Generator
################################################################################

generate_domain_csproj() {
    local SERVICE_NAME=$1
    local DOMAIN_PATH=$2

    local CSPROJ_FILE="$DOMAIN_PATH/TaskFlow.$SERVICE_NAME.Domain.csproj"

    print_info "Generating TaskFlow.$SERVICE_NAME.Domain.csproj..."

    cat > "$CSPROJ_FILE" << 'EOF'
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

    print_success "Created TaskFlow.$SERVICE_NAME.Domain.csproj"
}

generate_application_csproj() {
    local SERVICE_NAME=$1
    local APP_PATH=$2

    local CSPROJ_FILE="$APP_PATH/TaskFlow.$SERVICE_NAME.Application.csproj"

    print_info "Generating TaskFlow.$SERVICE_NAME.Application.csproj..."

    cat > "$CSPROJ_FILE" << EOF
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
  </ItemGroup>

</Project>
EOF

    print_success "Created TaskFlow.$SERVICE_NAME.Application.csproj"
}

generate_infrastructure_csproj() {
    local SERVICE_NAME=$1
    local INFRA_PATH=$2

    local CSPROJ_FILE="$INFRA_PATH/TaskFlow.$SERVICE_NAME.Infrastructure.csproj"

    print_info "Generating TaskFlow.$SERVICE_NAME.Infrastructure.csproj..."

    cat > "$CSPROJ_FILE" << EOF
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MassTransit" Version="8.2.5" />
    <PackageReference Include="MassTransit.RabbitMQ" Version="8.2.5" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.10" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.10">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.10" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.10" />
    <PackageReference Include="Polly" Version="8.4.2" />
    <PackageReference Include="StackExchange.Redis" Version="2.8.16" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\\TaskFlow.$SERVICE_NAME.Application\\TaskFlow.$SERVICE_NAME.Application.csproj" />
  </ItemGroup>

</Project>
EOF

    print_success "Created TaskFlow.$SERVICE_NAME.Infrastructure.csproj"
}

generate_api_csproj() {
    local SERVICE_NAME=$1
    local API_PATH=$2

    local CSPROJ_FILE="$API_PATH/TaskFlow.$SERVICE_NAME.API.csproj"

    print_info "Generating TaskFlow.$SERVICE_NAME.API.csproj..."

    cat > "$CSPROJ_FILE" << EOF
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Asp.Versioning.Mvc.ApiExplorer" Version="8.1.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.10" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.21" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
    <PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.8.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\\TaskFlow.$SERVICE_NAME.Infrastructure\\TaskFlow.$SERVICE_NAME.Infrastructure.csproj" />
  </ItemGroup>

</Project>
EOF

    print_success "Created TaskFlow.$SERVICE_NAME.API.csproj"
}
