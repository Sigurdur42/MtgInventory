using System.Diagnostics;

namespace MkmApi.Entities
{
    /// <summary>
    /// See https://api.cardmarket.com/ws/documentation/API_2.0:Entities:Game
    /// </summary>
    [DebuggerDisplay("{IdGame} {Name}")]
    public class Game
    {
        ////idGame:                     // game ID
        ////name:                       // game's name
        ////abbreviation                // the game's abbreviation
        ///
        public int IdGame { get; set; }

        public string Name { get; set; } = "";
        public string Abbreviation { get; set; } = "";
    }
}