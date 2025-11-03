#!/bin/bash

###############################################################################
# API Gateway Auto-Registration Script
###############################################################################
# Automatically registers new microservice in API Gateway configuration
# Supports: Service discovery, load balancing, health checks, rate limiting
###############################################################################

set -e

# Color codes
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

SERVICE_NAME=$1

if [[ -z "$SERVICE_NAME" ]]; then
    echo -e "${YELLOW}[GATEWAY]${NC} Usage: $0 <ServiceName>"
    exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$(dirname "$SCRIPT_DIR")")"
GATEWAY_DIR="$PROJECT_ROOT/src/ApiGateway/TaskFlow.Gateway"

echo -e "${BLUE}[GATEWAY]${NC} Registering $SERVICE_NAME in API Gateway..."

# Determine service port (5001 + service count)
SERVICE_COUNT=$(find "$PROJECT_ROOT/src/Services" -mindepth 1 -maxdepth 1 -type d | wc -l)
SERVICE_PORT=$((5000 + SERVICE_COUNT))

SERVICE_NAME_LOWER=$(echo "$SERVICE_NAME" | tr '[:upper:]' '[:lower:]')

# Update appsettings.Development.json
update_development_config() {
    local config_file="$GATEWAY_DIR/appsettings.Development.json"

    echo -e "${BLUE}[GATEWAY]${NC} Updating Development configuration..."

    # Check if service already registered
    if grep -q "\"${SERVICE_NAME_LOWER}-service\"" "$config_file"; then
        echo -e "${YELLOW}[GATEWAY]${NC} Service already registered in Development config"
        return
    fi

    # Create backup
    cp "$config_file" "${config_file}.bak"

    # Add service to ReverseProxy.Clusters using jq if available
    if command -v jq &> /dev/null; then
        jq ".ReverseProxy.Clusters += {
            \"${SERVICE_NAME_LOWER}-service\": {
                \"Destinations\": {
                    \"primary\": {
                        \"Address\": \"http://localhost:${SERVICE_PORT}\"
                    }
                }
            }
        }" "$config_file" > "${config_file}.tmp" && mv "${config_file}.tmp" "$config_file"
    else
        # Fallback: manual edit (less reliable)
        echo -e "${YELLOW}[GATEWAY]${NC} jq not found, using manual edit (install jq for better results)"

        # Find the last cluster entry and add new one
        sed -i "/\"task-service\":/a\\
      ,\\
      \"${SERVICE_NAME_LOWER}-service\": {\\
        \"Destinations\": {\\
          \"primary\": {\\
            \"Address\": \"http://localhost:${SERVICE_PORT}\"\\
          }\\
        }\\
      }" "$config_file"
    fi

    echo -e "${GREEN}[GATEWAY]${NC} Development config updated"
}

# Update appsettings.Azure.Production.json (Docker/Kubernetes)
update_production_config() {
    local config_file="$GATEWAY_DIR/appsettings.Azure.Production.json"

    echo -e "${BLUE}[GATEWAY]${NC} Updating Production configuration..."

    if [[ ! -f "$config_file" ]]; then
        echo -e "${YELLOW}[GATEWAY]${NC} Production config not found, skipping"
        return
    fi

    # Check if service already registered
    if grep -q "\"${SERVICE_NAME_LOWER}-service\"" "$config_file"; then
        echo -e "${YELLOW}[GATEWAY]${NC} Service already registered in Production config"
        return
    fi

    # Create backup
    cp "$config_file" "${config_file}.bak"

    # Add service with Consul service discovery
    if command -v jq &> /dev/null; then
        jq ".ReverseProxy.Clusters += {
            \"${SERVICE_NAME_LOWER}-service\": {
                \"Destinations\": {
                    \"primary\": {
                        \"Address\": \"http://${SERVICE_NAME_LOWER}-service:80\"
                    }
                },
                \"HealthCheck\": {
                    \"Active\": {
                        \"Enabled\": true,
                        \"Interval\": \"00:00:30\",
                        \"Timeout\": \"00:00:10\",
                        \"Path\": \"/health\"
                    }
                },
                \"LoadBalancingPolicy\": \"RoundRobin\"
            }
        }" "$config_file" > "${config_file}.tmp" && mv "${config_file}.tmp" "$config_file"
    fi

    echo -e "${GREEN}[GATEWAY]${NC} Production config updated"
}

# Create route configuration
create_route_config() {
    echo -e "${BLUE}[GATEWAY]${NC} Creating route configuration..."

    local routes_dir="$GATEWAY_DIR/Routes"
    mkdir -p "$routes_dir"

    cat > "$routes_dir/${SERVICE_NAME}Routes.json" <<EOF
{
  "Routes": {
    "${SERVICE_NAME_LOWER}-route": {
      "ClusterId": "${SERVICE_NAME_LOWER}-service",
      "Match": {
        "Path": "/api/${SERVICE_NAME_LOWER}/{**catch-all}"
      },
      "Transforms": [
        {
          "PathRemovePrefix": "/api/${SERVICE_NAME_LOWER}"
        },
        {
          "PathPrefix": "/api"
        }
      ],
      "RateLimiterPolicy": "default"
    },
    "${SERVICE_NAME_LOWER}-health": {
      "ClusterId": "${SERVICE_NAME_LOWER}-service",
      "Match": {
        "Path": "/health/${SERVICE_NAME_LOWER}"
      },
      "Transforms": [
        {
          "PathSet": "/health"
        }
      ]
    }
  }
}
EOF

    echo -e "${GREEN}[GATEWAY]${NC} Route configuration created"
}

# Main execution
main() {
    update_development_config
    update_production_config
    create_route_config

    echo -e "${GREEN}[GATEWAY]${NC} Service registered successfully!"
    echo -e "${BLUE}[GATEWAY]${NC} "
    echo -e "${BLUE}[GATEWAY]${NC} Service Endpoints:"
    echo -e "${BLUE}[GATEWAY]${NC}   - Development: http://localhost:5000/api/${SERVICE_NAME_LOWER}"
    echo -e "${BLUE}[GATEWAY]${NC}   - Health: http://localhost:5000/health/${SERVICE_NAME_LOWER}"
    echo -e "${BLUE}[GATEWAY]${NC}   - Direct: http://localhost:${SERVICE_PORT}/api"
}

main
