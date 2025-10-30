#!/bin/bash

################################################################################
# TaskFlow AI-Powered Feature Scaffolding System
#
# This script uses AI to generate complete microservice features from scratch
# based on business requirements and user input.
#
# Usage: ./ai-scaffold.sh <feature-name> <service-name>
# Example: ./ai-scaffold.sh Identity User
################################################################################

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Function to print colored output
print_success() { echo -e "${GREEN}✓${NC} $1"; }
print_info() { echo -e "${CYAN}ℹ${NC} $1"; }
print_warning() { echo -e "${YELLOW}⚠${NC} $1"; }
print_error() { echo -e "${RED}✗${NC} $1"; }
print_section() { echo -e "\n${BLUE}▶${NC} $1"; }

# Validate arguments
if [ $# -ne 2 ]; then
    print_error "Usage: $0 <feature-name> <service-name>"
    print_info "Example: $0 Identity User"
    exit 1
fi

FEATURE_NAME=$1
SERVICE_NAME=$2
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
DOCS_DIR="$PROJECT_ROOT/docs/features"
FEATURE_FILE="$DOCS_DIR/${FEATURE_NAME}_feature.md"

# Create docs directory if it doesn't exist
mkdir -p "$DOCS_DIR"

# Banner
echo ""
echo "╔════════════════════════════════════════════════════════════════╗"
echo "║   TaskFlow AI-Powered Feature Scaffolding System              ║"
echo "║   Generate complete features from business requirements       ║"
echo "╚════════════════════════════════════════════════════════════════╝"
echo ""

print_info "Feature: $FEATURE_NAME"
print_info "Service: $SERVICE_NAME"
print_info "Feature Document: $FEATURE_FILE"

# Check if feature file already exists
if [ -f "$FEATURE_FILE" ]; then
    print_warning "Feature document already exists: $FEATURE_FILE"
    read -p "Do you want to recreate it? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        print_info "Using existing feature document"
        SKIP_CREATION=true
    fi
fi

# Step 1: Create feature specification document
if [ -z "$SKIP_CREATION" ]; then
    print_section "Step 1: Creating Feature Specification Document"

    cat > "$FEATURE_FILE" << EOF
# $FEATURE_NAME Feature Specification

**Service**: $SERVICE_NAME
**Created**: $(date +"%Y-%m-%d %H:%M:%S")
**Status**: Draft

---

## 1. Overview

### Purpose
[AI will fill this based on feature name and user input]

### Scope
[AI will define the scope of this feature]

---

## 2. Business Requirements

### Functional Requirements
- [ ] [AI will list functional requirements]

### Non-Functional Requirements
- [ ] Performance requirements
- [ ] Security requirements
- [ ] Scalability requirements

---

## 3. Domain Model

### Aggregate Root: ${FEATURE_NAME}Entity

#### Properties
| Property | Type | Required | Description |
|----------|------|----------|-------------|
| Id | Guid | Yes | Unique identifier |
| CreatedAt | DateTime | Yes | Creation timestamp |
| UpdatedAt | DateTime? | No | Last update timestamp |

#### Business Rules
1. [AI will define business rules]

#### Domain Events
- [ ] ${FEATURE_NAME}Created
- [ ] ${FEATURE_NAME}Updated
- [ ] ${FEATURE_NAME}Deleted

---

## 4. Use Cases

### Commands (Write Operations)
1. **Create${FEATURE_NAME}**
   - Input: [AI will define]
   - Output: ${FEATURE_NAME}Id
   - Validation: [AI will define]

2. **Update${FEATURE_NAME}**
   - Input: [AI will define]
   - Output: Success/Failure
   - Validation: [AI will define]

3. **Delete${FEATURE_NAME}**
   - Input: ${FEATURE_NAME}Id
   - Output: Success/Failure
   - Validation: [AI will define]

### Queries (Read Operations)
1. **Get${FEATURE_NAME}ById**
   - Input: ${FEATURE_NAME}Id
   - Output: ${FEATURE_NAME}Dto

2. **GetAll${FEATURE_NAME}s**
   - Input: Pagination parameters
   - Output: List<${FEATURE_NAME}Dto>

---

## 5. API Endpoints

| Method | Endpoint | Description | Request | Response |
|--------|----------|-------------|---------|----------|
| POST | /api/v1/${FEATURE_NAME,,}s | Create new ${FEATURE_NAME} | Create${FEATURE_NAME}Command | 201 Created |
| GET | /api/v1/${FEATURE_NAME,,}s/{id} | Get by ID | - | 200 OK |
| GET | /api/v1/${FEATURE_NAME,,}s | Get all | Query params | 200 OK |
| PUT | /api/v1/${FEATURE_NAME,,}s/{id} | Update | Update${FEATURE_NAME}Command | 204 No Content |
| DELETE | /api/v1/${FEATURE_NAME,,}s/{id} | Delete | - | 204 No Content |

---

## 6. Data Model

### Database Table: ${FEATURE_NAME}s

\`\`\`sql
CREATE TABLE ${FEATURE_NAME}s (
    Id uniqueidentifier PRIMARY KEY,
    CreatedAt datetime2 NOT NULL,
    UpdatedAt datetime2 NULL,
    -- Additional fields will be defined by AI
);
\`\`\`

### Indexes
- [ ] Primary Key on Id
- [ ] [AI will suggest additional indexes]

---

## 7. Security & Authorization

### Permissions Required
- [ ] Create${FEATURE_NAME}: [Define role]
- [ ] Read${FEATURE_NAME}: [Define role]
- [ ] Update${FEATURE_NAME}: [Define role]
- [ ] Delete${FEATURE_NAME}: [Define role]

---

## 8. Integration Points

### Dependencies
- [ ] [AI will list dependencies on other services]

### Events Published
- [ ] ${FEATURE_NAME}CreatedEvent
- [ ] ${FEATURE_NAME}UpdatedEvent
- [ ] ${FEATURE_NAME}DeletedEvent

### Events Consumed
- [ ] [AI will list events this feature consumes]

---

## 9. Testing Strategy

### Unit Tests
- [ ] Domain entity tests
- [ ] Command handler tests
- [ ] Query handler tests
- [ ] Validator tests

### Integration Tests
- [ ] API endpoint tests
- [ ] Database integration tests
- [ ] Event publishing tests

---

## 10. Implementation Checklist

### Domain Layer
- [ ] ${FEATURE_NAME}Entity.cs
- [ ] ${FEATURE_NAME}Status enum (if needed)
- [ ] Domain events
- [ ] Domain exceptions

### Application Layer
- [ ] ${FEATURE_NAME}Dto.cs
- [ ] I${FEATURE_NAME}Repository.cs
- [ ] Commands (Create, Update, Delete)
- [ ] Command Handlers
- [ ] Command Validators
- [ ] Queries (GetById, GetAll)
- [ ] Query Handlers
- [ ] Mapping configuration

### Infrastructure Layer
- [ ] ${FEATURE_NAME}Repository.cs
- [ ] ${FEATURE_NAME}Configuration.cs (EF Core)
- [ ] Database migrations

### API Layer
- [ ] ${FEATURE_NAME}sController.cs
- [ ] API documentation

### Tests
- [ ] ${FEATURE_NAME}EntityTests.cs
- [ ] Command handler tests
- [ ] Query handler tests
- [ ] ${FEATURE_NAME}sControllerTests.cs

---

## 11. User Input Required

Please provide the following information to complete this specification:

1. **What is the main purpose of this feature?**
   - Answer: _[User will provide]_

2. **What properties should the ${FEATURE_NAME} entity have?**
   - Example: Email (string), FirstName (string), LastName (string), etc.
   - Answer: _[User will provide]_

3. **What are the key business rules?**
   - Example: Email must be unique, User must be 18+, etc.
   - Answer: _[User will provide]_

4. **What validations are required?**
   - Answer: _[User will provide]_

5. **Are there any special relationships with other entities?**
   - Answer: _[User will provide]_

6. **What additional operations beyond CRUD are needed?**
   - Answer: _[User will provide]_

---

## Next Steps

After reviewing and completing this specification:
1. Run: \`./scripts/generate-from-spec.sh ${FEATURE_NAME} ${SERVICE_NAME}\`
2. This will generate all the code files based on this specification
3. Review generated code and run tests
4. Create database migration
5. Deploy and test

EOF

    print_success "Feature specification created: $FEATURE_FILE"
fi

# Step 2: Show the feature file to user
print_section "Step 2: Review Feature Specification"

if command -v cat > /dev/null 2>&1; then
    echo ""
    echo "═══════════════════════════════════════════════════════════════════"
    cat "$FEATURE_FILE"
    echo "═══════════════════════════════════════════════════════════════════"
    echo ""
fi

# Step 3: Ask user for input
print_section "Step 3: Gather Requirements"

echo ""
print_info "Please answer the following questions to customize your feature:"
echo ""

# Question 1: Purpose
echo "1. What is the main purpose of the $FEATURE_NAME feature?"
read -p "   Answer: " PURPOSE

# Question 2: Properties
echo ""
echo "2. What properties should the ${FEATURE_NAME}Entity have?"
echo "   Format: PropertyName:Type:Required (one per line, empty line to finish)"
echo "   Example: Email:string:true"
PROPERTIES=()
while true; do
    read -p "   Property: " PROP
    if [ -z "$PROP" ]; then
        break
    fi
    PROPERTIES+=("$PROP")
done

# Question 3: Business Rules
echo ""
echo "3. What are the key business rules?"
echo "   (one per line, empty line to finish)"
BUSINESS_RULES=()
while true; do
    read -p "   Rule: " RULE
    if [ -z "$RULE" ]; then
        break
    fi
    BUSINESS_RULES+=("$RULE")
done

# Question 4: Additional Operations
echo ""
echo "4. What additional operations beyond CRUD are needed?"
echo "   Example: ActivateUser, DeactivateUser, ResetPassword"
echo "   (one per line, empty line to finish)"
ADDITIONAL_OPS=()
while true; do
    read -p "   Operation: " OP
    if [ -z "$OP" ]; then
        break
    fi
    ADDITIONAL_OPS+=("$OP")
done

# Step 4: Update feature specification with user input
print_section "Step 4: Updating Feature Specification"

# Create temporary file with updated content
TEMP_FILE="${FEATURE_FILE}.tmp"
cp "$FEATURE_FILE" "$TEMP_FILE"

# Update purpose
if [ -n "$PURPOSE" ]; then
    sed -i "s|\[AI will fill this based on feature name and user input\]|$PURPOSE|g" "$TEMP_FILE"
fi

# Add properties to table
if [ ${#PROPERTIES[@]} -gt 0 ]; then
    PROPERTIES_TABLE=""
    for PROP in "${PROPERTIES[@]}"; do
        IFS=':' read -r PNAME PTYPE PREQ <<< "$PROP"
        REQ_TEXT="Yes"
        if [ "$PREQ" = "false" ]; then
            REQ_TEXT="No"
        fi
        PROPERTIES_TABLE="${PROPERTIES_TABLE}| $PNAME | $PTYPE | $REQ_TEXT | [Description] |\n"
    done
    # This is complex in sed, so we'll note it for manual update or use a better tool
    print_info "Properties recorded: ${#PROPERTIES[@]} properties"
fi

# Add business rules
if [ ${#BUSINESS_RULES[@]} -gt 0 ]; then
    print_info "Business rules recorded: ${#BUSINESS_RULES[@]} rules"
fi

# Add additional operations
if [ ${#ADDITIONAL_OPS[@]} -gt 0 ]; then
    print_info "Additional operations recorded: ${#ADDITIONAL_OPS[@]} operations"
fi

mv "$TEMP_FILE" "$FEATURE_FILE"
print_success "Feature specification updated"

# Step 5: Confirmation
print_section "Step 5: Ready to Generate Code"

echo ""
print_info "Feature specification is ready at: $FEATURE_FILE"
print_info ""
print_warning "Next step: Generate code from specification"
echo ""
read -p "Do you want to generate the code now? (y/N): " -n 1 -r
echo ""

if [[ $REPLY =~ ^[Yy]$ ]]; then
    print_info "Calling code generator..."

    # Create the data file for the generator
    DATA_FILE="$DOCS_DIR/${FEATURE_NAME}_data.json"

    cat > "$DATA_FILE" << EOF
{
  "featureName": "$FEATURE_NAME",
  "serviceName": "$SERVICE_NAME",
  "purpose": "$PURPOSE",
  "properties": [
EOF

    # Add properties
    for i in "${!PROPERTIES[@]}"; do
        IFS=':' read -r PNAME PTYPE PREQ <<< "${PROPERTIES[$i]}"
        echo "    {\"name\": \"$PNAME\", \"type\": \"$PTYPE\", \"required\": $PREQ}" >> "$DATA_FILE"
        if [ $i -lt $((${#PROPERTIES[@]} - 1)) ]; then
            echo "," >> "$DATA_FILE"
        else
            echo "" >> "$DATA_FILE"
        fi
    done

    cat >> "$DATA_FILE" << EOF
  ],
  "businessRules": [
EOF

    # Add business rules
    for i in "${!BUSINESS_RULES[@]}"; do
        echo "    \"${BUSINESS_RULES[$i]}\"" >> "$DATA_FILE"
        if [ $i -lt $((${#BUSINESS_RULES[@]} - 1)) ]; then
            echo "," >> "$DATA_FILE"
        else
            echo "" >> "$DATA_FILE"
        fi
    done

    cat >> "$DATA_FILE" << EOF
  ],
  "additionalOperations": [
EOF

    # Add additional operations
    for i in "${!ADDITIONAL_OPS[@]}"; do
        echo "    \"${ADDITIONAL_OPS[$i]}\"" >> "$DATA_FILE"
        if [ $i -lt $((${#ADDITIONAL_OPS[@]} - 1)) ]; then
            echo "," >> "$DATA_FILE"
        else
            echo "" >> "$DATA_FILE"
        fi
    done

    cat >> "$DATA_FILE" << EOF
  ]
}
EOF

    print_success "Feature data saved to: $DATA_FILE"
    print_info "You can now run: ./scripts/generate-from-spec.sh $FEATURE_NAME $SERVICE_NAME"
    print_info "Or continue with AI assistance to generate the code"
else
    print_info "You can generate code later by running:"
    print_info "  ./scripts/generate-from-spec.sh $FEATURE_NAME $SERVICE_NAME"
fi

echo ""
print_success "Feature scaffolding setup complete!"
echo ""
