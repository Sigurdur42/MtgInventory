using System;
using System.Collections.Generic;
using System.Text;
using MkmApi.Entities;
using ScryfallApi.Client.Models;

namespace MtgInventory.Service.Models
{
    // TODO: Implement this like cards

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

        internal void UpdateFromMkm(Expansion mkm)
        {
            SetCodeMkm = mkm.Abbreviation;
            SetNameMkm = mkm.EnName;
            ReleaseDate = mkm.ReleaseDate;
            ReleaseDateParsed = mkm.ReleaseDateParsed;
            IsReleased = mkm.IsReleased;
            LastUpdated = DateTime.Now;
       }

        internal void UpdateFromScryfall(Set scryfall)
        {
            SetCodeScryfall = scryfall.Code;
            SetNameScryfall = scryfall.Name;
            LastUpdated = DateTime.Now;
        }
    }
}
