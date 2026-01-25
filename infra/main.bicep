// Golf Scheduler Azure Infrastructure
// Provisions: App Service Plan, Web App (API), Static Web App (UI), PostgreSQL

@description('Environment name (dev, staging, prod)')
@allowed(['dev', 'staging', 'prod'])
param environment string = 'dev'

@description('Azure region for resources')
param location string = resourceGroup().location

@description('Base name for resources')
param appName string = 'golfscheduler'

@description('PostgreSQL administrator login')
param postgresAdminLogin string = 'pgadmin'

@description('PostgreSQL administrator password')
@secure()
param postgresAdminPassword string

@description('Azure AD B2C tenant name')
param b2cTenantName string = ''

@description('Azure AD B2C client ID')
param b2cClientId string = ''

// Resource naming
var resourceSuffix = '${appName}-${environment}'
var appServicePlanName = 'asp-${resourceSuffix}'
var apiAppName = 'api-${resourceSuffix}'
var staticWebAppName = 'web-${resourceSuffix}'
var postgresServerName = 'psql-${resourceSuffix}'
var postgresDatabaseName = 'golfscheduler'

// App Service Plan (Linux, Basic B1)
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  kind: 'linux'
  sku: {
    name: environment == 'prod' ? 'S1' : 'B1'
    tier: environment == 'prod' ? 'Standard' : 'Basic'
  }
  properties: {
    reserved: true // Required for Linux
  }
}

// API App Service (.NET 9)
resource apiApp 'Microsoft.Web/sites@2023-01-01' = {
  name: apiAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      alwaysOn: environment == 'prod'
      minTlsVersion: '1.2'
      cors: {
        allowedOrigins: [
          'https://${staticWebAppName}.azurestaticapps.net'
          environment == 'dev' ? 'http://localhost:5173' : ''
        ]
        supportCredentials: true
      }
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment == 'prod' ? 'Production' : 'Development'
        }
        {
          name: 'FrontendUrl'
          value: 'https://${staticWebAppName}.azurestaticapps.net'
        }
        {
          name: 'AzureAdB2C__Instance'
          value: b2cTenantName != '' ? 'https://${b2cTenantName}.b2clogin.com/' : ''
        }
        {
          name: 'AzureAdB2C__Domain'
          value: b2cTenantName != '' ? '${b2cTenantName}.onmicrosoft.com' : ''
        }
        {
          name: 'AzureAdB2C__ClientId'
          value: b2cClientId
        }
        {
          name: 'BypassAuthentication'
          value: environment == 'dev' ? 'true' : 'false'
        }
      ]
      connectionStrings: [
        {
          name: 'DefaultConnection'
          connectionString: 'Host=${postgresServer.properties.fullyQualifiedDomainName};Database=${postgresDatabaseName};Username=${postgresAdminLogin};Password=${postgresAdminPassword};SSL Mode=Require;Trust Server Certificate=true'
          type: 'PostgreSQL'
        }
      ]
    }
  }
}

// Static Web App (Vue.js Frontend)
resource staticWebApp 'Microsoft.Web/staticSites@2023-01-01' = {
  name: staticWebAppName
  location: location
  sku: {
    name: environment == 'prod' ? 'Standard' : 'Free'
    tier: environment == 'prod' ? 'Standard' : 'Free'
  }
  properties: {
    buildProperties: {
      appLocation: 'src/golf-scheduler-ui'
      outputLocation: 'dist'
      appBuildCommand: 'npm run build'
    }
  }
}

// Static Web App settings
resource staticWebAppSettings 'Microsoft.Web/staticSites/config@2023-01-01' = {
  parent: staticWebApp
  name: 'appsettings'
  properties: {
    VITE_API_URL: 'https://${apiApp.properties.defaultHostName}'
    VITE_BYPASS_AUTH: environment == 'dev' ? 'true' : 'false'
    VITE_B2C_CLIENT_ID: b2cClientId
    VITE_B2C_AUTHORITY: b2cTenantName != '' ? 'https://${b2cTenantName}.b2clogin.com/${b2cTenantName}.onmicrosoft.com/B2C_1_signupsignin' : ''
    VITE_B2C_KNOWN_AUTHORITIES: b2cTenantName != '' ? '${b2cTenantName}.b2clogin.com' : ''
    VITE_B2C_REDIRECT_URI: 'https://${staticWebAppName}.azurestaticapps.net'
  }
}

// PostgreSQL Flexible Server
resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' = {
  name: postgresServerName
  location: location
  sku: {
    name: environment == 'prod' ? 'Standard_B2s' : 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: postgresAdminLogin
    administratorLoginPassword: postgresAdminPassword
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: environment == 'prod' ? 14 : 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      publicNetworkAccess: 'Enabled'
    }
  }
}

// PostgreSQL Database
resource postgresDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-03-01-preview' = {
  parent: postgresServer
  name: postgresDatabaseName
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

// PostgreSQL Firewall - Allow Azure Services
resource postgresFirewallAzure 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-03-01-preview' = {
  parent: postgresServer
  name: 'AllowAllAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '255.255.255.255'
  }
}

// Outputs
output apiAppUrl string = 'https://${apiApp.properties.defaultHostName}'
output staticWebAppUrl string = 'https://${staticWebApp.properties.defaultHostname}'
output staticWebAppDeploymentToken string = staticWebApp.listSecrets().properties.apiKey
output postgresServerFqdn string = postgresServer.properties.fullyQualifiedDomainName
