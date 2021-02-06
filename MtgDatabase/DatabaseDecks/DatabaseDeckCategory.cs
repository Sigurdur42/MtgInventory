using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MtgDatabase.DatabaseDecks
{
    [DebuggerDisplay("{Name} {GetTotalCardCount()} cards")]
    public class DatabaseDeckCategory
    {
        public string CategoryName { get; set; } = "";

        public IList<DatabaseDeckLine> Lines { get; set; } = new List<DatabaseDeckLine>();
    }
}
