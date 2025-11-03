#!/bin/bash

# =============================================================================
# Deploy TaskFlow to AWS (ECS/EKS)
# =============================================================================
# This script deploys TaskFlow microservices to AWS
# Supports: ECS (Fargate), EKS (Kubernetes)
# =============================================================================

set -e

source "$(dirname "$0")/common-functions.sh"

# =============================================================================
# Configuration
# =============================================================================

AWS_REGION="${AWS_REGION:-us-east-1}"
DEPLOYMENT_TYPE="${DEPLOYMENT_TYPE:-ecs}"  # ecs or eks
ECS_CLUSTER_NAME="${ECS_CLUSTER_NAME:-taskflow-cluster}"
EKS_CLUSTER_NAME="${EKS_CLUSTER_NAME:-taskflow-eks-cluster}"

log_info "==================================================================="
log_info "  TaskFlow AWS Deployment"
log_info "==================================================================="
log_info "Region: ${AWS_REGION}"
log_info "Deployment Type: ${DEPLOYMENT_TYPE}"
log_info "==================================================================="

# =============================================================================
# Validate AWS Credentials
# =============================================================================

validate_aws_credentials() {
    log_info "Validating AWS credentials..."

    if ! aws sts get-caller-identity > /dev/null 2>&1; then
        log_error "AWS credentials not configured"
        log_error "Run: aws configure"
        exit 1
    fi

    log_success "AWS credentials validated"
}

# =============================================================================
# Deploy to ECS (Fargate)
# =============================================================================

deploy_to_ecs() {
    log_info "Deploying to AWS ECS (Fargate)..."

    # Register task definitions
    log_info "Registering task definitions..."

    aws ecs register-task-definition \
        --cli-input-json file://aws/ecs/task-definition-api-gateway.json \
        --region ${AWS_REGION}

    # Update services
    log_info "Updating ECS services..."

    aws ecs update-service \
        --cluster ${ECS_CLUSTER_NAME} \
        --service taskflow-api-gateway \
        --task-definition taskflow-api-gateway \
        --force-new-deployment \
        --region ${AWS_REGION}

    # Wait for deployment
    log_info "Waiting for deployment to complete..."

    aws ecs wait services-stable \
        --cluster ${ECS_CLUSTER_NAME} \
        --services taskflow-api-gateway \
        --region ${AWS_REGION}

    log_success "ECS deployment complete"
}

# =============================================================================
# Deploy to EKS (Kubernetes)
# =============================================================================

deploy_to_eks() {
    log_info "Deploying to AWS EKS..."

    # Update kubeconfig
    log_info "Updating kubeconfig..."
    aws eks update-kubeconfig \
        --name ${EKS_CLUSTER_NAME} \
        --region ${AWS_REGION}

    # Apply Kubernetes manifests
    log_info "Applying Kubernetes manifests..."

    kubectl apply -f k8s/namespaces/
    kubectl apply -f k8s/secrets/
    kubectl apply -f k8s/configmaps/
    kubectl apply -f k8s/deployments/
    kubectl apply -f k8s/services/
    kubectl apply -f k8s/ingress/

    # Wait for rollout
    log_info "Waiting for rollout..."
    kubectl rollout status deployment/api-gateway -n taskflow

    log_success "EKS deployment complete"
}

# =============================================================================
# Setup AWS Infrastructure (if needed)
# =============================================================================

setup_aws_infrastructure() {
    log_info "Setting up AWS infrastructure..."

    # Create Parameter Store secrets
    log_info "Storing secrets in AWS Parameter Store..."

    aws ssm put-parameter \
        --name "/taskflow/production/postgres/username" \
        --value "${POSTGRES_USER}" \
        --type "SecureString" \
        --overwrite \
        --region ${AWS_REGION} || true

    aws ssm put-parameter \
        --name "/taskflow/production/postgres/password" \
        --value "${POSTGRES_PASSWORD}" \
        --type "SecureString" \
        --overwrite \
        --region ${AWS_REGION} || true

    aws ssm put-parameter \
        --name "/taskflow/production/rabbitmq/username" \
        --value "${RABBITMQ_USERNAME}" \
        --type "SecureString" \
        --overwrite \
        --region ${AWS_REGION} || true

    aws ssm put-parameter \
        --name "/taskflow/production/rabbitmq/password" \
        --value "${RABBITMQ_PASSWORD}" \
        --type "SecureString" \
        --overwrite \
        --region ${AWS_REGION} || true

    log_success "AWS infrastructure setup complete"
}

# =============================================================================
# Main
# =============================================================================

main() {
    validate_aws_credentials
    setup_aws_infrastructure

    case "${DEPLOYMENT_TYPE}" in
        ecs)
            deploy_to_ecs
            ;;
        eks)
            deploy_to_eks
            ;;
        *)
            log_error "Invalid deployment type: ${DEPLOYMENT_TYPE}"
            log_error "Valid options: ecs, eks"
            exit 1
            ;;
    esac

    log_success "AWS deployment complete!"
}

main
