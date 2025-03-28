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
        public string SkuId { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string ProductName { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string ServiceId { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string PriceType { get; set; } = "Consumption";

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
            try
            {
                var client = _clientFactory.CreateClient("AzureRetailPrices");
                var filters = new List<string>();

                if (!string.IsNullOrWhiteSpace(SkuId))
                    filters.Add($"skuId eq '{SkuId}'");
                if (!string.IsNullOrWhiteSpace(Location))
                    filters.Add($"armRegionName eq '{Location}'");
                if (!string.IsNullOrWhiteSpace(ServiceName))
                    filters.Add($"contains(serviceName, '{ServiceName}')");
                
                // Build the API request and await the response
                string filterQuery = string.Join(" and ", filters);
                string requestUri = $"?$filter={filterQuery}&currencyCode='{CurrencyCode}'";
                var response = await client.GetAsync(requestUri);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var priceData = JsonConvert.DeserializeObject<PriceDataResponse>(content);
                    Prices = priceData?.Items ?? new List<PriceItem>();
                }
                // ...rest of the filter conditions...
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve prices");
                ModelState.AddModelError("", "Failed to retrieve prices. Please try again later.");
            }
        }
    }
}