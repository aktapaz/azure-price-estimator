using Newtonsoft.Json;

namespace RetailPriceWebApp.Models
{
    public class PriceItem
    {
        [JsonProperty("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonProperty("skuName")]
        public string SkuName { get; set; } = string.Empty;

        [JsonProperty("location")]
        public string Location { get; set; } = string.Empty;

        [JsonProperty("retailPrice")]
        public decimal RetailPrice { get; set; }

        [JsonProperty("unitOfMeasure")]
        public string UnitOfMeasure { get; set; } = string.Empty;

        [JsonProperty("serviceFamily")]
        public string ServiceFamily { get; set; } = string.Empty;
    }
}