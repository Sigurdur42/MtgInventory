using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MtgDatabase.Models;

namespace MtgDatabase.DatabaseDecks
{
    [DebuggerDisplay("{Quantity} {CardName} ({LineType})")]
    public class DatabaseDeckLine
    {
        public int Quantity { get; set; }

        public QueryableMagicCard? Card { get; set; }
    }
}
