﻿using System;
using System.Linq;
using MkmApi.Entities;
using MtgInventory.Service.ReferenceData;

namespace MtgInventory.Service.Models
{
    public class DetailedMagicCard
    {
        // The internal id for the database
        public Guid Id { get; set; } = Guid.NewGuid();

        public string MkmId { get; set; }
        public string MkmMetaCardId { get; set; }
        public string CollectorNumber { get; set; }
        public Guid ScryfallId { get; set; }

        public string NameEn { get; set; } = "";
        public string SetCode { get; set; }
        public string SetName { get; set; }
        public string TypeLine { get; set; }

        public string ScryfallCardSite { get; set; } = "";

        public string SetCodeMkm { get; set; } = "";
        public string SetNameMkm { get; set; } = "";

        public string SetCodeScryfall { get; set; } = "";
        public string SetNameScryfall { get; set; } = "";


        public string MkmWebSite { get; set; } = "";

        public int CountReprints { get; set; }

        public DateTime? SetReleaseDate { get; set; }
        public DateTime? LastUpdateMkm { get; set; }

        public DateTime? LastUpdateScryfall { get; set; }

        public bool IsBasicLand { get; set; }
        public bool IsLand { get; set; }
        public bool IsCreature { get; set; }
        public bool IsArtifact { get; set; }
        public bool IsInstant { get; set; }
        public bool IsSorcery { get; set; }
        public bool IsToken { get; set; }

        public int PrimaryMultiverseId { get; set; }

        public int[] MultiverseIds { get; set; } = new int[0];

        public ImageLinkUri[] ScryfallImages { get; set; } = new ImageLinkUri[0];

        public ImageLinkUri[] MkmImages { get; set; } = new ImageLinkUri[0];

        public bool MkmDetailsRequired => !string.IsNullOrWhiteSpace(MkmId) && string.IsNullOrWhiteSpace(MkmWebSite);
       
        public bool IsScryfallOnly => string.IsNullOrWhiteSpace(MkmId);

        public override string ToString()
        {
            return $"{NameEn} {SetName} [{MkmId}, {ScryfallId}]";
        }

        public void UpdateFromScryfall(ScryfallCard card, DetailedSetInfo? setInfo)
        {
            NameEn = card.Name;
            ScryfallId = card.Id;
            SetCode = card.Set;
            SetName = card.SetName;
            SetCodeScryfall = card.Set;
            SetNameScryfall = card.SetName;
            TypeLine = card.TypeLine;
            ScryfallCardSite = card.ScryfallUri?.ToString() ?? "";
            LastUpdateScryfall = DateTime.Now;
            CollectorNumber = card.CollectorNumber;
            MultiverseIds = card.MultiverseIds;
            PrimaryMultiverseId = card.MultiverseIds.FirstOrDefault();
            ScryfallImages = card.Images;

            UpdateFromTypeLine(card.TypeLine);

            if (setInfo?.ReleaseDateParsed != null)
            {
                SetReleaseDate = setInfo?.ReleaseDateParsed;
            }
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

        internal CardReferenceData GenerateReferenceData()
        {
            return new CardReferenceData()
            {
                MkmId = MkmId,
                Name = NameEn,
                MkmWebSite = MkmWebSite,
                ScryfallId = ScryfallId,
                SetCodeMkm = SetCodeMkm,
                Id = Id,
                MkmImageUrl = MkmImages?.FirstOrDefault()?.Uri ?? "",
            };
        }

        internal void UpdateFromMkm(MkmProductInfo card, DetailedSetInfo setInfo)
        {
            SetCode = card.ExpansionCode;
            SetName = card.ExpansionName;

            SetCodeMkm = card.ExpansionCode;
            SetNameMkm = card.ExpansionName;

            NameEn = card.Name;
            MkmId = card.Id;
            MkmMetaCardId = card.MetacardId;

            LastUpdateMkm = DateTime.Now;

            if (setInfo?.ReleaseDateParsed != null)
            {
                SetReleaseDate = setInfo?.ReleaseDateParsed;
            }

            UpdateFromTypeLine(card.Name);
        }

        internal void UpdateFromMkm(Product product)
        {
            MkmWebSite = "http://www.cardmarket.com" + product.WebSite;
            MkmImages = new[]
            {
                new ImageLinkUri()
                {
                    Category="",
                    Uri = "http:" + product.Image,
                }
            };
        }
    }
}