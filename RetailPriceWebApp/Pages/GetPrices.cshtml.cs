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

        public GetPricesModel(IHttpClientFactory clientFactory, IConfiguration config, ILogger<GetPricesModel> logger)
        {
            _clientFactory = clientFactory;
            _config = config;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string CurrencyCode { get; set; } = "EUR";

        [BindProperty(SupportsGet = true)]
        public decimal RetailPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Location { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string ServiceName { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string ServiceFamily { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string UnitOfMeasure { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Type { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string ArmSkuName { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int UsageHours { get; set; } = 730;

        public List<PriceItem> Prices { get; set; } = new();

 public async Task OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(Location) || !string.IsNullOrWhiteSpace(ServiceName))
            {
                try
                {
                    var client = _clientFactory.CreateClient("AzureRetailPrices");
                    var filters = new List<string>();
                    
                    if (!string.IsNullOrWhiteSpace(SkuId))
                        filters.Add($"skuId eq '{SkuId}'");
                    if (!string.IsNullOrWhiteSpace(ProductName))
                        filters.Add($"contains(productName, '{ProductName}')");
                    if (!string.IsNullOrWhiteSpace(ServiceId))
                        filters.Add($"serviceId eq '{ServiceId}'");
                    if (!string.IsNullOrWhiteSpace(PriceType))
                        filters.Add($"priceType eq '{PriceType}'");
                    if (!string.IsNullOrWhiteSpace(Location))
                        filters.Add($"location eq '{Location}'");
                    if (!string.IsNullOrWhiteSpace(ServiceName))
                        filters.Add($"serviceName eq '{ServiceName}'");
                    if (!string.IsNullOrWhiteSpace(ServiceFamily))
                        filters.Add($"serviceFamily eq '{ServiceFamily}'");
                    if (!string.IsNullOrWhiteSpace(Type))
                        filters.Add($"type eq '{Type}'");
                    if (!string.IsNullOrWhiteSpace(ArmSkuName))
                        filters.Add($"armSkuName eq '{ArmSkuName}'");
                    if (!string.IsNullOrWhiteSpace(CurrencyCode))
                        filters.Add($"currencyCode eq '{CurrencyCode}'");

                    var filter = string.Join(" and ", filters);
                    var requestUri = $"api/retail/prices?$filter={filter}&api-version=2023-01-01-preview";
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
                    var result = JsonConvert.DeserializeObject<RetailApiResult>(jsonString);

                    if (result?.Items == null || !result.Items.Any())
                    {
                        ModelState.AddModelError("", "No prices found for the specified criteria.");
                        return;
                    }

                    Prices = result.Items
                        .Where(p => p.RetailPrice > 0)
                        .OrderBy(p => p.ServiceFamily)
                        .ThenBy(p => p.ServiceName)
                        .ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to retrieve prices");
                    ModelState.AddModelError("", "Failed to retrieve prices. Please try again later.");
                }
            }
        }
    }
}