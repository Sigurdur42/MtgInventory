using System;

namespace MtgDatabase.Models
{
    public enum Rarity
    {
        Unknown = 0,
        Common,
        Uncommon,
        Rare,
        Mythic,
        BasicLand,
        Token,
    }
    
    public static class RarityConverter
    {
        public static Rarity ToRarity(this string value)
        {
            return value?.ToUpperInvariant() switch
            {
                "COMMON" => Rarity.Common,
                "UNCOMMON" => Rarity.Uncommon,
                "RARE" => Rarity.Rare,
                "MYTHIC" => Rarity.Mythic,
                "TOKEN" => Rarity.Token,
                _ => throw new InvalidCastException($"Cannot convert value {value?.ToUpperInvariant()} to Rarity")
            };
        }
    }
}