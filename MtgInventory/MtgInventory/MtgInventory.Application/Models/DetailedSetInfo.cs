using System;
using System.Diagnostics;
using MkmApi.Entities;

namespace MtgInventory.Service.Models
{
    [DebuggerDisplay("{SetCode} {SetName}")]
    public class DetailedSetInfo
    {
        public Guid Id { get; set; }

        public string SetCode { get; set; }

        public string SetCodeMkm { get; set; }

        public string SetNameMkm { get; set; }

        public string SetCodeScryfall { get; set; }

        public string SetNameScryfall { get; set; }

        public string ReleaseDate { get; set; }

        public DateTime? ReleaseDateParsed { get; set; }

        public string IsReleased { get; set; }

        public DateTime LastUpdated { get; set; }
        public string SetName { get; set; }

        /// <summary>
        /// Does this artifical set code only contains tokens?
        /// </summary>
        public bool IsTokenSet { get; set; }

        internal void UpdateFromMkm(string normalizedSetCode, Expansion mkm)
        {
            SetCode = normalizedSetCode;
            SetCodeMkm = mkm.Abbreviation?.ToUpperInvariant() ?? "";
            SetNameMkm = mkm.EnName;
            ReleaseDate = mkm.ReleaseDate;
            ReleaseDateParsed = mkm.ReleaseDateParsed;
            IsReleased = mkm.IsReleased;
            LastUpdated = DateTime.Now;

            if (SetNameMkm.Contains("Token", StringComparison.InvariantCultureIgnoreCase))
            {
                IsTokenSet = true;
            }

            SetName ??= SetNameMkm;
        }

        internal void UpdateFromScryfall(string normalizedSetCode, ScryfallSet scryfall)
        {
            SetCode = normalizedSetCode;
            SetCodeScryfall = scryfall.Code?.ToUpperInvariant() ?? "";
            SetNameScryfall = scryfall.Name;
            LastUpdated = DateTime.Now;

            SetName ??= SetNameScryfall;

            if (scryfall.Name.Contains("Token", StringComparison.InvariantCultureIgnoreCase))
            {
                IsTokenSet = true;
            }
        }
    }
}