using System.Linq;
using MtgInventory.Service.Models;

namespace MtgInventory.Service.Database
{
    public class CardReferenceData
    {
        public CardReferenceData()
        {
        }

        public CardReferenceData(DetailedMagicCard c)
        {
            MkmId = c.MkmId;
            MkmImageUrl = c.MkmImages.FirstOrDefault()?.Uri?.ToString() ?? "";
            MkmWebSite = c.MkmWebSite;
            Name = c.NameEn;
            ScryfallSetCode = c.SetCodeScryfall;
            ScryfallCollectorNumber = c.CollectorNumber;
        }

        public string MkmId { get; set; } = "";
        public string MkmImageUrl { get; set; } = "";
        public string MkmWebSite { get; set; } = "";
        public string Name { get; set; } = "";
        public string ScryfallCollectorNumber { get; set; } = "";
        public string ScryfallSetCode { get; set; } = "";

        public string GetScryfallIndexKey() => MakeScryfallKey(ScryfallCollectorNumber, ScryfallSetCode);

        internal static string MakeScryfallKey(
            string collectorNumber,
            string scryfallSetCode) => $"{collectorNumber}_{scryfallSetCode}";
    }
}