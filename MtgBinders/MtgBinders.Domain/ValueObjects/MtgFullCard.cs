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

        public string ManaCost { get; set; }
        public double ConvertedManaCost { get; set; }
        public string TypeLine { get; set; }
        public string OracleText { get; set; }
        public string CollectorNumber { get; set; }
        public bool IsDigitalOnly { get; set; }

        public string ImageLarge { get; set; }
        public string Layout { get; set; }
    }
}