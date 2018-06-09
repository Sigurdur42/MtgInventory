using System;
using System.Collections.Generic;
using System.Text;

namespace MtgBinders.Domain.ValueObjects
{
    public sealed class MtgFullCard
    {
        public string Name { get; set; }
        public string SetCode { get; set; }
        public MtgRarity Rarity { get; set; }
    }
}