using Newtonsoft.Json;
using System.Collections.Generic;

namespace RetailPriceWebApp.Models
{
    public class PriceDataResponse
    {
        [JsonProperty("Items")]
        public List<PriceItem> Items { get; set; } = new();

        [JsonProperty("Count")]
        public int Count { get; set; }

        [JsonProperty("NextPageLink")]
        public string? NextPageLink { get; set; }
    }
}