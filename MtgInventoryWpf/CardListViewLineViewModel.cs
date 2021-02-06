using System;
using System.Collections.Generic;
using System.Text;
using MtgDatabase.Models;
using PropertyChanged;

namespace MtgInventoryWpf
{
    [AddINotifyPropertyChangedInterface]
    public class CardListViewLineViewModel
    {
        public int Quantity { get; set; }

        public string Category { get; set; } = "";

        public QueryableMagicCard? Card { get; set; }
    }
}
