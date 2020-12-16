using System;
using System.Globalization;

namespace MtgInventory.Service.Converter
{
    public static class NumberFormatter
    {
        internal static DateTime? ParseDateTime(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                return parsed;
            }

            return null;
        }
    }
}