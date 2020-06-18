using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MtgBinder.Domain.Decks
{
    [DebuggerDisplay("{Count} {Name}")]
    public class DeckItem
    {
        public int Count { get; set; }

        public string Name { get; set; }
    }
}
