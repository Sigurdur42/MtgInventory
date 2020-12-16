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
        Plane
    }

    public static class RarityConverter
    {
        public static Rarity ToRarity(this string value, string? argTypeLine)
        {
            if ((argTypeLine?.Contains("Basic Land", StringComparison.InvariantCultureIgnoreCase) ?? false)
                || (argTypeLine?.Contains("Basic Snow Land", StringComparison.InvariantCultureIgnoreCase) ?? false))
            {
                return Rarity.BasicLand;
            }

            if (argTypeLine?.StartsWith("Plane ", StringComparison.InvariantCultureIgnoreCase) ?? false)
            {
                return Rarity.Plane;
            }

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