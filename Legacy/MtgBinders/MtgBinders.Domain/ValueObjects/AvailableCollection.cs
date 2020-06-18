using System;
using System.Linq;

namespace MtgBinders.Domain.ValueObjects
{
    public static class AvailableCollection
    {
        public static MtgRarity[] AvailableRarities()
        {
            return ((MtgRarity[])Enum.GetValues(typeof(MtgRarity)))
                .Where(r => r != MtgRarity.Unknown)
                .ToArray();
        }

        public static MtgCondition[] AvailableConditions()
        {
            return ((MtgCondition[])Enum.GetValues(typeof(MtgCondition)))
                .Where(r => r != MtgCondition.Unknown)
                .ToArray();
        }

        public static string[] AvailableLanguages()
        {
            return new[] { "EN", "DE", "ES", "PT" };
        }
    }
}