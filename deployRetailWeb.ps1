<#
    .SYNOPSIS
    Deploys a .NET 8 WebApp that can retrieve prices from the Azure Retail API.

    .NOTES
    - Requires Azure CLI, PowerShell, and .NET 8 SDK installed.
    - Run in PowerShell.
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$subscriptionId,

    [Parameter(Mandatory=$true)]
    [string]$resourceGroupName,

    [Parameter(Mandatory=$true)]
    [string]$location,

    [Parameter(Mandatory=$true)]
    [string]$webAppName
)

# 1) Sign in and set subscription
az login
az account set --subscription $subscriptionId

# 2) Create resource group
az group create --name $resourceGroupName --location $location

# 3) Create an App Service Plan
az appservice plan create `
  --name "$($webAppName)-plan" `
  --resource-group $resourceGroupName `
  --location $location `
  --sku B1 `
  --is-linux

# 4) Create the Web App
az webapp create `
  --name $webAppName `
  --resource-group $resourceGroupName `
  --plan "$($webAppName)-plan" `
  --runtime "DOTNETCORE:8.0"

# 5) Configure application settings (Retail API endpoint)
az webapp config appsettings set `
  --name $webAppName `
  --resource-group $resourceGroupName `
  --settings AZURE_RETAIL_API_ENDPOINT="https://prices.azure.com/api/retail/prices"

# 6) Create a .NET 8 web app project (locally)
Write-Host "Creating sample .NET project..."
mkdir "RetailPriceWebApp" -ErrorAction Ignore | Out-Null
Push-Location "RetailPriceWebApp"

dotnet new webapp --framework net8.0
dotnet add package Microsoft.Extensions.Http
dotnet add package Newtonsoft.Json

# Replace Program.cs with sample code for retrieving prices
Remove-Item Program.cs
@"
//// filepath: RetailPriceWebApp/Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
"@ | Out-File Program.cs -Encoding utf8

# Create GetPrices page
mkdir Pages -ErrorAction Ignore | Out-Null
@"
//// filepath: RetailPriceWebApp/Pages/GetPrices.cshtml
@page
@model GetPricesModel
@{
    ViewData["Title"] = "Get Azure Retail Prices";
}

<h1>Get Azure Retail Prices</h1>
<form method=""get"">
    <label for=""skuName"">SKU Name:</label>
    <input type=""text"" id=""skuName"" name=""skuName"" value=""@Model.SkuName"" />
    <button type=""submit"">Get Prices</button>
</form>

@if (Model.Prices != null && Model.Prices.Any())
{
    <table class=""table"">
        <thead>
            <tr>
                <th>ProductName</th>
                <th>MeterName</th>
                <th>RetailPrice</th>
                <th>UnitOfMeasure</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var item in Model.Prices)
            {
                <tr>
                    <td>@item.productName</td>
                    <td>@item.meterName</td>
                    <td>@item.retailPrice</td>
                    <td>@item.unitOfMeasure</td>
                </tr>
            }
        </tbody>
    </table>
}
else if (!string.IsNullOrEmpty(Model.SkuName))
{
    <p>No prices found.</p>
}
"@ | Out-File .\Pages\GetPrices.cshtml -Encoding utf8

@"
//// filepath: RetailPriceWebApp/Pages/GetPrices.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

public class GetPricesModel : PageModel
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<GetPricesModel> _logger;

    public GetPricesModel(IHttpClientFactory clientFactory, IConfiguration config, ILogger<GetPricesModel> logger)
    {
        _clientFactory = clientFactory;
        _config = config;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string SkuName { get; set; }

    public List<PriceItem> Prices { get; set; } = new();

    public async Task OnGet()
    {
        if (!string.IsNullOrWhiteSpace(SkuName))
        {
            var endpoint = _config["AZURE_RETAIL_API_ENDPOINT"] ?? "https://prices.azure.com/api/retail/prices";
            var filter = $"startswith(skuName,'{SkuName}')";
            var requestUri = $"{endpoint}?`$filter={filter}&api-version=2023-03-01-preview";

            var client = _clientFactory.CreateClient();
            try
            {
                var response = await client.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<RetailApiResult>(jsonString);
                Prices = result?.Items ?? new List<PriceItem>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, ""API request failed"");
            }
        }
    }
}

public class RetailApiResult
{
    [JsonProperty(""-Items"")]
    public List<PriceItem> Items { get; set; }
}

public class PriceItem
{
    [JsonProperty(""productName"")]
    public string productName { get; set; }

    [JsonProperty(""meterName"")]
    public string meterName { get; set; }

    [JsonProperty(""retailPrice"")]
    public decimal retailPrice { get; set; }

    [JsonProperty(""unitOfMeasure"")]
    public string unitOfMeasure { get; set; }
}
"@ | Out-File .\Pages\GetPrices.cshtml.cs -Encoding utf8

# Optional: add a link in the default index page
Add-Content -Value "`n<a href=""/GetPrices"">Get Azure Retail Prices</a>`n" -Path .\Pages\Index.cshtml

# 7) Publish locally to a folder
dotnet publish -c Release -o publish

# 8) Deploy to Azure
az webapp deploy `
  --resource-group $resourceGroupName `
  --name $webAppName `
  --src-path ".\publish" `
  --clean true

Pop-Location

Write-Host "Deployment complete. Visit your web app at https://$($webAppName).azurewebsites.net/GetPrices"