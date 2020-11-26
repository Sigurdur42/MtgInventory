using System;

namespace MtgDatabase.Models
{
    public enum SanctionedFormat
    {
        Unknown=0,
        Pauper,
        Commander,
        Standard,
        Modern,
        Legacy,
        Brawl,
        Duel,
        Future,
        Penny,
        OldSchool,
        Historic,
        Vintage,
        Pioneer
    }
    
    public static class SanctionedFormatConverter
    {
        public static SanctionedFormat ToSanctionedFormat(this string value)
        {
            var result = value?.ToUpperInvariant() switch
            {
                "STANDARD" => SanctionedFormat.Standard,
                "PAUPER" => SanctionedFormat.Pauper,
                "COMMANDER" => SanctionedFormat.Commander,
                "MODERN" => SanctionedFormat.Modern,
                "LEGACY" => SanctionedFormat.Legacy,
                "BRAWL" => SanctionedFormat.Brawl,
                "DUEL" => SanctionedFormat.Duel,
                "FUTURE" => SanctionedFormat.Future,
                "PENNY" => SanctionedFormat.Penny,
                // ReSharper disable once StringLiteralTypo
                "OLDSCHOOL" => SanctionedFormat.OldSchool,
                "HISTORIC" => SanctionedFormat.Historic,
                "VINTAGE" => SanctionedFormat.Vintage,
                "PIONEER" => SanctionedFormat.Pioneer,
                _ =>  SanctionedFormat.Unknown // throw new InvalidCastException($"Cannot convert value {value} to sanctioned format")
            };

            if (result == SanctionedFormat.Unknown)
            {
                Console.WriteLine($"Cannot convert value {value} to sanctioned format");
            }

            return result;
        }
    }
}