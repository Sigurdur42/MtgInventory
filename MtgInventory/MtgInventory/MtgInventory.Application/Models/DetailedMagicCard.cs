using System;
using ScryfallApi.Client.Models;

namespace MtgInventory.Service.Models
{
    public class DetailedMagicCard
    {
        // The internal id for the database
        public Guid Id { get; set; }

        public string MkmId { get; set; }

        public Guid ScryfallId { get; set; }

        public string NameEn { get; set; }
        public string SetCode { get; set; }
        public string SetName { get; set; }

        public string MkmWebSite { get; set; }

        public int CountReprints { get; set; }

        public DateTime? LastUpdateMkm { get; set; }
        public DateTime? LastUpdateScryfall { get; set; }

        public void UpdateFromScryfall(Card card)
        {
            ScryfallId = card.Id;
            SetCode = card.Set;
            SetName = card.SetName;
            LastUpdateScryfall = DateTime.Now;
        }

        internal void UpdateFromMkm(MkmProductInfo card)
        {
            MkmId = card.Id;
            MkmWebSite = card.MkmProductUrl;

            LastUpdateMkm = DateTime.Now;
        }
    }
}