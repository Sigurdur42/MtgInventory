using System;

namespace MtgDatabase.Models
{
    public class QueryableMagicCard
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = "";

        public ReprintInfo[] ReprintInfos { get; set; } = Array.Empty<ReprintInfo>();

        public Legality[] Legalities { get; set; } = Array.Empty<Legality>();
        
        // TODO: More properties
    }
}