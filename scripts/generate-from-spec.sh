#!/bin/bash

################################################################################
# TaskFlow Code Generator from Specification
#
# This script reads a feature specification (JSON) and generates all required
# code files following Clean Architecture, DDD, and CQRS patterns.
#
# Usage: ./generate-from-spec.sh <feature-name> <service-name>
# Example: ./generate-from-spec.sh Identity User
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
DATA_FILE="$DOCS_DIR/${FEATURE_NAME}_data.json"
FEATURE_FILE="$DOCS_DIR/${FEATURE_NAME}_feature.md"

# Check if data file exists
if [ ! -f "$DATA_FILE" ]; then
    print_error "Data file not found: $DATA_FILE"
    print_info "Please run: ./scripts/ai-scaffold.sh $FEATURE_NAME $SERVICE_NAME"
    exit 1
fi

# Check if feature file exists
if [ ! -f "$FEATURE_FILE" ]; then
    print_warning "Feature specification not found: $FEATURE_FILE"
fi

# Banner
echo ""
echo "╔════════════════════════════════════════════════════════════════╗"
echo "║   TaskFlow Code Generator                                      ║"
echo "║   Generating Clean Architecture code from specification        ║"
echo "╚════════════════════════════════════════════════════════════════╝"
echo ""

print_info "Feature: $FEATURE_NAME"
print_info "Service: $SERVICE_NAME"
print_info "Data Source: $DATA_FILE"

# Read JSON data (using jq if available, otherwise parse manually)
if command -v jq &> /dev/null; then
    PROPERTIES=$(jq -r '.properties' "$DATA_FILE")
    BUSINESS_RULES=$(jq -r '.businessRules[]' "$DATA_FILE" 2>/dev/null || echo "")
    ADDITIONAL_OPS=$(jq -r '.additionalOperations[]' "$DATA_FILE" 2>/dev/null || echo "")
else
    print_warning "jq not found. Using basic JSON parsing."
fi

# Base paths
SERVICE_PATH="$PROJECT_ROOT/src/Services/$SERVICE_NAME"
DOMAIN_PATH="$SERVICE_PATH/TaskFlow.$SERVICE_NAME.Domain"
APP_PATH="$SERVICE_PATH/TaskFlow.$SERVICE_NAME.Application"
INFRA_PATH="$SERVICE_PATH/TaskFlow.$SERVICE_NAME.Infrastructure"
API_PATH="$SERVICE_PATH/TaskFlow.$SERVICE_NAME.API"
TESTS_PATH="$PROJECT_ROOT/tests"

# Create directories
print_section "Creating Directory Structure"

mkdir -p "$DOMAIN_PATH/Entities"
mkdir -p "$DOMAIN_PATH/Enums"
mkdir -p "$DOMAIN_PATH/Events"
mkdir -p "$DOMAIN_PATH/Exceptions"
mkdir -p "$APP_PATH/DTOs"
mkdir -p "$APP_PATH/Interfaces"
mkdir -p "$APP_PATH/Features/${FEATURE_NAME}s/Commands"
mkdir -p "$APP_PATH/Features/${FEATURE_NAME}s/Queries"
mkdir -p "$APP_PATH/Mappings"
mkdir -p "$INFRA_PATH/Repositories"
mkdir -p "$INFRA_PATH/Persistence/Configurations"
mkdir -p "$API_PATH/Controllers"
mkdir -p "$TESTS_PATH/UnitTests/TaskFlow.$SERVICE_NAME.UnitTests/Domain"
mkdir -p "$TESTS_PATH/IntegrationTests/TaskFlow.$SERVICE_NAME.IntegrationTests/Api"

print_success "Directory structure created"

# Load helper script for code generation
source "$SCRIPT_DIR/generators/generate-domain.sh"
source "$SCRIPT_DIR/generators/generate-application.sh"
source "$SCRIPT_DIR/generators/generate-infrastructure.sh"
source "$SCRIPT_DIR/generators/generate-api.sh"
source "$SCRIPT_DIR/generators/generate-tests.sh"

# Generate Domain Layer
print_section "Generating Domain Layer"
generate_domain_entity "$FEATURE_NAME" "$SERVICE_NAME" "$DOMAIN_PATH"
generate_domain_events "$FEATURE_NAME" "$SERVICE_NAME" "$DOMAIN_PATH"
generate_domain_exceptions "$FEATURE_NAME" "$SERVICE_NAME" "$DOMAIN_PATH"

# Generate Application Layer
print_section "Generating Application Layer"
generate_dto "$FEATURE_NAME" "$SERVICE_NAME" "$APP_PATH"
generate_repository_interface "$FEATURE_NAME" "$SERVICE_NAME" "$APP_PATH"
generate_commands "$FEATURE_NAME" "$SERVICE_NAME" "$APP_PATH"
generate_queries "$FEATURE_NAME" "$SERVICE_NAME" "$APP_PATH"
generate_mapping_config "$FEATURE_NAME" "$SERVICE_NAME" "$APP_PATH"

# Generate Infrastructure Layer
print_section "Generating Infrastructure Layer"
generate_repository "$FEATURE_NAME" "$SERVICE_NAME" "$INFRA_PATH"
generate_ef_configuration "$FEATURE_NAME" "$SERVICE_NAME" "$INFRA_PATH"

# Generate API Layer
print_section "Generating API Layer"
generate_controller "$FEATURE_NAME" "$SERVICE_NAME" "$API_PATH"

# Generate Tests
print_section "Generating Tests"
generate_unit_tests "$FEATURE_NAME" "$SERVICE_NAME" "$TESTS_PATH"
generate_integration_tests "$FEATURE_NAME" "$SERVICE_NAME" "$TESTS_PATH"

# Summary
print_section "Generation Complete!"

echo ""
print_success "Successfully generated all files for $FEATURE_NAME feature"
echo ""

print_info "Next steps:"
echo "  1. Review generated code"
echo "  2. Update DbContext to include ${FEATURE_NAME}Entity"
echo "  3. Register repository in DependencyInjection.cs"
echo "  4. Create database migration:"
echo "     cd $INFRA_PATH"
echo "     dotnet ef migrations add Add${FEATURE_NAME}Entity --startup-project ../TaskFlow.$SERVICE_NAME.API"
echo "  5. Run tests:"
echo "     dotnet test"
echo "  6. Build solution:"
echo "     dotnet build"
echo ""

print_success "Feature $FEATURE_NAME is ready! 🚀"
echo ""
