<#
.SYNOPSIS
    TaskFlow Microservices Scaffolding Script

.DESCRIPTION
    Generates production-ready CRUD implementation for a domain entity following Clean Architecture,
    DDD, CQRS patterns. Creates all required classes, interfaces, handlers, validators, controllers,
    and tests.

.PARAMETER EntityPath
    Path to the domain entity file (e.g., "src/Services/User/TaskFlow.User.Domain/Entities/UserEntity.cs")

.PARAMETER Force
    Overwrite existing files without prompting

.EXAMPLE
    .\scripts\scaffold.ps1 -EntityPath "src/Services/User/TaskFlow.User.Domain/Entities/UserEntity.cs"

.EXAMPLE
    .\scripts\scaffold.ps1 -EntityPath "src/Services/User/TaskFlow.User.Domain/Entities/UserEntity.cs" -Force

.NOTES
    Author: TaskFlow Team
    Version: 1.0.0
    Generated with Claude Code
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true, HelpMessage="Path to the domain entity file")]
    [Alias("Entity")]
    [string]$EntityPath,

    [Parameter(Mandatory=$false)]
    [switch]$Force
)

# Color output functions
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Section { param($Message) Write-Host "`n🔷 $Message" -ForegroundColor Blue -BackgroundColor White }

# Script configuration
$ErrorActionPreference = "Stop"
$ScriptRoot = Split-Path -Parent $PSScriptRoot

Write-Host @"
╔════════════════════════════════════════════════════════════════╗
║   TaskFlow Microservices - Entity Scaffolding Script          ║
║   Generates production-ready CRUD with Clean Architecture     ║
╚════════════════════════════════════════════════════════════════╝
"@ -ForegroundColor Cyan

# ============================================================================
# STEP 1: Validate and Parse Entity
# ============================================================================
Write-Section "Step 1: Validating Entity File"

# Normalize path
$EntityPath = $EntityPath -replace '^\./', '' -replace '^\.\\', ''
$FullEntityPath = Join-Path $ScriptRoot $EntityPath

if (-not (Test-Path $FullEntityPath)) {
    Write-Error "Entity file not found: $FullEntityPath"
    exit 1
}

Write-Success "Found entity file: $EntityPath"

# Parse entity information
$EntityContent = Get-Content $FullEntityPath -Raw
$EntityFileName = Split-Path -Leaf $FullEntityPath

# Extract entity name (remove .cs extension)
$EntityName = $EntityFileName -replace '\.cs$', ''
Write-Info "Entity Name: $EntityName"

# Extract namespace from file
if ($EntityContent -match 'namespace\s+([\w\.]+)') {
    $EntityNamespace = $Matches[1]
    Write-Info "Entity Namespace: $EntityNamespace"
} else {
    Write-Error "Could not extract namespace from entity file"
    exit 1
}

# Determine service name (e.g., TaskFlow.User.Domain -> User)
if ($EntityNamespace -match 'TaskFlow\.(\w+)\.Domain') {
    $ServiceName = $Matches[1]
    Write-Info "Service Name: $ServiceName"
} else {
    Write-Error "Could not determine service name from namespace: $EntityNamespace"
    exit 1
}

# Extract properties from entity (simplified - looks for public properties)
$Properties = @()
$PropertyMatches = [regex]::Matches($EntityContent, 'public\s+(\w+)\s+(\w+)\s*\{\s*get;')
foreach ($match in $PropertyMatches) {
    $propType = $match.Groups[1].Value
    $propName = $match.Groups[2].Value

    # Skip Id property (handled separately)
    if ($propName -eq 'Id') { continue }

    $Properties += @{
        Type = $propType
        Name = $propName
    }
}

Write-Info "Found $($Properties.Count) properties (excluding Id)"

# Determine ID type
if ($EntityContent -match 'AggregateRoot<(\w+)>') {
    $IdType = $Matches[1]
    Write-Info "ID Type: $IdType"
} elseif ($EntityContent -match 'Entity<(\w+)>') {
    $IdType = $Matches[1]
    Write-Info "ID Type: $IdType"
} else {
    $IdType = "Guid"
    Write-Warning "Could not determine ID type, using default: Guid"
}

# Base paths
$ServiceBasePath = "src/Services/$ServiceName"
$TestBasePath = "tests"

Write-Success "Entity analysis complete"

# ============================================================================
# STEP 2: Generate Application Layer - DTOs
# ============================================================================
Write-Section "Step 2: Generating Application Layer - DTOs"

$DtoPath = "$ServiceBasePath/TaskFlow.$ServiceName.Application/DTOs"
New-Item -ItemType Directory -Force -Path (Join-Path $ScriptRoot $DtoPath) | Out-Null

$DtoName = "${EntityName}Dto"
$DtoFile = Join-Path $ScriptRoot "$DtoPath/$DtoName.cs"

$DtoProperties = ($Properties | ForEach-Object { "    public $($_.Type) $($_.Name) { get; init; }" }) -join "`n"

