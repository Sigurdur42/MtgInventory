using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtgBinders.Domain.ValueObjects
{
    public static class RarityExtension
    {
        public static MtgRarity ToMtgRarity(this string rarity, ILogger logger)
        {
            switch (rarity?.ToLowerInvariant())
            {
                case "common":
                    return MtgRarity.Common;

                case "uncommon":
                    return MtgRarity.Uncommon;

                case "rare":
                    return MtgRarity.Rare;

                case "mythic":
                    return MtgRarity.Mythic;

                default:
                    logger?.LogWarning($"Cannot convert '{rarity}' to MtgRarity.");
                    return MtgRarity.Unknown;
            }
        }
    }
}