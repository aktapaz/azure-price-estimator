@description('Base name for all resources')
param baseName string = 'retailpricing${uniqueString(resourceGroup().id)}'

@description('Location for all resources')
param location string = resourceGroup().location

@description('The SKU of App Service Plan')
param appServicePlanSku object = {
  name: 'B1'
  tier: 'Basic'
  capacity: 1
}

@description('The runtime stack for the web app')
param linuxFxVersion string = 'DOTNETCORE|8.0'

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: '${baseName}-plan'
  location: location
  sku: appServicePlanSku
  kind: 'linux'
  properties: {
    reserved: true
  }
}

// Web App
resource webApp 'Microsoft.Web/sites@2022-09-01' = {
  name: baseName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: linuxFxVersion
      minTlsVersion: '1.2'
      http20Enabled: true
      appSettings: [
        {
          name: 'AZURE_RETAIL_API_ENDPOINT'
          value: 'https://prices.azure.com/api/retail/prices'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: 'true'
        }
      ]
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

// Output the web app URL
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output webAppName string = webApp.name
