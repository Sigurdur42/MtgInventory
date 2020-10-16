using System;

namespace MtgInventory.Service.ReferenceData
{
    public class CardReferenceData
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string MkmId { get; set; } = "";

        public Guid ScryfallId { get; set; } = Guid.Empty;

        public string Name { get; set; } = "";

        public string SetCodeMkm { get; set; } = "";

        public string MkmWebSite { get; set; } = "";

        public string MkmImageUrl { get; set; } = "";
    }
}