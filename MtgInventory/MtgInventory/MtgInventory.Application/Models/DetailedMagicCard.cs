﻿using System;
using MkmApi.Entities;
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
        public string TypeLine { get; set; }

        public string ScryfallCardSite { get; set; }

        public int CountReprints { get; set; }

        public DateTime? LastUpdateMkm { get; set; }

        public DateTime? LastUpdateScryfall { get; set; }

        public bool IsBasicLand { get; set; }
        public bool IsLand { get; set; }
        public bool IsCreature { get; set; }
        public bool IsArtifact { get; set; }
        public bool IsInstant { get; set; }
        public bool IsSorcery { get; set; }
        public bool IsToken { get; set; }

        public override string ToString()
        {
            return $"{NameEn} {SetName} [{MkmId}, {ScryfallId}]";
        }

        public void UpdateFromScryfall(Card card)
        {
            NameEn = card.Name;
            ScryfallId = card.Id;
            SetCode = card.Set;
            SetName = card.SetName;
            TypeLine = card.TypeLine;
            ScryfallCardSite = card.ScryfallUri?.ToString();
            LastUpdateScryfall = DateTime.Now;

            UpdateFromTypeLine(card.TypeLine);
        }

        public void UpdateFromTypeLine(string typeLine)
        {
            IsBasicLand = IsBasicLand || typeLine.Contains("Basic Land", StringComparison.InvariantCulture);
            IsLand = IsLand || typeLine.Contains("Land", StringComparison.InvariantCulture);
            IsArtifact = IsArtifact || typeLine.Contains("Artifact", StringComparison.InvariantCulture);
            IsCreature = IsCreature || typeLine.Contains("Creature", StringComparison.InvariantCulture);
            IsInstant = IsInstant || typeLine.Contains("Instant", StringComparison.InvariantCulture);
            IsSorcery = IsSorcery || typeLine.Contains("Sorcery", StringComparison.InvariantCulture);

            // TODO: Improve token detection
            IsToken = IsToken || typeLine.Contains("Token", StringComparison.InvariantCulture);
        }

        internal void UpdateFromMkm(MkmProductInfo card)
        {
            SetCode = card.ExpansionCode;
            SetName = card.ExpansionName;

            NameEn = card.Name;
            MkmId = card.Id;

            LastUpdateMkm = DateTime.Now;

            UpdateFromTypeLine(card.Name);
        }

        internal void UpdateFromProduct(Product card)
        {
            LastUpdateMkm = DateTime.Now;
        }
    }
}