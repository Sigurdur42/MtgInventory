using System;

namespace MtgJson.Database
{
    public class DbPriceItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CardId { get; set; } = Guid.Empty;

        public string Type { get; set; } = "";
        public string Currency { get; set; } = "";
        public string Seller { get; set; } = "";
        public string PaperOrOnline { get; set; } = "";

        public string BuylistOrRetail { get; set; } = "";
        public bool IsFoil { get; set; }
        public string Date { get; set; } = "";
        public double Price { get; set; }

    }
}
