using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LiteDB;
using MtgDomain;

namespace MtgBinder.Domain.Decks
{
    [DebuggerDisplay("{Name}")]
    public class DeckList
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public Format Format { get; set; }

        public List<DeckItem> Mainboard { get; set; } = new List<DeckItem>();
        public List<DeckItem> Sideboard { get; set; } = new List<DeckItem>();

        [BsonIgnore]
        public int TotalCards => Mainboard.Sum(c => c.Count) + Sideboard.Sum(c => c.Count);
    }
}