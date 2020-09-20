using System;
using MkmApi.Entities;

namespace MtgInventory.Service.Models
{
    public class DetailedSetInfo
    {
        public Guid Id { get; set; }

        public string SetCodeMkm { get; set; }

        public string SetNameMkm { get; set; }

        public string SetCodeScryfall { get; set; }

        public string SetNameScryfall { get; set; }

        public string ReleaseDate { get; set; }

        public DateTime? ReleaseDateParsed { get; set; }

        public string IsReleased { get; set; }

        public DateTime LastUpdated { get; set; }
        public string SetName { get; set; }

        internal void UpdateFromMkm(Expansion mkm)
        {
            SetCodeMkm = mkm.Abbreviation?.ToUpperInvariant();
            SetNameMkm = mkm.EnName;
            ReleaseDate = mkm.ReleaseDate;
            ReleaseDateParsed = mkm.ReleaseDateParsed;
            IsReleased = mkm.IsReleased;
            LastUpdated = DateTime.Now;

            SetName = SetNameMkm;
        }

        internal void UpdateFromScryfall(ScryfallSet scryfall)
        {
            SetCodeScryfall = scryfall.Code?.ToUpperInvariant();
            SetNameScryfall = scryfall.Name;
            LastUpdated = DateTime.Now;

            SetName = SetNameScryfall;
        }
    }
}