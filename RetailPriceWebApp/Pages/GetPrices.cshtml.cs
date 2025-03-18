using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RetailPriceWebApp.Models;

namespace RetailPriceWebApp.Pages
{
    public class GetPricesModel(IConfiguration config) : PageModel
    {
        private readonly IConfiguration _config = config;

        [BindProperty(SupportsGet = true)]
        public string SkuName { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string ProductName { get; set; } = string.Empty;

        public List<PriceItem> Prices { get; set; } = new();

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrWhiteSpace(SkuName) || !string.IsNullOrWhiteSpace(ProductName))
            {
                var endpoint = _config["AZURE_RETAIL_API_ENDPOINT"] ?? "https://prices.azure.com/api/retail/prices";
                var filters = new List<string>();
                
                if (!string.IsNullOrWhiteSpace(SkuName))
                    filters.Add($"startswith(skuName,'{SkuName}')");
                if (!string.IsNullOrWhiteSpace(ProductName))
                    filters.Add($"contains(productName,'{ProductName}')");

                var filter = string.Join(" and ", filters);
                var requestUri = $"{endpoint}?$filter={filter}&api-version=2023-03-01-preview";

                using var client = new HttpClient();
                var response = await client.GetFromJsonAsync<PriceResponse>(requestUri);
                if (response?.Items != null)
                {
                    Prices = response.Items;
                }
            }
        }

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