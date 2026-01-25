using './main.bicep'

param environment = 'dev'
param appName = 'golfscheduler'
param postgresAdminLogin = 'pgadmin'
param postgresAdminPassword = '' // Set via CLI: --parameters postgresAdminPassword='<password>'

// Optional: Azure AD B2C settings (leave empty to use bypass auth in dev)
param b2cTenantName = ''
param b2cClientId = ''
