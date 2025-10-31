#!/bin/bash

################################################################################
# Application Layer Code Generator
################################################################################

generate_dto() {
    local FEATURE_NAME=$1
    local SERVICE_NAME=$2
    local APP_PATH=$3

    local DTO_FILE="$APP_PATH/DTOs/${FEATURE_NAME}Dto.cs"

    print_info "Generating ${FEATURE_NAME}Dto.cs..."

    cat > "$DTO_FILE" << EOF
namespace TaskFlow.$SERVICE_NAME.Application.DTOs;

/// <summary>
/// Data Transfer Object for $FEATURE_NAME
/// </summary>
public sealed record ${FEATURE_NAME}Dto
{
    public Guid Id { get; init; }
    // TODO: Add properties based on ${FEATURE_NAME}Entity
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
EOF

    print_success "Created ${FEATURE_NAME}Dto.cs"
}

generate_repository_interface() {
    local FEATURE_NAME=$1
    local SERVICE_NAME=$2
    local APP_PATH=$3

    local INTERFACE_FILE="$APP_PATH/Interfaces/I${FEATURE_NAME}Repository.cs"

    print_info "Generating I${FEATURE_NAME}Repository.cs..."

    cat > "$INTERFACE_FILE" << EOF
using TaskFlow.$SERVICE_NAME.Domain.Entities;

namespace TaskFlow.$SERVICE_NAME.Application.Interfaces;

/// <summary>
/// Repository interface for $FEATURE_NAME aggregate
/// </summary>
public interface I${FEATURE_NAME}Repository
{
    /// <summary>
    /// Gets a $FEATURE_NAME by ID
    /// </summary>
    Task<${FEATURE_NAME}Entity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all ${FEATURE_NAME}s
    /// </summary>
    Task<IReadOnlyList<${FEATURE_NAME}Entity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new $FEATURE_NAME
    /// </summary>
    Task AddAsync(${FEATURE_NAME}Entity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing $FEATURE_NAME
    /// </summary>
    Task UpdateAsync(${FEATURE_NAME}Entity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a $FEATURE_NAME
    /// </summary>
    Task DeleteAsync(${FEATURE_NAME}Entity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a $FEATURE_NAME exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
EOF

    print_success "Created I${FEATURE_NAME}Repository.cs"
}

generate_commands() {
    local FEATURE_NAME=$1
    local SERVICE_NAME=$2
    local APP_PATH=$3

    local COMMANDS_PATH="$APP_PATH/Features/${FEATURE_NAME}s/Commands"

    # Create${FEATURE_NAME} Command
    mkdir -p "$COMMANDS_PATH/Create$FEATURE_NAME"

    print_info "Generating Create${FEATURE_NAME} command files..."

    # Command
    cat > "$COMMANDS_PATH/Create$FEATURE_NAME/Create${FEATURE_NAME}Command.cs" << EOF
using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Commands.Create$FEATURE_NAME;

/// <summary>
/// Command to create a new $FEATURE_NAME
/// </summary>
public sealed record Create${FEATURE_NAME}Command : IRequest<Result<Guid>>
{
    // TODO: Add required properties for creating ${FEATURE_NAME}
}
EOF

    # Handler
    cat > "$COMMANDS_PATH/Create$FEATURE_NAME/Create${FEATURE_NAME}CommandHandler.cs" << EOF
using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.$SERVICE_NAME.Application.Interfaces;
using TaskFlow.$SERVICE_NAME.Domain.Entities;

namespace TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Commands.Create$FEATURE_NAME;

/// <summary>
/// Handler for Create${FEATURE_NAME}Command
/// </summary>
public sealed class Create${FEATURE_NAME}CommandHandler : IRequestHandler<Create${FEATURE_NAME}Command, Result<Guid>>
{
    private readonly I${FEATURE_NAME}Repository _repository;

    public Create${FEATURE_NAME}CommandHandler(I${FEATURE_NAME}Repository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(
        Create${FEATURE_NAME}Command request,
        CancellationToken cancellationToken)
    {
        // TODO: Customize entity creation with actual properties from request
        var entity = ${FEATURE_NAME}Entity.Create();

        await _repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id);
    }
}
EOF

    # Validator
    cat > "$COMMANDS_PATH/Create$FEATURE_NAME/Create${FEATURE_NAME}CommandValidator.cs" << EOF
using FluentValidation;

namespace TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Commands.Create$FEATURE_NAME;

/// <summary>
/// Validator for Create${FEATURE_NAME}Command
/// </summary>
public sealed class Create${FEATURE_NAME}CommandValidator : AbstractValidator<Create${FEATURE_NAME}Command>
{
    public Create${FEATURE_NAME}CommandValidator()
    {
        // TODO: Add validation rules for Create${FEATURE_NAME}Command properties
    }
}
EOF

    print_success "Created Create${FEATURE_NAME} command files"

    # Update${FEATURE_NAME} Command
    mkdir -p "$COMMANDS_PATH/Update$FEATURE_NAME"

    print_info "Generating Update${FEATURE_NAME} command files..."

    # Command
    cat > "$COMMANDS_PATH/Update$FEATURE_NAME/Update${FEATURE_NAME}Command.cs" << EOF
using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Commands.Update$FEATURE_NAME;

/// <summary>
/// Command to update an existing $FEATURE_NAME
/// </summary>
public sealed record Update${FEATURE_NAME}Command : IRequest<Result>
{
    public required Guid Id { get; init; }
    // TODO: Add properties to update for ${FEATURE_NAME}
}
EOF

    # Handler
    cat > "$COMMANDS_PATH/Update$FEATURE_NAME/Update${FEATURE_NAME}CommandHandler.cs" << EOF
using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.$SERVICE_NAME.Application.Interfaces;

namespace TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Commands.Update$FEATURE_NAME;

/// <summary>
/// Handler for Update${FEATURE_NAME}Command
/// </summary>
public sealed class Update${FEATURE_NAME}CommandHandler : IRequestHandler<Update${FEATURE_NAME}Command, Result>
{
    private readonly I${FEATURE_NAME}Repository _repository;

    public Update${FEATURE_NAME}CommandHandler(I${FEATURE_NAME}Repository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(
        Update${FEATURE_NAME}Command request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure(new Error(
                "$FEATURE_NAME.NotFound",
                "$FEATURE_NAME not found"));
        }

        // TODO: Customize entity update with actual properties from request
        entity.Update();

        await _repository.UpdateAsync(entity, cancellationToken);

        return Result.Success();
    }
}
EOF

    # Validator
    cat > "$COMMANDS_PATH/Update$FEATURE_NAME/Update${FEATURE_NAME}CommandValidator.cs" << EOF
using FluentValidation;

namespace TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Commands.Update$FEATURE_NAME;

/// <summary>
/// Validator for Update${FEATURE_NAME}Command
/// </summary>
public sealed class Update${FEATURE_NAME}CommandValidator : AbstractValidator<Update${FEATURE_NAME}Command>
{
    public Update${FEATURE_NAME}CommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");

        // TODO: Add validation rules for Update${FEATURE_NAME}Command properties
    }
}
EOF

    print_success "Created Update${FEATURE_NAME} command files"

    # Delete${FEATURE_NAME} Command
    mkdir -p "$COMMANDS_PATH/Delete$FEATURE_NAME"

    print_info "Generating Delete${FEATURE_NAME} command files..."

    # Command
    cat > "$COMMANDS_PATH/Delete$FEATURE_NAME/Delete${FEATURE_NAME}Command.cs" << EOF
using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Commands.Delete$FEATURE_NAME;

/// <summary>
/// Command to delete a $FEATURE_NAME
/// </summary>
public sealed record Delete${FEATURE_NAME}Command(Guid Id) : IRequest<Result>;
EOF

    # Handler
    cat > "$COMMANDS_PATH/Delete$FEATURE_NAME/Delete${FEATURE_NAME}CommandHandler.cs" << EOF
using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.$SERVICE_NAME.Application.Interfaces;

namespace TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Commands.Delete$FEATURE_NAME;

/// <summary>
/// Handler for Delete${FEATURE_NAME}Command
/// </summary>
public sealed class Delete${FEATURE_NAME}CommandHandler : IRequestHandler<Delete${FEATURE_NAME}Command, Result>
{
    private readonly I${FEATURE_NAME}Repository _repository;

    public Delete${FEATURE_NAME}CommandHandler(I${FEATURE_NAME}Repository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(
        Delete${FEATURE_NAME}Command request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure(new Error(
                "$FEATURE_NAME.NotFound",
                "$FEATURE_NAME not found"));
        }

        entity.Delete();

        await _repository.DeleteAsync(entity, cancellationToken);

        return Result.Success();
    }
}
EOF

    print_success "Created Delete${FEATURE_NAME} command files"
}

generate_queries() {
    local FEATURE_NAME=$1
    local SERVICE_NAME=$2
    local APP_PATH=$3

    local QUERIES_PATH="$APP_PATH/Features/${FEATURE_NAME}s/Queries"

    # Get${FEATURE_NAME}ById Query
    mkdir -p "$QUERIES_PATH/Get${FEATURE_NAME}ById"

    print_info "Generating Get${FEATURE_NAME}ById query files..."

    # Query
    cat > "$QUERIES_PATH/Get${FEATURE_NAME}ById/Get${FEATURE_NAME}ByIdQuery.cs" << EOF
using MediatR;
using TaskFlow.$SERVICE_NAME.Application.DTOs;

namespace TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Queries.Get${FEATURE_NAME}ById;

/// <summary>
/// Query to get a $FEATURE_NAME by ID
/// </summary>
public sealed record Get${FEATURE_NAME}ByIdQuery(Guid Id) : IRequest<${FEATURE_NAME}Dto?>;
EOF

    # Handler
    cat > "$QUERIES_PATH/Get${FEATURE_NAME}ById/Get${FEATURE_NAME}ByIdQueryHandler.cs" << EOF
using Mapster;
using MediatR;
using TaskFlow.$SERVICE_NAME.Application.DTOs;
using TaskFlow.$SERVICE_NAME.Application.Interfaces;

namespace TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Queries.Get${FEATURE_NAME}ById;

/// <summary>
/// Handler for Get${FEATURE_NAME}ByIdQuery
/// </summary>
public sealed class Get${FEATURE_NAME}ByIdQueryHandler : IRequestHandler<Get${FEATURE_NAME}ByIdQuery, ${FEATURE_NAME}Dto?>
{
    private readonly I${FEATURE_NAME}Repository _repository;

    public Get${FEATURE_NAME}ByIdQueryHandler(I${FEATURE_NAME}Repository repository)
    {
        _repository = repository;
    }

    public async Task<${FEATURE_NAME}Dto?> Handle(
        Get${FEATURE_NAME}ByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        return entity?.Adapt<${FEATURE_NAME}Dto>();
    }
}
EOF

    print_success "Created Get${FEATURE_NAME}ById query files"

    # GetAll${FEATURE_NAME}s Query
    mkdir -p "$QUERIES_PATH/GetAll${FEATURE_NAME}s"

    print_info "Generating GetAll${FEATURE_NAME}s query files..."

    # Query
    cat > "$QUERIES_PATH/GetAll${FEATURE_NAME}s/GetAll${FEATURE_NAME}sQuery.cs" << EOF
using MediatR;
using TaskFlow.$SERVICE_NAME.Application.DTOs;

namespace TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Queries.GetAll${FEATURE_NAME}s;

/// <summary>
/// Query to get all ${FEATURE_NAME}s
/// </summary>
public sealed record GetAll${FEATURE_NAME}sQuery : IRequest<IReadOnlyList<${FEATURE_NAME}Dto>>;
EOF

    # Handler
    cat > "$QUERIES_PATH/GetAll${FEATURE_NAME}s/GetAll${FEATURE_NAME}sQueryHandler.cs" << EOF
using Mapster;
using MediatR;
using TaskFlow.$SERVICE_NAME.Application.DTOs;
using TaskFlow.$SERVICE_NAME.Application.Interfaces;

namespace TaskFlow.$SERVICE_NAME.Application.Features.${FEATURE_NAME}s.Queries.GetAll${FEATURE_NAME}s;

/// <summary>
/// Handler for GetAll${FEATURE_NAME}sQuery
/// </summary>
public sealed class GetAll${FEATURE_NAME}sQueryHandler : IRequestHandler<GetAll${FEATURE_NAME}sQuery, IReadOnlyList<${FEATURE_NAME}Dto>>
{
    private readonly I${FEATURE_NAME}Repository _repository;

    public GetAll${FEATURE_NAME}sQueryHandler(I${FEATURE_NAME}Repository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<${FEATURE_NAME}Dto>> Handle(
        GetAll${FEATURE_NAME}sQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);

        return entities.Adapt<IReadOnlyList<${FEATURE_NAME}Dto>>();
    }
}
EOF

    print_success "Created GetAll${FEATURE_NAME}s query files"
}

generate_mapping_config() {
    local FEATURE_NAME=$1
    local SERVICE_NAME=$2
    local APP_PATH=$3

    local MAPPING_FILE="$APP_PATH/Mappings/${FEATURE_NAME}MappingConfig.cs"

    print_info "Generating ${FEATURE_NAME}MappingConfig.cs..."

    cat > "$MAPPING_FILE" << EOF
using Mapster;
using TaskFlow.$SERVICE_NAME.Application.DTOs;
using TaskFlow.$SERVICE_NAME.Domain.Entities;

namespace TaskFlow.$SERVICE_NAME.Application.Mappings;

/// <summary>
/// Mapster configuration for $FEATURE_NAME mappings
/// </summary>
public static class ${FEATURE_NAME}MappingConfig
{
    public static void Configure()
    {
        // $FEATURE_NAME to ${FEATURE_NAME}Dto
        TypeAdapterConfig<${FEATURE_NAME}Entity, ${FEATURE_NAME}Dto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            // TODO: Add property mappings
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);
    }
}
EOF

    print_success "Created ${FEATURE_NAME}MappingConfig.cs"
}
