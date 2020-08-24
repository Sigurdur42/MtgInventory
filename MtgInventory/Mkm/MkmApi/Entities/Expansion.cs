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
        public string Abbreviation { get; set; }
        public string Icon { get; set; }
        public string ReleaseDate { get; set; }
        public string IsReleased { get; set; }
        public int IdGame { get; set; }
    }
}