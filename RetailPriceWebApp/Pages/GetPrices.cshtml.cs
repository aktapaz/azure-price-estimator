using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Globalization;
using ClosedXML.Excel;
using RetailPriceWebApp.Models;

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
            if (!string.IsNullOrEmpty(Location) && !string.IsNullOrEmpty(ServiceFamily))
            {
                _logger.LogInformation($"Fetching prices for {ServiceFamily} in {Location}");
                
                using var client = _clientFactory.CreateClient();
                var baseUrl = _config["AzurePriceApi:BaseUrl"];
                var filters = new List<string>();

                // Add filters based on selected options
                if (!string.IsNullOrEmpty(Location))
                    filters.Add($"armRegionName eq '{Location}'");
                
                if (!string.IsNullOrEmpty(ServiceFamily))
                    filters.Add($"serviceFamily eq '{ServiceFamily}'");
                
                if (!string.IsNullOrEmpty(ArmSkuName))
                    filters.Add($"armSkuName eq '{ArmSkuName}'");

                var filterString = string.Join(" and ", filters);
                var url = $"{baseUrl}/prices?$filter={Uri.EscapeDataString(filterString)}";

                var response = await client.GetFromJsonAsync<PriceDataResponse>(url);
                if (response?.Items != null)
                {
                    Prices = response.Items;
                    _logger.LogInformation($"Found {Prices.Count} prices");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch prices");
            Prices = new List<PriceItem>();
        }
    }


        public async Task<IActionResult> OnPostExportAsync([FromForm] string[] prices)
        {
            if (prices == null || !prices.Any())
            {
                _logger.LogWarning("No prices provided for export");
                return RedirectToPage("./Error");
            }

            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Azure Prices");

                // Add headers
                var headers = new[] { "Service", "SKU", "Region", "License Type", "Description", "Price/Hour", "Total Cost", "Unit" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                }

                // Add data
                var row = 2;
                foreach (var priceJson in prices)
                {
                    var price = JsonConvert.DeserializeObject<PriceItem>(priceJson);
                    if (price == null) continue;

                    (string licenseType, string description, decimal totalCost) = GetPriceDetails(price);

                    worksheet.Cell(row, 1).Value = price.ServiceName;
                    worksheet.Cell(row, 2).Value = price.SkuName;
                    worksheet.Cell(row, 3).Value = price.Location;
                    worksheet.Cell(row, 4).Value = licenseType;
                    worksheet.Cell(row, 5).Value = description;
                    worksheet.Cell(row, 6).Value = price.RetailPrice;
                    worksheet.Cell(row, 7).Value = totalCost;
                    worksheet.Cell(row, 8).Value = price.UnitOfMeasure;
                    row++;
                }

                FormatWorksheet(worksheet);

                return await GenerateExcelFile(workbook);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate Excel export");
                return RedirectToPage("./Error");
            }
        }

        private (string licenseType, string description, decimal totalCost) GetPriceDetails(PriceItem price)
        {
            if (price.ServiceName.Contains("Storage", StringComparison.OrdinalIgnoreCase))
            {
                return ("Storage", 
                       $"{price.SkuName} storage with {StorageSize:N0} GB capacity", 
                       price.RetailPrice * (StorageSize ?? 1));
            }
            
            if (price.SkuName.Contains("3 Year", StringComparison.OrdinalIgnoreCase))
            {
                return ("Reserved Instance (3 Years)", 
                       "3 year commitment with upfront payment required", 
                       price.RetailPrice * UsageHours);
            }
            
            if (price.SkuName.Contains("1 Year", StringComparison.OrdinalIgnoreCase))
            {
                return ("Reserved Instance (1 Year)", 
                       "1 year commitment with upfront payment required", 
                       price.RetailPrice * UsageHours);
            }

            return ("Pay-As-You-Go", 
                   "No commitment required", 
                   price.RetailPrice * UsageHours);
        }

        private void FormatWorksheet(IXLWorksheet worksheet)
        {
            // Format headers
            var headerRange = worksheet.Range(1, 1, 1, 8);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Format price columns
            var culture = new CultureInfo(CurrencyCode switch
            {
                "EUR" => "fr-FR",
                "GBP" => "en-GB",
                "USD" => "en-US",
                _ => "fr-FR"
            });

            worksheet.Column(6).Style.NumberFormat.Format = $"{culture.NumberFormat.CurrencySymbol}#,##0.0000";
            worksheet.Column(7).Style.NumberFormat.Format = $"{culture.NumberFormat.CurrencySymbol}#,##0.00";

            worksheet.Columns().AdjustToContents();
        }

        private async Task<IActionResult> GenerateExcelFile(XLWorkbook workbook)
        {
            using var stream = new MemoryStream();
            await Task.Run(() => workbook.SaveAs(stream));
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"azure-prices-{DateTime.Now:yyyy-MM-dd}.xlsx"
            );
        }

        public class PriceDataResponse
        {
            [JsonProperty("items")]
            public List<PriceItem> Items { get; set; } = new();
        }
    }
}