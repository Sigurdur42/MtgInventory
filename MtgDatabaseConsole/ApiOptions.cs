using System.Collections.Generic;
using CommandLine;

namespace ScryfallApiConsole
{
    [Verb("Mirror", HelpText = "Generates a local mirror of the Scryfall database")]
    public class ApiOptions
    {
        [Option(shortName: 's', longName: "Clear", Required = false, HelpText = "Clear the Scryfall database first")]
        public bool ClearScryfall { get; set; }
        
        [Option(shortName: 'd', longName: "ClearDatabase", Required = false, HelpText = "Clear the MTG database")]
        public bool ClearMtgDatabase { get; set; }
    }
}