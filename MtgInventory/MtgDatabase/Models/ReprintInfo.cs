namespace MtgDatabase.Models
{
    public class ReprintInfo
    {
        public string SetCode { get; set; } = "";

        public Rarity Rarity { get; set; } = Rarity.Unknown;

        public string CollectorNumber { get; set; } = "";
        // TODO: Image link
    }
}