#!/bin/bash

# =============================================================================
# API Gateway Launcher with Configuration Profiles
# =============================================================================
# Usage:
#   ./run-gateway.sh                          # Development (default)
#   ./run-gateway.sh dev                      # Development
#   ./run-gateway.sh staging                  # Staging
#   ./run-gateway.sh prod                     # Production
#   ./run-gateway.sh prod aws                 # Production on AWS
#   ./run-gateway.sh prod azure               # Production on Azure
#   ./run-gateway.sh prod gcp                 # Production on GCP
# =============================================================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored messages
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Parse arguments
ENVIRONMENT="${1:-dev}"
CLOUD_PROVIDER="${2:-}"

# Normalize environment name
case "${ENVIRONMENT,,}" in
    dev|development)
        ASPNETCORE_ENV="Development"
        ;;
    staging|stage)
        ASPNETCORE_ENV="Staging"
        ;;
    prod|production)
        ASPNETCORE_ENV="Production"
        ;;
    *)
        log_error "Invalid environment: $ENVIRONMENT"
        echo "Valid options: dev, staging, prod"
        exit 1
        ;;
esac

# Normalize cloud provider name (if provided)
if [[ -n "$CLOUD_PROVIDER" ]]; then
    case "${CLOUD_PROVIDER,,}" in
        aws)
            CLOUD_PROVIDER="Aws"
            ;;
        azure)
            CLOUD_PROVIDER="Azure"
            ;;
        gcp)
            CLOUD_PROVIDER="Gcp"
            ;;
        onprem|onpremise)
            CLOUD_PROVIDER="OnPremise"
            ;;
        *)
            log_error "Invalid cloud provider: $CLOUD_PROVIDER"
            echo "Valid options: aws, azure, gcp, onprem"
            exit 1
            ;;
    esac
fi

# Print configuration
echo ""
log_info "==================================================================="
log_info "  Starting API Gateway"
log_info "==================================================================="
log_info "Environment: ${ASPNETCORE_ENV}"
if [[ -n "$CLOUD_PROVIDER" ]]; then
    log_info "Cloud Provider: ${CLOUD_PROVIDER}"
else
    log_info "Cloud Provider: Not specified (using defaults)"
fi
log_info "==================================================================="
echo ""

# Check if appsettings files exist
GATEWAY_DIR="src/ApiGateway/TaskFlow.Gateway"
BASE_CONFIG="${GATEWAY_DIR}/appsettings.json"
ENV_CONFIG="${GATEWAY_DIR}/appsettings.${ASPNETCORE_ENV}.json"

if [[ ! -f "$BASE_CONFIG" ]]; then
    log_error "Base configuration not found: $BASE_CONFIG"
    exit 1
fi

if [[ ! -f "$ENV_CONFIG" ]]; then
    log_warning "Environment configuration not found: $ENV_CONFIG"
    log_warning "Using base configuration only"
fi

if [[ -n "$CLOUD_PROVIDER" ]]; then
    CLOUD_CONFIG="${GATEWAY_DIR}/appsettings.${CLOUD_PROVIDER}.${ASPNETCORE_ENV}.json"
    if [[ ! -f "$CLOUD_CONFIG" ]]; then
        log_warning "Cloud provider configuration not found: $CLOUD_CONFIG"
        log_warning "Proceeding without cloud-specific overrides"
    else
        log_info "Using cloud configuration: $CLOUD_CONFIG"
    fi
fi

# Export environment variables
export ASPNETCORE_ENVIRONMENT="$ASPNETCORE_ENV"
if [[ -n "$CLOUD_PROVIDER" ]]; then
    export CLOUD_PROVIDER
fi

# Configuration summary
log_info "Configuration loading order:"
log_info "  1. appsettings.json (base)"
log_info "  2. appsettings.${ASPNETCORE_ENV}.json"
if [[ -n "$CLOUD_PROVIDER" ]]; then
    log_info "  3. appsettings.${CLOUD_PROVIDER}.${ASPNETCORE_ENV}.json"
fi
log_info "  4. Environment variables"
log_info "  5. Command-line arguments"
echo ""

# Check for required environment variables based on configuration
if [[ "$ASPNETCORE_ENV" == "Production" ]]; then
    log_warning "Production mode requires these environment variables:"
    log_warning "  - RABBITMQ_USERNAME"
    log_warning "  - RABBITMQ_PASSWORD"
    log_warning "  - SEQ_API_KEY (optional)"

    if [[ "$CLOUD_PROVIDER" == "Aws" ]]; then
        log_warning "  - AWS_ACCESS_KEY"
        log_warning "  - AWS_SECRET_KEY"
    elif [[ "$CLOUD_PROVIDER" == "Azure" ]]; then
        log_warning "  - AZURE_SERVICE_BUS_CONNECTION_STRING"
    fi
    echo ""
fi

# Change to gateway directory
cd "$GATEWAY_DIR"

# Build the project
log_info "Building API Gateway..."
dotnet build --configuration Release --nologo --verbosity quiet

if [[ $? -ne 0 ]]; then
    log_error "Build failed!"
    exit 1
fi

log_success "Build completed successfully"
echo ""

# Run the gateway
log_info "Starting API Gateway..."
log_info "Press Ctrl+C to stop"
echo ""

dotnet run --configuration Release --no-build

# Cleanup on exit
log_info "API Gateway stopped"
