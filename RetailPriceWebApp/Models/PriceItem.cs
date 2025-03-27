using Newtonsoft.Json;

namespace RetailPriceWebApp.Models
{
    public class PriceItem
    {
        [JsonProperty("serviceName")]
        public string ServiceName { get; set; } = string.Empty;

        [JsonProperty("billingCurrency")]
        public string BillingCurrency { get; set; } = string.Empty;

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

        [JsonProperty("armRegionName")]
        public string ArmRegionName { get; set; } = string.Empty;

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; } = string.Empty;

        [JsonProperty("tierMinimumUnits")]
        public decimal TierMinimumUnits { get; set; }

        [JsonProperty("serviceFamily")]
        public string ServiceFamily { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("isPrimaryMeterRegion")]
        public bool IsPrimaryMeterRegion { get; set; }

        [JsonProperty("meterId")]
        public string MeterId { get; set; } = string.Empty;

        [JsonProperty("meterName")]
        public string MeterName { get; set; } = string.Empty;
    }
}