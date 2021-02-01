using System;
using System.Collections.Generic;
using System.Text;
using MtgDatabase.Decks;

namespace MtgDatabase.Models
{
    public class DeckLine
    {
        public int Quantity { get; set; } = 1;
        public string CardName { get; set; } = "";

        public DeckLineType LineType { get; set; } = DeckLineType.Card;

        // TODO: Include edition, condition, etc.
    }
}
