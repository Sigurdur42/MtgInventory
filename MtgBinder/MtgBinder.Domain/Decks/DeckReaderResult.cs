using System;
using System.Collections.Generic;
using System.Text;

namespace MtgBinder.Domain.Decks
{
    public class DeckReaderResult
    {
        public DeckList Deck { get; set; }

        public string[] UnreadLines { get; set; }
    }
}
