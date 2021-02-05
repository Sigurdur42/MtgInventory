using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MtgDatabase.Models
{
    [DebuggerDisplay("{CategoryName} ({Lines?.Count ?? 0})")]
    public class DeckCategory
    {
        public string CategoryName { get; set; } = "";

        public IList<DeckLine> Lines { get; set; } = new List<DeckLine>();
    }
}
