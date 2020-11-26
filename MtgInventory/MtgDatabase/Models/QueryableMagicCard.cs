using System;
using ScryfallApiServices.Models;

namespace MtgDatabase.Models
{
    public class QueryableMagicCard
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = "";
        public string TypeLine { get; set; } = "";

        public ReprintInfo[] ReprintInfos { get; set; } = Array.Empty<ReprintInfo>();

        public Legality[] Legalities { get; set; } = Array.Empty<Legality>();
        
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

        // TODO: More properties
    }
}