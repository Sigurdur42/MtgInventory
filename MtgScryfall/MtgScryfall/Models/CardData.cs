using System;
using System.Collections.Generic;
using System.Text;

namespace MtgScryfall.Models
{
    public class CardData
    {
        public string UniqueId { get; set; }
        public string Name { get; set; }
        public string Rarity { get; set; }
        public string SetCode { get; set; }
        public string ManaCost { get; set; }
        public double ConvertedManaCost { get; set; }
        public string TypeLine { get; set; }
        public string OracleText { get; set; }
        public string CollectorNumber { get; set; }
        public string Layout { get; set; }
        public bool IsDigitalOnly { get; set; }

        public bool IsPauperLegal { get; set; }
        public bool IsCommanderLegal { get; set; }
        public bool IsStandardLegal { get; set; }
        public bool IsModernLegal { get; set; }
        public bool IsLegacyLegal { get; set; }
        public bool IsVintageLegal { get; set; }

        public string ImageLarge { get; set; }

        public string MkmLink { get; set; }
        public string ScryfallLink { get; set; }
        public string GathererLink { get; set; }
        public string PriceUsd { get; set; }
        public string PriceTix { get; set; }
        public string PriceEur { get; set; }
    }
}