using System;
using System.Collections.Generic;

namespace MtgDatabase.Models
{
    public class QueryableMagicCard
    {
        public Guid Id { get; set; }

        public string UniqueId { get; set; } = "";
        public string Name { get; set; } = "";
        public string LocalName { get; set; } = "";
        public string TypeLine { get; set; } = "";

        public string Language { get; set; } = "";

        public ReprintInfo[] ReprintInfos { get; set; } = Array.Empty<ReprintInfo>();

        public string SetCode { get; set; } = "";

        public string OracleText { get; set; } = "";

        // TODO: Legalities
        // public Legality[] Legalities { get; set; } = Array.Empty<Legality>();
        public CardImages Images { get; set; } = new CardImages();


        public Rarity Rarity { get; set; } = Rarity.Unknown;

        public string CollectorNumber { get; set; } = "";

        public bool IsBasicLand { get; set; }

        public bool IsCreature { get; set; }
        public bool IsInstant { get; set; }
        public bool IsSorcery { get; set; }
        public bool IsArtefact { get; set; }
        public bool IsLand { get; set; }
        public bool IsToken { get; set; }
        public bool IsEmblem { get; set; }
        public bool IsEnchantment { get; set; }
        public bool IsLegendary { get; set; }
        public bool IsPlaneswalker { get; set; }
        public bool IsSnow { get; set; }
        public string SetName { get; set; } = "";

        public DateTime UpdateDateUtc { get; set; }

        public string WebSiteEdhRec { get; set; } = "";
        public string WebSiteGatherer { get; set; } = "";
        public string WebSiteScryfall { get; internal set; } = "";

        #region Price Data

        public Decimal? Usd { get; set; }

        public Decimal? UsdFoil { get; set; }

        public Decimal? Eur { get; set; }

        public Decimal? EurFoil { get; set; }

        public Decimal? Tix { get; set; }
        public string OracleId { get; internal set; } = "";
        public List<int> MultiverseIds { get; internal set; } = new List<int>();
        public bool Reserved { get; internal set; }
        public bool Oversized { get; internal set; }

        #endregion

        // TODO: More properties
    }
}