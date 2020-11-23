namespace MtgDatabase.Models
{
    public class ReprintInfo
    {
        public string SetCode { get; set; } = "";

        public Rarity Rarity { get; set; } = Rarity.Unknown;
        
        // TODO: Image link
    }
}