using System;
using System.Collections.Generic;
using System.Text;

namespace MtgDatabase.Models
{
    public class Deck
    {
        public string Name { get; set; } = "";

        public IList<DeckCategory> Categories { get; } = new List<DeckCategory>();

        // TODO: Wanted format, etx
    }
}
