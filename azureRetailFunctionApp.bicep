@description('Specifies the location for resources.')
param location string = resourceGroup().location

@description('Specifies the name of the Function App.')
param functionAppName string = 'func-${uniqueString(resourceGroup().id)}'

@description('Specifies the name of the storage account.')
param storageAccountName string = 'st${uniqueString(resourceGroup().id)}'

@description('Specifies the name of the App Service Plan.')
param appServicePlanName string = 'plan-${uniqueString(resourceGroup().id)}'

@description('Specifies the SKU of the App Service Plan.')
param appServicePlanSku object = {
  name: 'Y1'
  tier: 'Dynamic'
}

@description('Specifies the Runtime Stack of the Function App.')
param functionRuntime string = 'node'

@description('Specifies the Retail API Subscription Key.')
@secure()
param retailApiSubscriptionKey string

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

resource appServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: appServicePlanName
  location: location
  sku: appServicePlanSku
  properties: {
    reserved: true // Required for Linux
  }
}

resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(functionAppName)
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: functionRuntime
        }
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '~16'
        }
        {
          name: 'RETAIL_API_SUBSCRIPTION_KEY'
          value: retailApiSubscriptionKey
        }
        {
          name: 'RETAIL_API_ENDPOINT'
          value: 'https://api.retail.microsoft.com'
        }
      ]
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
    }
    httpsOnly: true
  }
}

output functionAppName string = functionApp.name
output functionAppDefaultHostName string = functionApp.properties.defaultHostName