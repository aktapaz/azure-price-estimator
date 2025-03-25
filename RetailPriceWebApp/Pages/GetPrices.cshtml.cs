using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using RetailPriceWebApp.Models;
using System.Net.Http;

namespace RetailPriceWebApp.Pages
{
    public class RetailApiResult
    {
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
        public string SkuName { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string Region { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string ServiceType { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public int UsageHours { get; set; } = 730;

        public List<PriceItem> Prices { get; set; } = new();

                public async Task OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(SkuName) || !string.IsNullOrWhiteSpace(Region) || !string.IsNullOrWhiteSpace(ServiceType))
            {
                try
                {
                    var client = _clientFactory.CreateClient();
                    client.BaseAddress = new Uri("https://prices.azure.com/");
                    
                    var filters = new List<string>();
                    
                    if (!string.IsNullOrWhiteSpace(SkuName))
                        filters.Add($"startswith(skuName,'{SkuName}')");
                    if (!string.IsNullOrWhiteSpace(Region))
                        filters.Add($"location eq '{Region}'");
                    if (!string.IsNullOrWhiteSpace(ServiceType))
                        filters.Add($"serviceFamily eq '{ServiceType}'");

                    var filter = string.Join(" and ", filters);
                    var requestUri = $"api/retail/prices?$filter={filter}&api-version=2023-03-01-preview";

                    _logger.LogInformation($"Requesting prices with URI: {requestUri}");

                    var response = await client.GetAsync(requestUri);
                    response.EnsureSuccessStatusCode();
                    
                    var jsonString = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug($"Received response: {jsonString}");

                    var result = JsonConvert.DeserializeObject<RetailApiResult>(jsonString);
                    
                    if (result?.Items == null || !result.Items.Any())
                    {
                        ModelState.AddModelError("", "No prices found for the specified criteria.");
                        return;
                    }

                    Prices = result.Items;
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
            }
        }
    }
}