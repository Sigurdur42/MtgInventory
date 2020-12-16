using System;
using System.Diagnostics;

namespace MtgInventory.Service.Decks
{
    [DebuggerDisplay("{Count} {Name}")]
    public class DeckItem
    {
        public int Count { get; set; }

        public string Name { get; set; } = "";

        public string SetCode { get; set; } = "";

        public string SetName { get; set; } = "";

        public Guid CardId { get; set; }

        public bool IsEmpty => string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(SetCode) && string.IsNullOrEmpty(SetName);
    }
}