#!/bin/bash

################################################################################
# Domain Layer Code Generator
################################################################################

generate_domain_entity() {
    local FEATURE_NAME=$1
    local SERVICE_NAME=$2
    local DOMAIN_PATH=$3

    local ENTITY_FILE="$DOMAIN_PATH/Entities/${FEATURE_NAME}Entity.cs"

    print_info "Generating ${FEATURE_NAME}Entity.cs..."

    # Read properties from JSON if available
    local PROPS=""
    if [ -f "$DATA_FILE" ] && command -v jq &> /dev/null; then
        PROPS=$(jq -r '.properties[] | "    public \(.type) \(.name) { get; private set; }"' "$DATA_FILE" 2>/dev/null)
    fi

    # If no properties from JSON, use defaults
    if [ -z "$PROPS" ]; then
        PROPS="    public string Name { get; private set; } = string.Empty;"
    fi

    cat > "$ENTITY_FILE" << 'EOF'
using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.${SERVICE_NAME}.Domain.Events;
using TaskFlow.${SERVICE_NAME}.Domain.Exceptions;

namespace TaskFlow.${SERVICE_NAME}.Domain.Entities;

/// <summary>
/// ${FEATURE_NAME} aggregate root
/// </summary>
public sealed class ${FEATURE_NAME}Entity : AggregateRoot<Guid>
{
${PROPERTIES}
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Private constructor for EF Core
    private ${FEATURE_NAME}Entity(Guid id) : base(id)
    {
    }

    /// <summary>
    /// Creates a new ${FEATURE_NAME}
    /// </summary>
    public static ${FEATURE_NAME}Entity Create(string name)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(name))
            throw new ${SERVICE_NAME}DomainException("Name is required");

        var entity = new ${FEATURE_NAME}Entity(Guid.NewGuid())
        {
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        entity.RaiseDomainEvent(new ${FEATURE_NAME}CreatedDomainEvent(entity.Id, entity.Name));

        return entity;
    }

    /// <summary>
    /// Updates ${FEATURE_NAME} information
    /// </summary>
    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ${SERVICE_NAME}DomainException("Name is required");

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deletes ${FEATURE_NAME}
    /// </summary>
    public void Delete()
    {
        RaiseDomainEvent(new ${FEATURE_NAME}DeletedDomainEvent(Id));
    }
}
EOF

    # Replace placeholders
    sed -i "s/\${SERVICE_NAME}/$SERVICE_NAME/g" "$ENTITY_FILE"
    sed -i "s/\${FEATURE_NAME}/$FEATURE_NAME/g" "$ENTITY_FILE"
    sed -i "s/\${PROPERTIES}/$PROPS/g" "$ENTITY_FILE"

    print_success "Created ${FEATURE_NAME}Entity.cs"
}

generate_domain_events() {
    local FEATURE_NAME=$1
    local SERVICE_NAME=$2
    local DOMAIN_PATH=$3

    local EVENTS_DIR="$DOMAIN_PATH/Events"

    # ${FEATURE_NAME}CreatedDomainEvent
    print_info "Generating ${FEATURE_NAME}CreatedDomainEvent.cs..."

    cat > "$EVENTS_DIR/${FEATURE_NAME}CreatedDomainEvent.cs" << EOF
using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.$SERVICE_NAME.Domain.Events;

/// <summary>
/// Domain event raised when a $FEATURE_NAME is created
/// </summary>
public sealed record ${FEATURE_NAME}CreatedDomainEvent(
    Guid ${FEATURE_NAME}Id,
    string Name) : IDomainEvent;
EOF

    print_success "Created ${FEATURE_NAME}CreatedDomainEvent.cs"

    # ${FEATURE_NAME}DeletedDomainEvent
    print_info "Generating ${FEATURE_NAME}DeletedDomainEvent.cs..."

    cat > "$EVENTS_DIR/${FEATURE_NAME}DeletedDomainEvent.cs" << EOF
using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.$SERVICE_NAME.Domain.Events;

/// <summary>
/// Domain event raised when a $FEATURE_NAME is deleted
/// </summary>
public sealed record ${FEATURE_NAME}DeletedDomainEvent(Guid ${FEATURE_NAME}Id) : IDomainEvent;
EOF

    print_success "Created ${FEATURE_NAME}DeletedDomainEvent.cs"
}

generate_domain_exceptions() {
    local FEATURE_NAME=$1
    local SERVICE_NAME=$2
    local DOMAIN_PATH=$3

    local EXCEPTION_FILE="$DOMAIN_PATH/Exceptions/${FEATURE_NAME}DomainException.cs"

    print_info "Generating ${FEATURE_NAME}DomainException.cs..."

    cat > "$EXCEPTION_FILE" << EOF
namespace TaskFlow.$SERVICE_NAME.Domain.Exceptions;

/// <summary>
/// Exception thrown when a domain rule is violated in $FEATURE_NAME
/// </summary>
public sealed class ${FEATURE_NAME}DomainException : Exception
{
    public ${FEATURE_NAME}DomainException(string message) : base(message)
    {
    }

    public ${FEATURE_NAME}DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
EOF

    print_success "Created ${FEATURE_NAME}DomainException.cs"
}
