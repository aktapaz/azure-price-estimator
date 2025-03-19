using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RetailPriceWebApp.Models;

namespace RetailPriceWebApp.Pages
{
    private readonly IHttpClientFactory _clientFactory = null!;
    private readonly IConfiguration _config = null!;
    private readonly ILogger<GetPricesModel> _logger = null!;

    public class GetPricesModel(IConfiguration config) : PageModel
    {
        private readonly IConfiguration _config = config;

        [BindProperty(SupportsGet = true)]
        public string SkuName { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string Region { get; set; } = "";

    [BindProperty(SupportsGet = true)]
    public string ServiceType { get; set; } = "";

    [BindProperty(SupportsGet = true)]
    public int UsageHours { get; set; } = 730; // Default to average hours in a month

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

        public List<PriceItem> Prices { get; set; } = new();

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(SkuName) || !string.IsNullOrWhiteSpace(ProductName))

  }
}

namespace RetailPriceWebApp.Models
{
    public class PriceItem
    {
        public string SkuName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal RetailPrice { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
    }

    public class PriceResponse
    {
        public List<PriceItem> Items { get; set; } = new();
    }
}