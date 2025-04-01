using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using RetailPriceWebApp.Models;
using System.Net.Http;

namespace RetailPriceWebApp.Pages
{
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
        public int? StorageSize { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StorageRedundancy { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public decimal RetailPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Location { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string ServiceName { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string ServiceFamily { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string ArmSkuName { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int UsageHours { get; set; } = 730;

        public List<PriceItem> Prices { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                var client = _clientFactory.CreateClient("AzureRetailPrices");
                var filters = new List<string>();

                if (!string.IsNullOrWhiteSpace(Location))
                    filters.Add($"armRegionName eq '{Location}'");
                if (!string.IsNullOrWhiteSpace(ServiceName))
                    filters.Add($"contains(serviceName, '{ServiceName}')");
                if (!string.IsNullOrWhiteSpace(ServiceFamily))
                    filters.Add($"contains(serviceFamily, '{ServiceFamily}')");
                if (!string.IsNullOrWhiteSpace(ArmSkuName))
                    filters.Add($"armSkuName eq '{ArmSkuName}'");

                var filter = string.Join(" and ", filters);
                var requestUri = $"api/retail/prices?$filter={filter}&api-version=2023-01-01-preview";

                _logger.LogInformation($"Requesting prices with URI: {requestUri}");

                using var response = await client.GetAsync(requestUri);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var priceData = JsonConvert.DeserializeObject<PriceDataResponse>(content);
                    Prices = priceData?.Items ?? new List<PriceItem>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve prices");
                ModelState.AddModelError("", "Failed to retrieve prices. Please try again later.");
            }
        }
    }
}