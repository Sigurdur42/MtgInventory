using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MtgDatabase.Models
{
    [DebuggerDisplay("{Name} {GetTotalCardCount()} cards")]
    public class Deck
    {
        public string Name { get; set; } = "";

        public IList<DeckCategory> Categories { get; } = new List<DeckCategory>();

        // TODO: Wanted format, etx

        public int GetTotalCardCount() => Categories.Sum(c => c.Lines.Sum(l => l.Quantity));
    }
}
