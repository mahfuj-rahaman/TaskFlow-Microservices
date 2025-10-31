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

    # Read properties from JSON file
    local PROPS=""
    if [ -f "$DATA_FILE" ]; then
        # Extract properties from JSON and convert to C# properties
        PROPS=$(grep -o '"name"[[:space:]]*:[[:space:]]*"[^"]*"' "$DATA_FILE" | \
                sed 's/"name"[[:space:]]*:[[:space:]]*"\([^"]*\)"/\1/' | \
                while read -r prop_name; do
                    # Get the type for this property
                    prop_type=$(grep -A 1 "\"name\"[[:space:]]*:[[:space:]]*\"$prop_name\"" "$DATA_FILE" | \
                                grep '"type"' | sed 's/.*"type"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/')

                    # Get if required
                    is_required=$(grep -A 2 "\"name\"[[:space:]]*:[[:space:]]*\"$prop_name\"" "$DATA_FILE" | \
                                 grep '"isRequired"' | sed 's/.*"isRequired"[[:space:]]*:[[:space:]]*\([^,]*\).*/\1/')

                    # Get default value - extract the full line for complex values
                    default_val=$(grep -A 5 "\"name\"[[:space:]]*:[[:space:]]*\"$prop_name\"" "$DATA_FILE" | \
                                 grep '"defaultValue"' | sed 's/.*"defaultValue"[[:space:]]*:[[:space:]]*"\(.*\)"[,}]*/\1/' | sed 's/\\"/"/g')

                    # Build C# property
                    nullable=""
                    initializer=""

                    # Handle nullable types
                    if [ "$is_required" = "false" ] && [[ ! "$prop_type" =~ ^(bool|int|decimal|double)$ ]]; then
                        if [ "$prop_type" = "DateTime" ]; then
                            nullable="?"
                        elif [ "$prop_type" = "string" ]; then
                            nullable=""
                        fi
                    fi

                    # Handle default values
                    if [ -n "$default_val" ]; then
                        if [[ "$prop_type" == List* ]]; then
                            # For List types, use the value as-is (already contains the full initialization)
                            initializer=" = $default_val;"
                        elif [ "$prop_type" = "bool" ]; then
                            initializer=" = ${default_val,,};"  # Convert to lowercase
                        elif [[ "$prop_type" =~ ^(int|decimal|double)$ ]]; then
                            initializer=" = $default_val;"
                        elif [ "$prop_type" = "string" ]; then
                            initializer=" = \"$default_val\";"
                        else
                            # For enums or other custom types, qualify with type name if not already qualified
                            if [[ ! "$default_val" =~ \. ]]; then
                                initializer=" = ${prop_type}.${default_val};"
                            else
                                initializer=" = $default_val;"
                            fi
                        fi
                    elif [ "$prop_type" = "string" ] && [ "$is_required" != "false" ]; then
                        initializer=" = string.Empty;"
                    fi

                    echo "    public ${prop_type}${nullable} ${prop_name} { get; private set; }${initializer}"
                done)
    fi

    # If no properties from JSON, use defaults
    if [ -z "$PROPS" ]; then
        PROPS="    public string Name { get; private set; } = string.Empty;"
    fi

    # Write the entity file directly without using sed for complex substitutions
    cat > "$ENTITY_FILE" << EOF
using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.$SERVICE_NAME.Domain.Events;
using TaskFlow.$SERVICE_NAME.Domain.Exceptions;
using TaskFlow.$SERVICE_NAME.Domain.Enums;

namespace TaskFlow.$SERVICE_NAME.Domain.Entities;

/// <summary>
/// $FEATURE_NAME aggregate root
/// </summary>
public sealed class ${FEATURE_NAME}Entity : AggregateRoot<Guid>
{
$PROPS
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Private constructor for EF Core
    private ${FEATURE_NAME}Entity(Guid id) : base(id)
    {
    }

    /// <summary>
    /// Creates a new $FEATURE_NAME
    /// </summary>
    public static ${FEATURE_NAME}Entity Create()
    {
        var entity = new ${FEATURE_NAME}Entity(Guid.NewGuid())
        {
            CreatedAt = DateTime.UtcNow
        };

        entity.RaiseDomainEvent(new ${FEATURE_NAME}CreatedDomainEvent(entity.Id));

        return entity;
    }

    /// <summary>
    /// Updates $FEATURE_NAME information
    /// </summary>
    public void Update()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deletes $FEATURE_NAME
    /// </summary>
    public void Delete()
    {
        RaiseDomainEvent(new ${FEATURE_NAME}DeletedDomainEvent(Id));
    }
}
EOF

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
public sealed record ${FEATURE_NAME}CreatedDomainEvent(Guid ${FEATURE_NAME}Id) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
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
public sealed record ${FEATURE_NAME}DeletedDomainEvent(Guid ${FEATURE_NAME}Id) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
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

generate_domain_enums() {
    local FEATURE_NAME=$1
    local SERVICE_NAME=$2
    local DOMAIN_PATH=$3

    local ENUMS_DIR="$DOMAIN_PATH/Enums"

    # Extract custom enum types from JSON
    if [ ! -f "$DATA_FILE" ]; then
        return
    fi

    # Find all unique enum types used in properties
    local enum_types=$(grep -o '"type"[[:space:]]*:[[:space:]]*"[^"]*"' "$DATA_FILE" | \
                      sed 's/"type"[[:space:]]*:[[:space:]]*"\([^"]*\)"/\1/' | \
                      grep -v -E '^(string|int|bool|DateTime|decimal|double|float|Guid|long|short|byte|List<)' | \
                      sort -u)

    if [ -z "$enum_types" ]; then
        return
    fi

    # Generate each enum
    while IFS= read -r enum_type; do
        [ -z "$enum_type" ] && continue

        print_info "Generating ${enum_type}.cs..."

        # Create enum file
        cat > "$ENUMS_DIR/${enum_type}.cs" << EOF
namespace TaskFlow.$SERVICE_NAME.Domain.Enums;

/// <summary>
/// $enum_type enumeration
/// </summary>
public enum $enum_type
{
    /// <summary>
    /// Active status
    /// </summary>
    Active = 1,

    /// <summary>
    /// Inactive status
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Locked status
    /// </summary>
    Locked = 3,

    /// <summary>
    /// Suspended status
    /// </summary>
    Suspended = 4
}
EOF

        print_success "Created ${enum_type}.cs"
    done <<< "$enum_types"
}
