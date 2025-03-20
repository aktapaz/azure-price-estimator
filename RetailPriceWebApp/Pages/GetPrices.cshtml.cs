using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace RetailPriceWebApp.Pages
{
    public class PriceItem
    {
        [JsonProperty("productName")]
        public string productName { get; set; } = "";

        [JsonProperty("meterName")]
        public string meterName { get; set; } = "";

        [JsonProperty("retailPrice")]
        public decimal retailPrice { get; set; }

        [JsonProperty("unitOfMeasure")]
        public string unitOfMeasure { get; set; } = "";

        [JsonProperty("location")]
        public string location { get; set; } = "";

        [JsonProperty("serviceFamily")]
        public string serviceFamily { get; set; } = "";
    }

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

        // Constructor with dependency injection
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

        // Single declaration of Prices property
        public List<PriceItem> Prices { get; set; } = new();

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(SkuName) || !string.IsNullOrWhiteSpace(Region) || !string.IsNullOrWhiteSpace(ServiceType))
            {
                var endpoint = _config["AZURE_RETAIL_API_ENDPOINT"] ?? "https://prices.azure.com/api/retail/prices";
                var filters = new List<string>();
                
                if (!string.IsNullOrWhiteSpace(SkuName))
                    filters.Add($"startswith(skuName,'{SkuName}')");
                if (!string.IsNullOrWhiteSpace(Region))
                    filters.Add($"location eq '{Region}'");
                if (!string.IsNullOrWhiteSpace(ServiceType))
                    filters.Add($"serviceFamily eq '{ServiceType}'");

                var filter = string.Join(" and ", filters);
                var requestUri = $"{endpoint}?$filter={filter}&api-version=2023-03-01-preview";

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
                    _logger.LogError(ex, "API request failed");
                }
            }
        }
    }
}