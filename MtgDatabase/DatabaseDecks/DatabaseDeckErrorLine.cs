using System;
using System.Collections.Generic;
using System.Text;

namespace MtgDatabase.DatabaseDecks
{
    public enum DeckErrorLineReason
    {
        CannotParse=0,
        CannotFindCardInDatabase=1,
    }

    public class DatabaseDeckErrorLine
    {
        public string Line { get; set; } = "";

        public DeckErrorLineReason Reason { get; set; } = DeckErrorLineReason.CannotParse;
    }
}
