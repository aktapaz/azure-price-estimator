@description('The name of the Function App')
param functionAppName string = 'az-retail-prices'

@description('The name of the App Service Plan')
param appServicePlanName string = 'az-retail-prices-plan'

@description('The location where the resources will be deployed')
param location string = resourceGroup().location

@description('The name of the Storage Account')
param storageAccountName string = 'azretailpricesstor'

@description('The name of the Application Insights resource')
param appInsightsName string = 'az-retail-prices-insights'

@description('The name of the Static Web App')
param staticWebAppName string = 'az-retail-prices-web'

@description('The location for the Static Web App')
param staticWebAppLocation string = 'westeurope'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
  }
}

resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

resource storageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobServices
  name: 'blobstore'
  properties: {
    publicAccess: 'Blob' // Allow anonymous access to blobs
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'S1'
    tier: 'Standard'
  }
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux' // Specify that the Function App is running on Linux
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: storageAccount.properties.primaryEndpoints.blob
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsights.properties.InstrumentationKey
        }
        {
          name: 'AZURE_RETAIL_API_PRICES_ENDPOINT'
          value: 'https://prices.azure.com/api/retail/prices'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: 'https://occdeploymentstorage.blob.core.windows.net/files/price-estimate.zip' // URL of the ZIP file
        }
      ]
      linuxFxVersion: 'DOTNET|6.0' // Specify the runtime stack for Linux
    }
  }
  dependsOn: [
    #disable-next-line no-unnecessary-dependson
    storageAccount
    storageContainer
    #disable-next-line no-unnecessary-dependson
    appServicePlan
    #disable-next-line no-unnecessary-dependson
    applicationInsights
  ]
}

resource functionAppHttpTrigger 'Microsoft.Web/sites/functions@2024-04-01' = {
  parent: functionApp
  name: 'GetRetailPrices'
  properties: {
    config: {
      bindings: [
        {
          name: 'req'
          type: 'httpTrigger'
          direction: 'in'
          authLevel: 'function'
          methods: [
            'get'
          ]
        }
        {
          name: 'res'
          type: 'http'
          direction: 'out'
        }
      ]
    }
    files: {
      'run.csx': '''
#r "Newtonsoft.Json"

using System.Net;
using Newtonsoft.Json;
using System.Net.Http;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, ILogger log)
{
    string endpoint = Environment.GetEnvironmentVariable("AZURE_RETAIL_API_PRICES_ENDPOINT");
    using (HttpClient client = new HttpClient())
    {
        HttpResponseMessage response = await client.GetAsync(endpoint);
        string content = await response.Content.ReadAsStringAsync();
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
        };
    }
}
'''
    }
  }
}

resource staticWebApp 'Microsoft.Web/staticSites@2024-04-01' = {
  name: staticWebAppName
  location: staticWebAppLocation
  sku: {
    name: 'Free' 
    tier: 'Free'
  }
  properties: {
    repositoryUrl: 'https://github.com/aktapaz/azure-price-estimator'
    branch: 'devtest'
    buildProperties: {
      appLocation: '/'
      apiLocation: 'api'
    }
  }
  dependsOn: [
    functionApp
  ]
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(storageAccount.id, 'Storage Blob Data Reader', functionApp.id)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe') // Storage Blob Data Reader role
    principalId: functionApp.identity.principalId
  }
}

output functionAppDefaultHostName string = functionApp.properties.defaultHostName
output staticWebAppDefaultHostName string = staticWebApp.properties.defaultHostname
