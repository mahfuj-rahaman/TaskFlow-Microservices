#!/bin/bash

################################################################################
# Infrastructure Layer Code Generator
################################################################################

generate_repository() {
    local FEATURE_NAME=$1
    local SERVICE_NAME=$2
    local INFRA_PATH=$3

    local REPO_FILE="$INFRA_PATH/Repositories/${FEATURE_NAME}Repository.cs"

    print_info "Generating ${FEATURE_NAME}Repository.cs..."

    cat > "$REPO_FILE" << EOF
using Microsoft.EntityFrameworkCore;
using TaskFlow.$SERVICE_NAME.Application.Interfaces;
using TaskFlow.$SERVICE_NAME.Domain.Entities;
using TaskFlow.$SERVICE_NAME.Infrastructure.Persistence;

namespace TaskFlow.$SERVICE_NAME.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for $FEATURE_NAME
/// </summary>
public sealed class ${FEATURE_NAME}Repository : I${FEATURE_NAME}Repository
{
    private readonly ${SERVICE_NAME}DbContext _context;

    public ${FEATURE_NAME}Repository(${SERVICE_NAME}DbContext context)
    {
        _context = context;
    }

    public async Task<${FEATURE_NAME}Entity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<${FEATURE_NAME}Entity>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<${FEATURE_NAME}Entity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<${FEATURE_NAME}Entity>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        ${FEATURE_NAME}Entity entity,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<${FEATURE_NAME}Entity>().AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(
        ${FEATURE_NAME}Entity entity,
        CancellationToken cancellationToken = default)
    {
        _context.Set<${FEATURE_NAME}Entity>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        ${FEATURE_NAME}Entity entity,
        CancellationToken cancellationToken = default)
    {
        _context.Set<${FEATURE_NAME}Entity>().Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<${FEATURE_NAME}Entity>()
            .AnyAsync(x => x.Id == id, cancellationToken);
    }
}
EOF

    print_success "Created ${FEATURE_NAME}Repository.cs"
}

generate_ef_configuration() {
    local FEATURE_NAME=$1
    local SERVICE_NAME=$2
    local INFRA_PATH=$3

    local CONFIG_FILE="$INFRA_PATH/Persistence/Configurations/${FEATURE_NAME}Configuration.cs"

    print_info "Generating ${FEATURE_NAME}Configuration.cs..."

    cat > "$CONFIG_FILE" << EOF
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.$SERVICE_NAME.Domain.Entities;

namespace TaskFlow.$SERVICE_NAME.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for $FEATURE_NAME
/// </summary>
public sealed class ${FEATURE_NAME}Configuration : IEntityTypeConfiguration<${FEATURE_NAME}Entity>
{
    public void Configure(EntityTypeBuilder<${FEATURE_NAME}Entity> builder)
    {
        builder.ToTable("${FEATURE_NAME}s");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(x => x.Name);

        // Ignore domain events (handled by base entity)
        builder.Ignore(x => x.DomainEvents);
    }
}
EOF

    print_success "Created ${FEATURE_NAME}Configuration.cs"
}
