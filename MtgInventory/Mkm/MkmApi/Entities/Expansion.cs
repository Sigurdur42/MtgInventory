using System;
using System.Text.RegularExpressions;

namespace MkmApi.Entities
{
    public class Expansion
    {
        /*
         expansion:
        idExpansion:                // Expansion's ID
        enName:                     // Expansion's name in English
        localization: {}            // localization entities for the expansion's name
        abbreviation:               // the expansion's abbreviation
        icon:                       // Index of the expansion's icon
        releaseDate:                // the expansion's release date
        isReleased:                 // true|false; a flag if the expansion is released yet
        idGame:                     // the game ID the expansion belongs to
        links: {}                   // HATEOAS links
         * */

        public int IdExpansion { get; set; }
        public string EnName { get; set; }
        public string Abbreviation { get; set; } = "";
        public string Icon { get; set; }
        public string ReleaseDate { get; set; }
        public string IsReleased { get; set; }
        public int IdGame { get; set; }

        public DateTime? ReleaseDateParsed { get; set; }

        public bool IsMkmOnlySet
        {
            get
            {
                if (Regex.IsMatch(Abbreviation, @"^TOK\d+$", RegexOptions.IgnoreCase))
                {
                    // Known artist token sets are prefixed with TOK
                    return true;
                }

                switch (Abbreviation.ToUpperInvariant())
                {
                    case "MKMT":
                    case "CGT":
                    case "MKM1":
                    case "CDZP":
                    case "JAT":
                    case "JH02":
                    case "JH03":
                    case "JVT":
                    case "SCT1":
                    case "SCT2":
                    case "SCT3":
                    case "SCT4":
                    case "SCCC":
                    case "MSTK":
                    case "TOKY":
                    case "TDPR":
                    case "UPPC":
                    case "YMGT":
                    case "YUM":
                        return true;

                    // TODO: Add only sets
                    default:
                        return false;
                }
            }
        }
    }
}