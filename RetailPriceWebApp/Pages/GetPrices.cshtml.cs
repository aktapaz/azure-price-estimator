using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using RetailPriceWebApp.Models;
using System.Net.Http;

namespace RetailPriceWebApp.Pages
{
    public class RetailApiResult
    {
        [JsonProperty("Items")]
        public List<PriceItem> Items { get; set; } = new();
    }

    public class GetPricesModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<GetPricesModel> _logger;

        public GetPricesModel(
            IHttpClientFactory clientFactory,
            IConfiguration config,
            ILogger<GetPricesModel> logger)
        {
            _clientFactory = clientFactory;
            _config = config;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string SkuName { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Region { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string ServiceName { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string BillingCurrency { get; set; } = "EUR";

        [BindProperty(SupportsGet = true)]
        public int UsageHours { get; set; } = 730;

        public List<PriceItem> Prices { get; set; } = new();

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(SkuName) || !string.IsNullOrWhiteSpace(Region) || 
                !string.IsNullOrWhiteSpace(ServiceName))
            {
                try
                {
                    var client = _clientFactory.CreateClient("AzureRetailPrices");
                    
                    var filters = new List<string>();
                    
                    if (!string.IsNullOrWhiteSpace(SkuName))
                        filters.Add($"startswith(skuName,'{SkuName}')");
                    if (!string.IsNullOrWhiteSpace(Region))
                        filters.Add($"location eq '{Region}'");
                    if (!string.IsNullOrWhiteSpace(ServiceName))
                        filters.Add($"serviceName eq '{ServiceName}'");
                    if (!string.IsNullOrWhiteSpace(BillingCurrency))
                        filters.Add($"currencyCode eq '{BillingCurrency}'");

                    var filter = string.Join(" and ", filters);
                    var requestUri = $"https://prices.azure.com/api/retail/prices?$filter={filter}&api-version=2023-01-01-preview";

                    _logger.LogInformation($"Requesting prices with URI: {requestUri}");

                    using var response = await client.GetAsync(requestUri);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"API request failed with status {response.StatusCode}: {errorContent}");
                        ModelState.AddModelError("", $"Failed to retrieve prices. Status: {response.StatusCode}");
                        return;
                    }
                    
                    var jsonString = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug($"Received response: {jsonString}");

                    var result = JsonConvert.DeserializeObject<RetailApiResult>(jsonString);
                    
                    if (result?.Items == null || !result.Items.Any())
                    {
                        ModelState.AddModelError("", "No prices found for the specified criteria.");
                        return;
                    }

                    Prices = result.Items
                        .Where(p => p.RetailPrice > 0)
                        .OrderBy(p => p.ServiceFamily)
                        .ThenBy(p => p.SkuName)
                        .ToList();
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Failed to retrieve prices from Azure Retail Prices API");
                    ModelState.AddModelError("", $"Failed to retrieve prices: {ex.Message}");
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to parse API response");
                    ModelState.AddModelError("", $"Failed to process the price data: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error occurred");
                    ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                }
            }
        }
    }
}