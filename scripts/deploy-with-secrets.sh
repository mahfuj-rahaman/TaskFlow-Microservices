#!/bin/bash

# =============================================================================
# Deploy TaskFlow with Secret Injection
# =============================================================================
# This script handles secret injection from GitHub Secrets into Docker Compose
# Usage: ./scripts/deploy-with-secrets.sh <environment> <cloud_provider>
# =============================================================================

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
log_warning() { echo -e "${YELLOW}[WARNING]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# =============================================================================
# Parse Arguments
# =============================================================================

ENVIRONMENT="${1:-development}"
CLOUD_PROVIDER="${2:-none}"

log_info "==================================================================="
log_info "  TaskFlow Deployment with Secret Management"
log_info "==================================================================="
log_info "Environment: ${ENVIRONMENT}"
log_info "Cloud Provider: ${CLOUD_PROVIDER}"
log_info "==================================================================="

# =============================================================================
# Validate Required Secrets
# =============================================================================

validate_secrets() {
    local missing_secrets=()

    # Common secrets (required for all environments)
    [[ -z "${POSTGRES_USER}" ]] && missing_secrets+=("POSTGRES_USER")
    [[ -z "${POSTGRES_PASSWORD}" ]] && missing_secrets+=("POSTGRES_PASSWORD")
    [[ -z "${REDIS_PASSWORD}" ]] && missing_secrets+=("REDIS_PASSWORD")
    [[ -z "${RABBITMQ_USERNAME}" ]] && missing_secrets+=("RABBITMQ_USERNAME")
    [[ -z "${RABBITMQ_PASSWORD}" ]] && missing_secrets+=("RABBITMQ_PASSWORD")

    # Production secrets
    if [[ "${ENVIRONMENT}" == "production" || "${ENVIRONMENT}" == "staging" ]]; then
        [[ -z "${SEQ_API_KEY}" ]] && log_warning "SEQ_API_KEY not set (optional)"
    fi

    # Cloud-specific secrets
    case "${CLOUD_PROVIDER}" in
        aws)
            [[ -z "${AWS_ACCESS_KEY}" ]] && missing_secrets+=("AWS_ACCESS_KEY")
            [[ -z "${AWS_SECRET_KEY}" ]] && missing_secrets+=("AWS_SECRET_KEY")
            [[ -z "${AWS_REGION}" ]] && missing_secrets+=("AWS_REGION")
            ;;
        azure)
            [[ -z "${AZURE_SERVICE_BUS_CONNECTION_STRING}" ]] && missing_secrets+=("AZURE_SERVICE_BUS_CONNECTION_STRING")
            ;;
        gcp)
            [[ -z "${GCP_PROJECT_ID}" ]] && log_warning "GCP_PROJECT_ID not set"
            ;;
    esac

    if [[ ${#missing_secrets[@]} -gt 0 ]]; then
        log_error "Missing required secrets:"
        for secret in "${missing_secrets[@]}"; do
            log_error "  - ${secret}"
        done
        exit 1
    fi

    log_success "All required secrets are present"
}

# =============================================================================
# Set Environment Variables for Docker Compose
# =============================================================================

setup_environment() {
    log_info "Setting up environment variables..."

    # Export common variables
    export ENVIRONMENT
    export CLOUD_PROVIDER

    # Infrastructure configuration (based on environment)
    case "${ENVIRONMENT}" in
        development)
            export MESSAGING_TECHNOLOGY="${MESSAGING_TECHNOLOGY:-MassTransit}"
            export EVENTBUS_MODE="${EVENTBUS_MODE:-InMemory}"
            export MESSAGING_PROVIDER="${MESSAGING_PROVIDER:-RabbitMQ}"
            ;;
        staging)
            export MESSAGING_TECHNOLOGY="${MESSAGING_TECHNOLOGY:-MassTransit}"
            export EVENTBUS_MODE="${EVENTBUS_MODE:-Hybrid}"
            export MESSAGING_PROVIDER="${MESSAGING_PROVIDER:-RabbitMQ}"
            ;;
        production)
            export MESSAGING_TECHNOLOGY="${MESSAGING_TECHNOLOGY:-MassTransit}"
            export EVENTBUS_MODE="${EVENTBUS_MODE:-Persistent}"

            # Set provider based on cloud
            case "${CLOUD_PROVIDER}" in
                aws)
                    export MESSAGING_PROVIDER="AmazonSQS"
                    ;;
                azure)
                    export MESSAGING_PROVIDER="AzureServiceBus"
                    ;;
                *)
                    export MESSAGING_PROVIDER="${MESSAGING_PROVIDER:-RabbitMQ}"
                    ;;
            esac
            ;;
    esac

    # Docker image configuration
    export DOCKER_REGISTRY="${DOCKER_REGISTRY:-ghcr.io}"
    export IMAGE_PREFIX="${IMAGE_PREFIX:-$(git config user.name)/taskflow}"
    export IMAGE_TAG="${IMAGE_TAG:-$(git rev-parse --short HEAD)}"

    # Host configuration
    export RABBITMQ_HOST="${RABBITMQ_HOST:-rabbitmq}"
    export POSTGRES_HOST="${POSTGRES_HOST:-postgres}"
    export REDIS_HOST="${REDIS_HOST:-redis}"
    export CONSUL_HOST="${CONSUL_HOST:-http://consul:8500}"
    export SEQ_URL="${SEQ_URL:-http://seq:5341}"
    export JAEGER_HOST="${JAEGER_HOST:-jaeger}"

    log_success "Environment variables configured"
}

# =============================================================================
# Generate .env file (for local reference, not committed to git)
# =============================================================================

