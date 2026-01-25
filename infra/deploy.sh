#!/bin/bash
set -e

# Golf Scheduler Azure Deployment Script
# Usage: ./deploy.sh <environment> <resource-group> <postgres-password>
# Example: ./deploy.sh dev rg-golfscheduler-dev 'MySecurePassword123!'

ENVIRONMENT=${1:-dev}
RESOURCE_GROUP=${2:-"rg-golfscheduler-$ENVIRONMENT"}
POSTGRES_PASSWORD=$3
LOCATION=${4:-"eastus2"}

if [ -z "$POSTGRES_PASSWORD" ]; then
    echo "Error: PostgreSQL password is required"
    echo "Usage: ./deploy.sh <environment> <resource-group> <postgres-password> [location]"
    exit 1
fi

echo "======================================"
echo "Golf Scheduler Azure Deployment"
echo "======================================"
echo "Environment:    $ENVIRONMENT"
echo "Resource Group: $RESOURCE_GROUP"
echo "Location:       $LOCATION"
echo "======================================"

# Check if logged in to Azure
if ! az account show &> /dev/null; then
    echo "Please login to Azure first: az login"
    exit 1
fi

# Create resource group if it doesn't exist
echo "Creating resource group..."
az group create \
    --name "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --output none

# Deploy Bicep template
echo "Deploying infrastructure..."
DEPLOYMENT_OUTPUT=$(az deployment group create \
    --resource-group "$RESOURCE_GROUP" \
    --template-file main.bicep \
    --parameters "parameters.$ENVIRONMENT.bicepparam" \
    --parameters postgresAdminPassword="$POSTGRES_PASSWORD" \
    --query 'properties.outputs' \
    --output json)

# Extract outputs
API_URL=$(echo "$DEPLOYMENT_OUTPUT" | jq -r '.apiAppUrl.value')
WEB_URL=$(echo "$DEPLOYMENT_OUTPUT" | jq -r '.staticWebAppUrl.value')
DEPLOYMENT_TOKEN=$(echo "$DEPLOYMENT_OUTPUT" | jq -r '.staticWebAppDeploymentToken.value')
POSTGRES_FQDN=$(echo "$DEPLOYMENT_OUTPUT" | jq -r '.postgresServerFqdn.value')

echo ""
echo "======================================"
echo "Deployment Complete!"
echo "======================================"
echo "API URL:        $API_URL"
echo "Web URL:        $WEB_URL"
echo "PostgreSQL:     $POSTGRES_FQDN"
echo ""
echo "Static Web App Deployment Token (save this for CI/CD):"
echo "$DEPLOYMENT_TOKEN"
echo ""
echo "Next steps:"
echo "1. Deploy API:  az webapp deploy --resource-group $RESOURCE_GROUP --name api-golfscheduler-$ENVIRONMENT --src-path <publish-folder>"
echo "2. Deploy UI:   Use the deployment token above with GitHub Actions or Azure CLI"
echo "======================================"
