using System.Globalization;

namespace MkmApi.EntityReader
{
    internal static class Converters
    {
        public static int ToInt(this string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return -1;
            }

            if (int.TryParse(content, NumberStyles.Any, CultureInfo.InvariantCulture, out var converted))
            {
                return converted;
            }

            return -1;
        }

        public static decimal? ToDecimal(this string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            if (decimal.TryParse(content, NumberStyles.Any, CultureInfo.InvariantCulture, out var converted))
            {
                return converted;
            }

            return null;
        }
    }
}