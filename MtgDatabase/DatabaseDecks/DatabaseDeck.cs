using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MtgDatabase.DatabaseDecks
{
    [DebuggerDisplay("{CategoryName} ({Lines?.Count ?? 0})")]
    public class DatabaseDeck
    {
        public string Name { get; set; } = "";

        public IList<DatabaseDeckCategory> Categories { get; } = new List<DatabaseDeckCategory>();

        // TODO: Wanted format, etc

        public int GetTotalCardCount() => Categories.Sum(c => c.Lines.Sum(l => l.Quantity));
    }
}
