namespace MkmApi.Entities
{
    public class PriceGuide
    {
        public decimal? PriceSell { get; set; }
        public decimal? PriceLow { get; set; }
        public decimal? PriceLowEx { get; set; }
        public decimal? PriceLowFoil { get; set; }
        public decimal? PriceAverage { get; set; }
        public decimal? PriceTrend { get; set; }
        public decimal? PriceTrendFoil { get; set; }
    }
}