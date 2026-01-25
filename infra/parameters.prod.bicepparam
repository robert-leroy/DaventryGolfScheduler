using './main.bicep'

param environment = 'prod'
param appName = 'golfscheduler'
param postgresAdminLogin = 'pgadmin'
param postgresAdminPassword = '' // Set via CLI or Key Vault reference

// Azure AD B2C settings (required for production)
param b2cTenantName = '' // e.g., 'yourtenantname'
param b2cClientId = ''   // e.g., 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx'
