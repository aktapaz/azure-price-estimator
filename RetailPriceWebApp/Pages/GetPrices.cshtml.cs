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
