namespace MtgDatabase.Models
{
    public class ReprintInfo
    {
        public string SetCode { get; set; } = "";
        public string SetName { get; set; } = "";

        public Rarity Rarity { get; set; } = Rarity.Unknown;

        public string CollectorNumber { get; set; } = "";

        public CardImages Images { get; set; } = new CardImages();
    }
}