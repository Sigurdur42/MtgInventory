using System;
using System.Collections.Generic;
using System.Text;

namespace MtgDatabase.Models
{
    public class DeckLine
    {
        public int Quantity { get; set; }
        public string CardName { get; set; }

        // TODO: Include edition, condition, etc.
    }
}
