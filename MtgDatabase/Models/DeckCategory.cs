using System;
using System.Collections.Generic;
using System.Text;

namespace MtgDatabase.Models
{
    public class DeckCategory
    {
        public string CategoryName { get; set; } = "";

        public IList<DeckLine> Lines { get; set; } = new List<DeckLine>();
    }
}