$DtoContent = @"
using TaskFlow.$ServiceName.Domain.Enums;

namespace TaskFlow.$ServiceName.Application.DTOs;

/// <summary>
/// Data Transfer Object for $EntityName
/// </summary>
public sealed record ${DtoName}
{
    public $IdType Id { get; init; }
$DtoProperties
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
"@

if ($Force -or -not (Test-Path $DtoFile)) {
    Set-Content -Path $DtoFile -Value $DtoContent -Encoding UTF8
    Write-Success "Created DTO: $DtoName.cs"
} else {
    Write-Warning "DTO already exists (use -Force to overwrite): $DtoName.cs"
}

# ============================================================================
# STEP 3: Generate Application Layer - Repository Interface
# ============================================================================
Write-Section "Step 3: Generating Repository Interface"

$InterfacePath = "$ServiceBasePath/TaskFlow.$ServiceName.Application/Interfaces"
New-Item -ItemType Directory -Force -Path (Join-Path $ScriptRoot $InterfacePath) | Out-Null

$RepositoryInterface = "I${EntityName}Repository"
$RepositoryFile = Join-Path $ScriptRoot "$InterfacePath/$RepositoryInterface.cs"

$RepositoryContent = @"
using TaskFlow.$ServiceName.Domain.Entities;

namespace TaskFlow.$ServiceName.Application.Interfaces;

/// <summary>
/// Repository interface for $EntityName aggregate
/// </summary>
public interface $RepositoryInterface
{
    /// <summary>
    /// Gets a $EntityName by ID
    /// </summary>
    System.Threading.Tasks.Task<${EntityName}?> GetByIdAsync($IdType id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all ${EntityName}s with optional filtering
    /// </summary>
    System.Threading.Tasks.Task<IReadOnlyList<$EntityName>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new $EntityName
    /// </summary>
    System.Threading.Tasks.Task AddAsync($EntityName entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing $EntityName
    /// </summary>
    System.Threading.Tasks.Task UpdateAsync($EntityName entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a $EntityName
    /// </summary>
    System.Threading.Tasks.Task DeleteAsync($EntityName entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a $EntityName exists
    /// </summary>
    System.Threading.Tasks.Task<bool> ExistsAsync($IdType id, CancellationToken cancellationToken = default);
}
"@

if ($Force -or -not (Test-Path $RepositoryFile)) {
    Set-Content -Path $RepositoryFile -Value $RepositoryContent -Encoding UTF8
    Write-Success "Created Repository Interface: $RepositoryInterface.cs"
} else {
    Write-Warning "Repository Interface already exists: $RepositoryInterface.cs"
}

# ============================================================================
# STEP 4: Generate Application Layer - Mapster Configuration
# ============================================================================
Write-Section "Step 4: Generating Mapster Configuration"

$MappingPath = "$ServiceBasePath/TaskFlow.$ServiceName.Application/Mappings"
New-Item -ItemType Directory -Force -Path (Join-Path $ScriptRoot $MappingPath) | Out-Null

$MappingClass = "${EntityName}MappingConfig"
$MappingFile = Join-Path $ScriptRoot "$MappingPath/$MappingClass.cs"

$MappingContent = @"
using Mapster;
using TaskFlow.$ServiceName.Application.DTOs;
using TaskFlow.$ServiceName.Domain.Entities;

namespace TaskFlow.$ServiceName.Application.Mappings;

/// <summary>
/// Mapster configuration for $EntityName mappings
/// </summary>
public static class $MappingClass
{
    public static void Configure()
    {
        // $EntityName to ${DtoName}
        TypeAdapterConfig<$EntityName, $DtoName>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);

        // Add reverse mapping if needed
    }
}
"@

if ($Force -or -not (Test-Path $MappingFile)) {
    Set-Content -Path $MappingFile -Value $MappingContent -Encoding UTF8
    Write-Success "Created Mapping Configuration: $MappingClass.cs"
} else {
    Write-Warning "Mapping Configuration already exists: $MappingClass.cs"
}

# ============================================================================
# STEP 5: Generate Application Layer - CQRS Commands
# ============================================================================
Write-Section "Step 5: Generating CQRS Commands"

$CommandsPath = "$ServiceBasePath/TaskFlow.$ServiceName.Application/Features/${EntityName}s/Commands"

# === Create Command ===
$CreateCommandPath = "$CommandsPath/Create$EntityName"
New-Item -ItemType Directory -Force -Path (Join-Path $ScriptRoot $CreateCommandPath) | Out-Null

$CreateCommandProperties = ($Properties | ForEach-Object { "    public required $($_.Type) $($_.Name) { get; init; }" }) -join "`n"

$CreateCommandContent = @"
using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.$ServiceName.Application.Features.${EntityName}s.Commands.Create$EntityName;

/// <summary>
/// Command to create a new $EntityName
/// </summary>
public sealed record Create${EntityName}Command : IRequest<Result<$IdType>>
{
$CreateCommandProperties
}
"@

$CreateCommandFile = Join-Path $ScriptRoot "$CreateCommandPath/Create${EntityName}Command.cs"
if ($Force -or -not (Test-Path $CreateCommandFile)) {
    Set-Content -Path $CreateCommandFile -Value $CreateCommandContent -Encoding UTF8
    Write-Success "Created Command: Create${EntityName}Command.cs"
}

# === Create Command Handler ===
$CreateCommandHandlerContent = @"
using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.$ServiceName.Application.Interfaces;
using TaskFlow.$ServiceName.Domain.Entities;

namespace TaskFlow.$ServiceName.Application.Features.${EntityName}s.Commands.Create$EntityName;

/// <summary>
/// Handler for Create${EntityName}Command
/// </summary>
public sealed class Create${EntityName}CommandHandler : IRequestHandler<Create${EntityName}Command, Result<$IdType>>
{
    private readonly I${EntityName}Repository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public Create${EntityName}CommandHandler(
        I${EntityName}Repository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task<Result<$IdType>> Handle(
        Create${EntityName}Command request,
        CancellationToken cancellationToken)
    {
        // TODO: Implement Create method in $EntityName entity
        var entity = $EntityName.Create(
            // Map properties from request
        );

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(entity.Id);
    }
}
"@

$CreateCommandHandlerFile = Join-Path $ScriptRoot "$CreateCommandPath/Create${EntityName}CommandHandler.cs"
if ($Force -or -not (Test-Path $CreateCommandHandlerFile)) {
    Set-Content -Path $CreateCommandHandlerFile -Value $CreateCommandHandlerContent -Encoding UTF8
    Write-Success "Created Command Handler: Create${EntityName}CommandHandler.cs"
}

# === Create Command Validator ===
$CreateCommandValidatorContent = @"
using FluentValidation;

namespace TaskFlow.$ServiceName.Application.Features.${EntityName}s.Commands.Create$EntityName;

/// <summary>
/// Validator for Create${EntityName}Command
/// </summary>
public sealed class Create${EntityName}CommandValidator : AbstractValidator<Create${EntityName}Command>
{
    public Create${EntityName}CommandValidator()
    {
        // TODO: Add validation rules
        // Example:
        // RuleFor(x => x.PropertyName)
        //     .NotEmpty()
        //     .WithMessage("PropertyName is required")
        //     .MaximumLength(100)
        //     .WithMessage("PropertyName must not exceed 100 characters");
    }
}
"@

$CreateCommandValidatorFile = Join-Path $ScriptRoot "$CreateCommandPath/Create${EntityName}CommandValidator.cs"
if ($Force -or -not (Test-Path $CreateCommandValidatorFile)) {
    Set-Content -Path $CreateCommandValidatorFile -Value $CreateCommandValidatorContent -Encoding UTF8
    Write-Success "Created Command Validator: Create${EntityName}CommandValidator.cs"
}

# === Update Command ===
$UpdateCommandPath = "$CommandsPath/Update$EntityName"
New-Item -ItemType Directory -Force -Path (Join-Path $ScriptRoot $UpdateCommandPath) | Out-Null

$UpdateCommandContent = @"
using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.$ServiceName.Application.Features.${EntityName}s.Commands.Update$EntityName;

/// <summary>
/// Command to update an existing $EntityName
/// </summary>
public sealed record Update${EntityName}Command : IRequest<Result>
{
    public required $IdType Id { get; init; }
$CreateCommandProperties
}
"@

$UpdateCommandFile = Join-Path $ScriptRoot "$UpdateCommandPath/Update${EntityName}Command.cs"
if ($Force -or -not (Test-Path $UpdateCommandFile)) {
    Set-Content -Path $UpdateCommandFile -Value $UpdateCommandContent -Encoding UTF8
    Write-Success "Created Command: Update${EntityName}Command.cs"
}

# === Update Command Handler ===
$UpdateCommandHandlerContent = @"
using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.$ServiceName.Application.Interfaces;

namespace TaskFlow.$ServiceName.Application.Features.${EntityName}s.Commands.Update$EntityName;

/// <summary>
/// Handler for Update${EntityName}Command
/// </summary>
public sealed class Update${EntityName}CommandHandler : IRequestHandler<Update${EntityName}Command, Result>
{
    private readonly I${EntityName}Repository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public Update${EntityName}CommandHandler(
        I${EntityName}Repository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task<Result> Handle(
        Update${EntityName}Command request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure(new Error(
                "$EntityName.NotFound",
                "$EntityName not found"));
        }

        // TODO: Update entity properties using domain methods
        // Example: entity.UpdatePropertyName(request.PropertyName);

        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
"@

$UpdateCommandHandlerFile = Join-Path $ScriptRoot "$UpdateCommandPath/Update${EntityName}CommandHandler.cs"
if ($Force -or -not (Test-Path $UpdateCommandHandlerFile)) {
    Set-Content -Path $UpdateCommandHandlerFile -Value $UpdateCommandHandlerContent -Encoding UTF8
    Write-Success "Created Command Handler: Update${EntityName}CommandHandler.cs"
}

# === Delete Command ===
$DeleteCommandPath = "$CommandsPath/Delete$EntityName"
New-Item -ItemType Directory -Force -Path (Join-Path $ScriptRoot $DeleteCommandPath) | Out-Null

$DeleteCommandContent = @"
using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.$ServiceName.Application.Features.${EntityName}s.Commands.Delete$EntityName;

/// <summary>
/// Command to delete a $EntityName
/// </summary>
public sealed record Delete${EntityName}Command($IdType Id) : IRequest<Result>;
"@

$DeleteCommandFile = Join-Path $ScriptRoot "$DeleteCommandPath/Delete${EntityName}Command.cs"
if ($Force -or -not (Test-Path $DeleteCommandFile)) {
    Set-Content -Path $DeleteCommandFile -Value $DeleteCommandContent -Encoding UTF8
    Write-Success "Created Command: Delete${EntityName}Command.cs"
}

# === Delete Command Handler ===
$DeleteCommandHandlerContent = @"
using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.$ServiceName.Application.Interfaces;

namespace TaskFlow.$ServiceName.Application.Features.${EntityName}s.Commands.Delete$EntityName;

/// <summary>
/// Handler for Delete${EntityName}Command
/// </summary>
public sealed class Delete${EntityName}CommandHandler : IRequestHandler<Delete${EntityName}Command, Result>
{
    private readonly I${EntityName}Repository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public Delete${EntityName}CommandHandler(
        I${EntityName}Repository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async System.Threading.Tasks.Task<Result> Handle(
        Delete${EntityName}Command request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure(new Error(
                "$EntityName.NotFound",
                "$EntityName not found"));
        }

        await _repository.DeleteAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
"@

$DeleteCommandHandlerFile = Join-Path $ScriptRoot "$DeleteCommandPath/Delete${EntityName}CommandHandler.cs"
if ($Force -or -not (Test-Path $DeleteCommandHandlerFile)) {
    Set-Content -Path $DeleteCommandHandlerFile -Value $DeleteCommandHandlerContent -Encoding UTF8
    Write-Success "Created Command Handler: Delete${EntityName}CommandHandler.cs"
}

# ============================================================================
# STEP 6: Generate Application Layer - CQRS Queries
# ============================================================================
Write-Section "Step 6: Generating CQRS Queries"

$QueriesPath = "$ServiceBasePath/TaskFlow.$ServiceName.Application/Features/${EntityName}s/Queries"

# === Get By ID Query ===
$GetByIdQueryPath = "$QueriesPath/Get${EntityName}ById"
New-Item -ItemType Directory -Force -Path (Join-Path $ScriptRoot $GetByIdQueryPath) | Out-Null

$GetByIdQueryContent = @"
using MediatR;
using TaskFlow.$ServiceName.Application.DTOs;

namespace TaskFlow.$ServiceName.Application.Features.${EntityName}s.Queries.Get${EntityName}ById;

/// <summary>
/// Query to get a $EntityName by ID
/// </summary>
public sealed record Get${EntityName}ByIdQuery($IdType Id) : IRequest<${DtoName}?>;
"@

$GetByIdQueryFile = Join-Path $ScriptRoot "$GetByIdQueryPath/Get${EntityName}ByIdQuery.cs"
if ($Force -or -not (Test-Path $GetByIdQueryFile)) {
    Set-Content -Path $GetByIdQueryFile -Value $GetByIdQueryContent -Encoding UTF8
    Write-Success "Created Query: Get${EntityName}ByIdQuery.cs"
}

# === Get By ID Query Handler ===
$GetByIdQueryHandlerContent = @"
using Mapster;
using MediatR;
using TaskFlow.$ServiceName.Application.DTOs;
using TaskFlow.$ServiceName.Application.Interfaces;

namespace TaskFlow.$ServiceName.Application.Features.${EntityName}s.Queries.Get${EntityName}ById;

/// <summary>
/// Handler for Get${EntityName}ByIdQuery
/// </summary>
public sealed class Get${EntityName}ByIdQueryHandler : IRequestHandler<Get${EntityName}ByIdQuery, ${DtoName}?>
{
    private readonly I${EntityName}Repository _repository;

    public Get${EntityName}ByIdQueryHandler(I${EntityName}Repository repository)
    {
        _repository = repository;
    }

    public async System.Threading.Tasks.Task<${DtoName}?> Handle(
        Get${EntityName}ByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        return entity?.Adapt<${DtoName}>();
    }
}
"@

$GetByIdQueryHandlerFile = Join-Path $ScriptRoot "$GetByIdQueryPath/Get${EntityName}ByIdQueryHandler.cs"
if ($Force -or -not (Test-Path $GetByIdQueryHandlerFile)) {
    Set-Content -Path $GetByIdQueryHandlerFile -Value $GetByIdQueryHandlerContent -Encoding UTF8
    Write-Success "Created Query Handler: Get${EntityName}ByIdQueryHandler.cs"
}

# === Get All Query ===
$GetAllQueryPath = "$QueriesPath/GetAll${EntityName}s"
New-Item -ItemType Directory -Force -Path (Join-Path $ScriptRoot $GetAllQueryPath) | Out-Null

$GetAllQueryContent = @"
using MediatR;
using TaskFlow.$ServiceName.Application.DTOs;

namespace TaskFlow.$ServiceName.Application.Features.${EntityName}s.Queries.GetAll${EntityName}s;

/// <summary>
/// Query to get all ${EntityName}s
/// </summary>
public sealed record GetAll${EntityName}sQuery : IRequest<IReadOnlyList<${DtoName}>>;
"@

$GetAllQueryFile = Join-Path $ScriptRoot "$GetAllQueryPath/GetAll${EntityName}sQuery.cs"
if ($Force -or -not (Test-Path $GetAllQueryFile)) {
    Set-Content -Path $GetAllQueryFile -Value $GetAllQueryContent -Encoding UTF8
    Write-Success "Created Query: GetAll${EntityName}sQuery.cs"
}

# === Get All Query Handler ===
$GetAllQueryHandlerContent = @"
using Mapster;
using MediatR;
using TaskFlow.$ServiceName.Application.DTOs;
using TaskFlow.$ServiceName.Application.Interfaces;

namespace TaskFlow.$ServiceName.Application.Features.${EntityName}s.Queries.GetAll${EntityName}s;

/// <summary>
/// Handler for GetAll${EntityName}sQuery
/// </summary>
public sealed class GetAll${EntityName}sQueryHandler : IRequestHandler<GetAll${EntityName}sQuery, IReadOnlyList<${DtoName}>>
{
    private readonly I${EntityName}Repository _repository;

    public GetAll${EntityName}sQueryHandler(I${EntityName}Repository repository)
    {
        _repository = repository;
    }

    public async System.Threading.Tasks.Task<IReadOnlyList<${DtoName}>> Handle(
        GetAll${EntityName}sQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);

        return entities.Adapt<IReadOnlyList<${DtoName}>>();
    }
}
"@

$GetAllQueryHandlerFile = Join-Path $ScriptRoot "$GetAllQueryPath/GetAll${EntityName}sQueryHandler.cs"
if ($Force -or -not (Test-Path $GetAllQueryHandlerFile)) {
    Set-Content -Path $GetAllQueryHandlerFile -Value $GetAllQueryHandlerContent -Encoding UTF8
    Write-Success "Created Query Handler: GetAll${EntityName}sQueryHandler.cs"
}

# ============================================================================
# STEP 7: Generate Infrastructure Layer - Repository Implementation
# ============================================================================
Write-Section "Step 7: Generating Infrastructure Layer - Repository"

$RepositoriesPath = "$ServiceBasePath/TaskFlow.$ServiceName.Infrastructure/Repositories"
New-Item -ItemType Directory -Force -Path (Join-Path $ScriptRoot $RepositoriesPath) | Out-Null

$RepositoryImplContent = @"
using Microsoft.EntityFrameworkCore;
using TaskFlow.$ServiceName.Application.Interfaces;
using TaskFlow.$ServiceName.Domain.Entities;
using TaskFlow.$ServiceName.Infrastructure.Persistence;

namespace TaskFlow.$ServiceName.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for $EntityName
/// </summary>
public sealed class ${EntityName}Repository : I${EntityName}Repository
{
    private readonly ${ServiceName}DbContext _context;

    public ${EntityName}Repository(${ServiceName}DbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<${EntityName}?> GetByIdAsync(
        $IdType id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<$EntityName>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async System.Threading.Tasks.Task<IReadOnlyList<$EntityName>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<$EntityName>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task AddAsync(
        $EntityName entity,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<$EntityName>().AddAsync(entity, cancellationToken);
    }

    public System.Threading.Tasks.Task UpdateAsync(
        $EntityName entity,
        CancellationToken cancellationToken = default)
    {
        _context.Set<$EntityName>().Update(entity);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public System.Threading.Tasks.Task DeleteAsync(
        $EntityName entity,
        CancellationToken cancellationToken = default)
    {
        _context.Set<$EntityName>().Remove(entity);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public async System.Threading.Tasks.Task<bool> ExistsAsync(
        $IdType id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<$EntityName>()
            .AnyAsync(x => x.Id == id, cancellationToken);
    }
}
"@

$RepositoryImplFile = Join-Path $ScriptRoot "$RepositoriesPath/${EntityName}Repository.cs"
if ($Force -or -not (Test-Path $RepositoryImplFile)) {
    Set-Content -Path $RepositoryImplFile -Value $RepositoryImplContent -Encoding UTF8
    Write-Success "Created Repository Implementation: ${EntityName}Repository.cs"
}

# ============================================================================
# STEP 8: Generate Infrastructure Layer - Entity Configuration
# ============================================================================
Write-Section "Step 8: Generating Entity Configuration"

$ConfigurationsPath = "$ServiceBasePath/TaskFlow.$ServiceName.Infrastructure/Persistence/Configurations"
New-Item -ItemType Directory -Force -Path (Join-Path $ScriptRoot $ConfigurationsPath) | Out-Null

$ConfigurationContent = @"
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.$ServiceName.Domain.Entities;

namespace TaskFlow.$ServiceName.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for $EntityName
/// </summary>
public sealed class ${EntityName}Configuration : IEntityTypeConfiguration<$EntityName>
{
    public void Configure(EntityTypeBuilder<$EntityName> builder)
    {
        builder.ToTable("${EntityName}s");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        // TODO: Configure properties
        // Example:
        // builder.Property(x => x.PropertyName)
        //     .IsRequired()
        //     .HasMaxLength(100);

        // TODO: Configure indexes
        // Example:
        // builder.HasIndex(x => x.PropertyName);

        // TODO: Configure relationships
        // Example:
        // builder.HasOne(x => x.RelatedEntity)
        //     .WithMany()
        //     .HasForeignKey(x => x.RelatedEntityId);

        // Ignore domain events (handled by base entity)
        builder.Ignore(x => x.DomainEvents);
    }
}
"@

$ConfigurationFile = Join-Path $ScriptRoot "$ConfigurationsPath/${EntityName}Configuration.cs"
if ($Force -or -not (Test-Path $ConfigurationFile)) {
    Set-Content -Path $ConfigurationFile -Value $ConfigurationContent -Encoding UTF8
    Write-Success "Created Entity Configuration: ${EntityName}Configuration.cs"
}

# ============================================================================
# STEP 9: Generate API Layer - Controller
# ============================================================================
Write-Section "Step 9: Generating API Controller"

$ControllersPath = "$ServiceBasePath/TaskFlow.$ServiceName.API/Controllers"
New-Item -ItemType Directory -Force -Path (Join-Path $ScriptRoot $ControllersPath) | Out-Null

$ControllerContent = @"
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.$ServiceName.Application.DTOs;
using TaskFlow.$ServiceName.Application.Features.${EntityName}s.Commands.Create$EntityName;
using TaskFlow.$ServiceName.Application.Features.${EntityName}s.Commands.Update$EntityName;
using TaskFlow.$ServiceName.Application.Features.${EntityName}s.Commands.Delete$EntityName;
using TaskFlow.$ServiceName.Application.Features.${EntityName}s.Queries.Get${EntityName}ById;
using TaskFlow.$ServiceName.Application.Features.${EntityName}s.Queries.GetAll${EntityName}s;

namespace TaskFlow.$ServiceName.API.Controllers;

/// <summary>
/// Controller for $EntityName operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class ${EntityName}sController : ApiController
{
    private readonly ISender _sender;

    public ${EntityName}sController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all ${EntityName}s
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<${DtoName}>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll${EntityName}s(CancellationToken cancellationToken)
    {
        var query = new GetAll${EntityName}sQuery();
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get $EntityName by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(${DtoName}), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get${EntityName}ById($IdType id, CancellationToken cancellationToken)
    {
        var query = new Get${EntityName}ByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        if (result is null)
        {
            return NotFound(new { message = "$EntityName not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new $EntityName
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof($IdType), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create${EntityName}(
        [FromBody] Create${EntityName}Command command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        return HandleResult(result, id => CreatedAtAction(
            nameof(Get${EntityName}ById),
            new { id },
            new { id }));
    }

    /// <summary>
    /// Update an existing $EntityName
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update${EntityName}(
        $IdType id,
        [FromBody] Update${EntityName}Command command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }

        var result = await _sender.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Delete a $EntityName
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete${EntityName}($IdType id, CancellationToken cancellationToken)
    {
        var command = new Delete${EntityName}Command(id);
        var result = await _sender.Send(command, cancellationToken);

        return HandleResult(result);
    }
}
"@

$ControllerFile = Join-Path $ScriptRoot "$ControllersPath/${EntityName}sController.cs"
if ($Force -or -not (Test-Path $ControllerFile)) {
    Set-Content -Path $ControllerFile -Value $ControllerContent -Encoding UTF8
    Write-Success "Created API Controller: ${EntityName}sController.cs"
}

# Check if ApiController base exists, if not create it
$ApiControllerFile = Join-Path $ScriptRoot "$ControllersPath/ApiController.cs"
if (-not (Test-Path $ApiControllerFile)) {
    $ApiControllerContent = @"
using Microsoft.AspNetCore.Mvc;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.$ServiceName.API.Controllers;

/// <summary>
/// Base controller with common functionality
/// </summary>
[ApiController]
public abstract class ApiController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result, Func<T, IActionResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess?.Invoke(result.Value) ?? Ok(result.Value);
        }

        return HandleFailure(result);
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return HandleFailure(result);
    }

    private IActionResult HandleFailure(Result result)
    {
        return result.Error.Code switch
        {
            _ when result.Error.Code.EndsWith(".NotFound") => NotFound(new { error = result.Error.Message }),
            _ when result.Error.Code.EndsWith(".Validation") => BadRequest(new { error = result.Error.Message }),
            _ => StatusCode(500, new { error = "An error occurred processing your request" })
        };
    }
}
"@
    Set-Content -Path $ApiControllerFile -Value $ApiControllerContent -Encoding UTF8
    Write-Success "Created Base ApiController.cs"
}

# ============================================================================
# STEP 10: Generate Tests - Unit Tests
# ============================================================================
Write-Section "Step 10: Generating Unit Tests"

$UnitTestsPath = "$TestBasePath/UnitTests/TaskFlow.$ServiceName.UnitTests/Domain"
New-Item -ItemType Directory -Force -Path (Join-Path $ScriptRoot $UnitTestsPath) | Out-Null

$UnitTestContent = @"
using FluentAssertions;
using TaskFlow.$ServiceName.Domain.Entities;
using TaskFlow.$ServiceName.Domain.Exceptions;
using Xunit;

namespace TaskFlow.$ServiceName.UnitTests.Domain;

/// <summary>
/// Unit tests for $EntityName entity
/// </summary>
public class ${EntityName}Tests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreate${EntityName}()
    {
        // Arrange
        // TODO: Add test data

        // Act
        var entity = $EntityName.Create(/* parameters */);

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().NotBe(default($IdType));
        // TODO: Add more assertions
    }

    [Fact]
    public void Create_WithInvalidParameters_ShouldThrowException()
    {
        // Arrange
        // TODO: Add invalid test data

        // Act
        var act = () => $EntityName.Create(/* invalid parameters */);

        // Assert
        act.Should().Throw<${ServiceName}DomainException>();
    }

    // TODO: Add more tests for domain methods
}
"@

$UnitTestFile = Join-Path $ScriptRoot "$UnitTestsPath/${EntityName}Tests.cs"
if ($Force -or -not (Test-Path $UnitTestFile)) {
    Set-Content -Path $UnitTestFile -Value $UnitTestContent -Encoding UTF8
    Write-Success "Created Unit Tests: ${EntityName}Tests.cs"
}

# ============================================================================
# STEP 11: Generate Tests - Integration Tests
# ============================================================================
Write-Section "Step 11: Generating Integration Tests"

$IntegrationTestsPath = "$TestBasePath/IntegrationTests/TaskFlow.$ServiceName.IntegrationTests/Api"
New-Item -ItemType Directory -Force -Path (Join-Path $ScriptRoot $IntegrationTestsPath) | Out-Null

$IntegrationTestContent = @"
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.$ServiceName.Application.DTOs;
using TaskFlow.$ServiceName.Application.Features.${EntityName}s.Commands.Create$EntityName;
using Xunit;

namespace TaskFlow.$ServiceName.IntegrationTests.Api;

/// <summary>
/// Integration tests for ${EntityName}sController
/// </summary>
public class ${EntityName}sControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ${EntityName}sControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll${EntityName}s_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/${EntityName}s");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create${EntityName}_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var command = new Create${EntityName}Command
        {
            // TODO: Add test data
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/${EntityName}s", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    // TODO: Add more integration tests
}
"@

$IntegrationTestFile = Join-Path $ScriptRoot "$IntegrationTestsPath/${EntityName}sControllerTests.cs"
if ($Force -or -not (Test-Path $IntegrationTestFile)) {
    Set-Content -Path $IntegrationTestFile -Value $IntegrationTestContent -Encoding UTF8
    Write-Success "Created Integration Tests: ${EntityName}sControllerTests.cs"
}

# ============================================================================
# STEP 12: Generate Summary Document
# ============================================================================
Write-Section "Step 12: Generating Scaffolding Summary"

$SummaryContent = @"
# Scaffolding Summary - $EntityName

Generated on: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Entity Information
- **Entity Name:** $EntityName
- **Service Name:** $ServiceName
- **ID Type:** $IdType
- **Namespace:** $EntityNamespace
- **Properties:** $($Properties.Count)

## Generated Files

### Application Layer (DTOs & Interfaces)
- ✅ DTOs/$DtoName.cs
- ✅ Interfaces/I${EntityName}Repository.cs
- ✅ Mappings/${EntityName}MappingConfig.cs

### Application Layer (CQRS Commands)
- ✅ Features/${EntityName}s/Commands/Create$EntityName/Create${EntityName}Command.cs
- ✅ Features/${EntityName}s/Commands/Create$EntityName/Create${EntityName}CommandHandler.cs
- ✅ Features/${EntityName}s/Commands/Create$EntityName/Create${EntityName}CommandValidator.cs
- ✅ Features/${EntityName}s/Commands/Update$EntityName/Update${EntityName}Command.cs
- ✅ Features/${EntityName}s/Commands/Update$EntityName/Update${EntityName}CommandHandler.cs
- ✅ Features/${EntityName}s/Commands/Delete$EntityName/Delete${EntityName}Command.cs
- ✅ Features/${EntityName}s/Commands/Delete$EntityName/Delete${EntityName}CommandHandler.cs

### Application Layer (CQRS Queries)
- ✅ Features/${EntityName}s/Queries/Get${EntityName}ById/Get${EntityName}ByIdQuery.cs
- ✅ Features/${EntityName}s/Queries/Get${EntityName}ById/Get${EntityName}ByIdQueryHandler.cs
- ✅ Features/${EntityName}s/Queries/GetAll${EntityName}s/GetAll${EntityName}sQuery.cs
- ✅ Features/${EntityName}s/Queries/GetAll${EntityName}s/GetAll${EntityName}sQueryHandler.cs

### Infrastructure Layer
- ✅ Repositories/${EntityName}Repository.cs
- ✅ Persistence/Configurations/${EntityName}Configuration.cs

### API Layer
- ✅ Controllers/${EntityName}sController.cs
- ✅ Controllers/ApiController.cs (if not exists)

### Tests
- ✅ UnitTests/Domain/${EntityName}Tests.cs
- ✅ IntegrationTests/Api/${EntityName}sControllerTests.cs

## Next Steps

### 1. Update Domain Entity
Add a static Create method to your domain entity:
``````csharp
public static $EntityName Create(/* parameters */)
{
    // Validation logic
    // Create instance
    // Raise domain events
    return entity;
}
``````

### 2. Update DbContext
Add DbSet to your DbContext:
``````csharp
public DbSet<$EntityName> ${EntityName}s => Set<$EntityName>();
``````

Apply configuration:
``````csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfiguration(new ${EntityName}Configuration());
}
``````

### 3. Register Repository in DI
Update DependencyInjection.cs:
``````csharp
services.AddScoped<I${EntityName}Repository, ${EntityName}Repository>();
``````

### 4. Configure Mapster
Update Program.cs:
``````csharp
${EntityName}MappingConfig.Configure();
``````

### 5. Create Migration
``````bash
cd $ServiceBasePath/TaskFlow.$ServiceName.Infrastructure
dotnet ef migrations add Add${EntityName}Entity --startup-project ../TaskFlow.$ServiceName.API
``````

### 6. Update Tests
- Fill in TODOs in unit tests
- Add test data to integration tests
- Implement validation rules

### 7. Test the Implementation
``````bash
# Run tests
dotnet test

# Run the API
dotnet run --project $ServiceBasePath/TaskFlow.$ServiceName.API

# Test endpoints
curl http://localhost:5000/api/v1/${EntityName}s
``````

## API Endpoints

- **GET** /api/v1/${EntityName}s - Get all ${EntityName}s
- **GET** /api/v1/${EntityName}s/{id} - Get ${EntityName} by ID
- **POST** /api/v1/${EntityName}s - Create new ${EntityName}
- **PUT** /api/v1/${EntityName}s/{id} - Update ${EntityName}
- **DELETE** /api/v1/${EntityName}s/{id} - Delete ${EntityName}

## Architecture

``````
$ServiceName Service
├── Domain Layer
│   └── Entities/$EntityName.cs (Your existing entity)
├── Application Layer
│   ├── DTOs
│   ├── Commands (Create, Update, Delete)
│   ├── Queries (GetById, GetAll)
│   └── Interfaces
├── Infrastructure Layer
│   ├── Repositories
│   └── Configurations
└── API Layer
    └── Controllers
``````

## Pattern Summary

✅ Clean Architecture - 4 layers with proper separation
✅ Domain-Driven Design - Entity as aggregate root
✅ CQRS - Separate commands and queries
✅ Repository Pattern - Data access abstraction
✅ Result Pattern - Functional error handling
✅ Validation - FluentValidation
✅ Mapping - Mapster
✅ RESTful API - Standard HTTP methods

## Generated with

🤖 TaskFlow Scaffolding Script v1.0.0
Generated with Claude Code
"@

$SummaryFile = Join-Path $ScriptRoot "SCAFFOLD-SUMMARY-${EntityName}.md"
Set-Content -Path $SummaryFile -Value $SummaryContent -Encoding UTF8
Write-Success "Created Scaffolding Summary: SCAFFOLD-SUMMARY-${EntityName}.md"

# ============================================================================
# FINAL SUMMARY
# ============================================================================
Write-Host "`n"
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║         Scaffolding Complete! 🎉                               ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host "`n"

Write-Info "Entity: $EntityName"
Write-Info "Service: $ServiceName"
Write-Info "Files Generated: ~20 files"
Write-Host "`n"

Write-Host "📋 Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Review generated files and complete TODOs"
Write-Host "  2. Update DbContext and register repository"
Write-Host "  3. Configure Mapster in Program.cs"
Write-Host "  4. Create EF Core migration"
Write-Host "  5. Run tests and validate"
Write-Host "`n"

Write-Host "📖 For detailed instructions, see:" -ForegroundColor Cyan
Write-Host "   SCAFFOLD-SUMMARY-${EntityName}.md"
Write-Host "`n"

Write-Success "Scaffolding completed successfully!"
Write-Host "`n"
