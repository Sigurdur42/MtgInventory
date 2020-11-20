using System;
using System.Linq;
using MkmApi.Entities;
using MtgInventory.Service.Database;

namespace MtgInventory.Service.Models
{
    public class DetailedMagicCard
    {
        public string CollectorNumber { get; set; } = "";

        public int CountReprints { get; set; }

        // The internal id for the database
        public Guid Id { get; set; } = Guid.NewGuid();

        public bool IsArtifact { get; set; }
        public bool IsBasicLand { get; set; }
        public bool IsCreature { get; set; }
        public bool IsInstant { get; set; }
        public bool IsLand { get; set; }
        public bool IsMappedByReferenceCard { get; set; }
        public bool IsMkmOnly { get; set; }
        public bool IsScryfallOnly { get; set; }
        public bool IsSorcery { get; set; }
        public bool IsToken { get; set; }
        public bool IsEmblem { get; set; }
        public bool IsPunchCard { get; set; }
        public bool IsTipCard { get; set; }
        public bool IsOnlineCode { get; set; }
        public bool IsCheckList { get; set; }

        public DateTime? LastUpdateMkm { get; set; }
        public DateTime? LastUpdateScryfall { get; set; }
        public CardMigrationStatus MigrationStatus { get; set; } = CardMigrationStatus.Unknown;
        public bool MkmDetailsRequired => !string.IsNullOrWhiteSpace(MkmId) && string.IsNullOrWhiteSpace(MkmWebSite);
        public string MkmId { get; set; } = "";
        public ImageLinkUri[] MkmImages { get; set; } = new ImageLinkUri[0];
        public string MkmMetaCardId { get; set; } = "";
        public string MkmWebSite { get; set; } = "";
        public int[] MultiverseIds { get; set; } = new int[0];
        public string NameEn { get; set; } = "";
        public int PrimaryMultiverseId { get; set; }
        public string ScryfallCardSite { get; set; } = "";
        public Guid ScryfallId { get; set; }
        public ImageLinkUri[] ScryfallImages { get; set; } = new ImageLinkUri[0];
        public string SetCode { get; set; } = "";
        public string SetCodeMkm { get; set; } = "";
        public string SetCodeScryfall { get; set; } = "";
        public string SetName { get; set; } = "";
        public string SetNameMkm { get; set; } = "";
        public string SetNameScryfall { get; set; } = "";
        public DateTime? SetReleaseDate { get; set; }
        public string TypeLine { get; set; } = "";

        public override string ToString()
        {
            return $"{NameEn} {SetName} [{MkmId}, {ScryfallId}]";
        }

        public void UpdateFromScryfall(ScryfallCard card, DetailedSetInfo? setInfo)
        {
            NameEn = string.IsNullOrEmpty(NameEn) ? card.Name : NameEn;
            ScryfallId = card.Id;
            SetCode = string.IsNullOrEmpty(SetCode) ? card.Set : SetCode;
            SetName = string.IsNullOrEmpty(SetName) ? card.SetName : SetName;
            SetCodeScryfall = card.Set;
            SetNameScryfall = card.SetName;
            TypeLine = card.TypeLine;
            ScryfallCardSite = card.ScryfallUri?.ToString() ?? "";
            LastUpdateScryfall = DateTime.Now;
            CollectorNumber = card.CollectorNumber;
            MultiverseIds = card.MultiverseIds;
            PrimaryMultiverseId = card.MultiverseIds.FirstOrDefault();
            ScryfallImages = card.Images;

            UpdateFromName(card.Name);
            UpdateFromTypeLine(card.TypeLine);

            if (setInfo?.ReleaseDateParsed != null)
            {
                SetReleaseDate = setInfo?.ReleaseDateParsed;
            }

            if (setInfo != null)
            {
                IsMkmOnly = setInfo.IsKnownMkmOnlySet;
                IsScryfallOnly = setInfo.IsKnownScryfallOnlySet;
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
            IsEmblem = IsEmblem || typeLine.EndsWith(" Emblem", StringComparison.InvariantCulture);

            // TODO: Improve token detection
            IsToken = IsToken || typeLine.Contains("Token", StringComparison.InvariantCulture);
        }

        public void UpdateFromName(string name)
        {
            UpdateToken(name);
            IsEmblem = IsEmblem || name.EndsWith(" Emblem", StringComparison.InvariantCulture);
            IsPunchCard = IsPunchCard || name.EndsWith(" Punch Card", StringComparison.InvariantCulture);
            IsTipCard = IsTipCard 
                        || name.StartsWith("Tip: ", StringComparison.InvariantCulture)
                        || name.StartsWith("Rules Tip: ", StringComparison.InvariantCulture)
                ;
            IsOnlineCode = IsOnlineCode
                           || name.StartsWith("Magic Online Code", StringComparison.InvariantCulture)
                           || name.StartsWith("Arena Code ", StringComparison.InvariantCulture)
                ;

            IsCheckList = IsCheckList
                          || name.EndsWith("Checklist", StringComparison.InvariantCulture)
                ;

        }

        public void UpdateToken(string name)
        {
            IsToken = IsToken
                || name.Contains("Token", StringComparison.InvariantCulture)
                || name.Equals("Energy Reserve", StringComparison.InvariantCultureIgnoreCase)
                || name.Equals("The Monarch", StringComparison.InvariantCultureIgnoreCase)
                || name.Equals("Poison Counter", StringComparison.InvariantCultureIgnoreCase)
                || name.Equals("Experience Counter", StringComparison.InvariantCultureIgnoreCase)
                ;
        }

        public void UpdateManualMapped(CardReferenceData reference)
        {
            MkmWebSite = reference.MkmWebSite;
            MkmImages = new[]{new ImageLinkUri()
            {
                Uri = reference.MkmImageUrl,
                Category = "normal"
            }};

            IsMappedByReferenceCard = true;
        }

        internal CardReferenceData GenerateReferenceData()
        {
            return new CardReferenceData()
            {
                MkmId = MkmId,
                Name = NameEn,
                MkmWebSite = MkmWebSite,
                ScryfallCollectorNumber = CollectorNumber,
                ScryfallSetCode = SetCodeScryfall,
                ScryfallId = ScryfallId,
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

            UpdateFromName(card.Name);
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