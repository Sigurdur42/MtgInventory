using System;
using System.Collections.Generic;
using System.Text;
using MtgDomain;

namespace MtgBinder.Domain.Inventory
{
    public class InventoryItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int Count { get; set; }

        public Guid CardId { get; set; }

        public string CardName { get; set; }

        public bool IsFoil { get; set; }

        public Condition Condition { get; set; }

        public Language Language { get; set; }

        public string Notes { get; set; }
    }
}
