using System;

namespace MtgDatabase.Models
{
    public enum LegalityState
    {
        Unknown = 0,
        Legal,
        NotLegal,
        Banned,
        Restricted
    }

    public static class LegalityStateConverter
    {
        public static LegalityState ToLegalityState(this string value) =>
            value?.ToUpperInvariant() switch
            {
                "NOT_LEGAL" => LegalityState.NotLegal,
                "LEGAL" => LegalityState.Legal,
                "BANNED" => LegalityState.Banned,
                "RESTRICTED" => LegalityState.Restricted,
                _ => throw new InvalidCastException($"Cannot convert value {value} to legality")
            };
    }
}