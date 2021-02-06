using System;
using System.Collections.Generic;
using System.Text;

namespace MtgDatabase.DatabaseDecks
{
    public class DatabaseDeckReaderResult
    {
        public string Name { get; set; } = "";
        public DatabaseDeck Deck { get; set; } = new DatabaseDeck();

        public DatabaseDeckErrorLine[] UnreadLines { get; set; } = new DatabaseDeckErrorLine[0];
    }
}
