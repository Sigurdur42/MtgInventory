using System;
using System.Diagnostics;
using MkmApi.Entities;

namespace MtgInventory.Service.Models
{
    [DebuggerDisplay("{SetCode} {SetName}")]
    public class DetailedSetInfo
    {
        public Guid Id { get; set; }

        public string SetCode { get; set; } = "";

        public string SetCodeMkm { get; set; } = "";

        public string SetNameMkm { get; set; } = "";

        public string SetCodeScryfall { get; set; } = "";

        public string SetNameScryfall { get; set; } = "";

        public string ReleaseDate { get; set; } = "";

        public DateTime? ReleaseDateParsed { get; set; }

        public string IsReleased { get; set; } = "";

        public DateTime SetLastUpdated { get; set; } = DateTime.Now.AddDays(-1000);

        public DateTime SetLastDownloaded { get; set; } = DateTime.Now.AddDays(-1000);

        public DateTime CardsLastUpdated { get; set; } = DateTime.Now.AddDays(-1000);

        public DateTime CardsLastDownloaded { get; set; } = DateTime.Now.AddDays(-1000);

        public string SetName { get; set; } = "";

        public int ScryfallCardCount { get; set; } = -1;
        
        public int MkmCardCount { get; set; } = -1;

        /// <summary>
        /// Does this artifical set code only contains tokens?
        /// </summary>
        public bool IsTokenSet { get; set; }

        public bool IsKnownScryfallOnlySet { get; set; }

        public bool IsKnownMkmOnlySet { get; set; }

        internal void UpdateFromMkm(string normalizedSetCode, Expansion mkm)
        {
            SetCode = normalizedSetCode;
            SetCodeMkm = mkm.Abbreviation?.ToUpperInvariant() ?? "";
            SetNameMkm = mkm.EnName;
            ReleaseDate = mkm.ReleaseDate;
            ReleaseDateParsed = mkm.ReleaseDateParsed;
            IsReleased = mkm.IsReleased;
            SetLastUpdated = DateTime.Now;
            SetLastDownloaded = DateTime.Now;
            IsKnownMkmOnlySet = mkm.IsMkmOnlySet;

            if (SetNameMkm.Contains("Token", StringComparison.InvariantCultureIgnoreCase))
            {
                IsTokenSet = true;
            }

            SetName = string.IsNullOrWhiteSpace(SetName) ? SetNameMkm: SetName;
        }

        internal void UpdateFromScryfall(string normalizedSetCode, ScryfallSet scryfall)
        {
            SetCode = normalizedSetCode;
            SetCodeScryfall = scryfall.Code?.ToUpperInvariant() ?? "";
            SetNameScryfall = scryfall.Name;
            SetLastUpdated = DateTime.Now;
            SetLastDownloaded = DateTime.Now;
            IsKnownScryfallOnlySet = scryfall.IsScryfallOnlySet;
            ScryfallCardCount = scryfall.card_count;

            SetName = string.IsNullOrWhiteSpace(SetName) ? SetNameScryfall : SetName;

            if (scryfall.Name.Contains("Token", StringComparison.InvariantCultureIgnoreCase))
            {
                IsTokenSet = true;
            }
        }
    }
}