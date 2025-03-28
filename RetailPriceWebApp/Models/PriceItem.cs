using Newtonsoft.Json;

namespace RetailPriceWebApp.Models
{
    public class PriceItem
    {
        [JsonProperty("serviceName")]
        public string ServiceName { get; set; } = string.Empty;

        [JsonProperty("skuId")]
        public string SkuId { get; set; } = string.Empty;

        [JsonProperty("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonProperty("serviceId")]
        public string ServiceId { get; set; } = string.Empty;

        [JsonProperty("skuName")]
        public string SkuName { get; set; } = string.Empty;

        [JsonProperty("priceType")]
        public string PriceType { get; set; } = string.Empty;

        [JsonProperty("armSkuName")]
        public string ArmSkuName { get; set; } = string.Empty;

        [JsonProperty("location")]
        public string Location { get; set; } = string.Empty;

        [JsonProperty("retailPrice")]
        public decimal RetailPrice { get; set; }
        [JsonProperty("meterId")]
        public string MeterId { get; set; } = string.Empty;

        [JsonProperty("meterName")]
        public string MeterName { get; set; } = string.Empty;
    }
}