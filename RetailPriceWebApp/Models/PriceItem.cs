namespace RetailPriceWebApp.Models
{
    public class AzurePriceItem
    {
        public string ProductName { get; set; } = string.Empty;
        public string MeterName { get; set; } = string.Empty;
        public decimal RetailPrice { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
    }
}