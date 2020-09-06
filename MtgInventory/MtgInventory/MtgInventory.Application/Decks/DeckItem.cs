using System.Diagnostics;

namespace MtgInventory.Service.Decks
{
    [DebuggerDisplay("{Count} {Name}")]
    public class DeckItem
    {
        public int Count { get; set; }

        public string Name { get; set; }
    }
}