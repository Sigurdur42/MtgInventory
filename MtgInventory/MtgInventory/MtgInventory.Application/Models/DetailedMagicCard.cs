using System;

namespace MtgInventory.Service.Models
{
    public class DetailedMagicCard
    {
        // The internal id for the database
        public string Id { get; set; }

        public string MkmId { get; set; }

        public string NameEn { get; set; }

        public string MkmWebSite { get; set; }

        public int CountReprints { get; set; }

        public DateTime? MkmLastUpdate { get; set; }
    }
}