generate_env_file() {
    local env_file=".env.${ENVIRONMENT}"

    log_info "Generating ${env_file} file..."

    cat > "${env_file}" <<EOF
# =============================================================================
# TaskFlow Environment Variables - ${ENVIRONMENT^^}
# Generated: $(date -u +"%Y-%m-%d %H:%M:%S UTC")
# =============================================================================
# WARNING: This file contains secrets. Do NOT commit to git!
# =============================================================================

# Environment
ENVIRONMENT=${ENVIRONMENT}
CLOUD_PROVIDER=${CLOUD_PROVIDER}

# Infrastructure
MESSAGING_TECHNOLOGY=${MESSAGING_TECHNOLOGY}
EVENTBUS_MODE=${EVENTBUS_MODE}
MESSAGING_PROVIDER=${MESSAGING_PROVIDER}

# Docker Configuration
DOCKER_REGISTRY=${DOCKER_REGISTRY}
IMAGE_PREFIX=${IMAGE_PREFIX}
IMAGE_TAG=${IMAGE_TAG}

# Database
POSTGRES_USER=${POSTGRES_USER}
POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
POSTGRES_DB=${POSTGRES_DB:-taskflow}
POSTGRES_HOST=${POSTGRES_HOST}

# Redis
REDIS_PASSWORD=${REDIS_PASSWORD}
REDIS_HOST=${REDIS_HOST}

# RabbitMQ
RABBITMQ_USERNAME=${RABBITMQ_USERNAME}
RABBITMQ_PASSWORD=${RABBITMQ_PASSWORD}
RABBITMQ_HOST=${RABBITMQ_HOST}
RABBITMQ_PORT=${RABBITMQ_PORT:-5672}

# AWS (if applicable)
AWS_REGION=${AWS_REGION:-}
AWS_ACCESS_KEY=${AWS_ACCESS_KEY:-}
AWS_SECRET_KEY=${AWS_SECRET_KEY:-}

# Azure (if applicable)
AZURE_SERVICE_BUS_CONNECTION_STRING=${AZURE_SERVICE_BUS_CONNECTION_STRING:-}

# GCP (if applicable)
GCP_PROJECT_ID=${GCP_PROJECT_ID:-}

# Monitoring
SEQ_URL=${SEQ_URL}
SEQ_API_KEY=${SEQ_API_KEY:-}
JAEGER_HOST=${JAEGER_HOST}

# Service Discovery
CONSUL_HOST=${CONSUL_HOST}
EOF

    chmod 600 "${env_file}"
    log_success "Generated ${env_file}"
    log_warning "Remember to add .env.* to .gitignore!"
}

# =============================================================================
# Deploy with Docker Compose
# =============================================================================

deploy() {
    log_info "Deploying TaskFlow using Docker Compose..."

    # Choose compose file based on context
    local compose_files="-f docker-compose.yml"

    # Add CI/CD compose file if available
    if [[ -f "docker-compose.ci.yml" ]]; then
        compose_files="${compose_files} -f docker-compose.ci.yml"
    fi

    # Add environment-specific overrides
    case "${ENVIRONMENT}" in
        development)
            [[ -f "docker-compose.dev.yml" ]] && compose_files="${compose_files} -f docker-compose.dev.yml"
            ;;
        staging)
            [[ -f "docker-compose.staging.yml" ]] && compose_files="${compose_files} -f docker-compose.staging.yml"
            ;;
        production)
            [[ -f "docker-compose.prod.yml" ]] && compose_files="${compose_files} -f docker-compose.prod.yml"
            ;;
    esac

    log_info "Using compose files: ${compose_files}"

    # Pull latest images
    log_info "Pulling latest Docker images..."
    docker-compose ${compose_files} pull

    # Stop existing containers
    log_info "Stopping existing containers..."
    docker-compose ${compose_files} down

    # Start services
    log_info "Starting services..."
    docker-compose ${compose_files} up -d

    # Wait for services to be healthy
    log_info "Waiting for services to be healthy..."
    sleep 10

    # Check health
    docker-compose ${compose_files} ps

    log_success "Deployment completed!"
}

# =============================================================================
# Health Check
# =============================================================================

health_check() {
    log_info "Performing health checks..."

    local services=(
        "http://localhost:8080/health:API Gateway"
        # Add more services as they are implemented
    )

    local failed=0

    for service_info in "${services[@]}"; do
        IFS=':' read -r url name <<< "$service_info"

        log_info "Checking ${name}..."

        if curl -f -s "${url}" > /dev/null 2>&1; then
            log_success "${name} is healthy"
        else
            log_error "${name} health check failed"
            ((failed++))
        fi
    done

    if [[ ${failed} -eq 0 ]]; then
        log_success "All health checks passed!"
        return 0
    else
        log_error "${failed} health check(s) failed"
        return 1
    fi
}

# =============================================================================
# Main Execution
# =============================================================================

main() {
    log_info "Starting deployment process..."

    # Validate secrets
    validate_secrets

    # Setup environment
    setup_environment

    # Generate .env file for reference
    generate_env_file

    # Deploy
    deploy

    # Health check
    sleep 5
    health_check

    log_success "==================================================================="
    log_success "  Deployment Complete!"
    log_success "==================================================================="
    log_info "Environment: ${ENVIRONMENT}"
    log_info "Cloud Provider: ${CLOUD_PROVIDER}"
    log_info "API Gateway: http://localhost:8080"
    log_info "Consul UI: http://localhost:8500"
    log_info "RabbitMQ UI: http://localhost:15672"
    log_info "Seq Logs: http://localhost:5341"
    log_info "Jaeger Tracing: http://localhost:16686"
    log_success "==================================================================="
}

# Run main function
main
